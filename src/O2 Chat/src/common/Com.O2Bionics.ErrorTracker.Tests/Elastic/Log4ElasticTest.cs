using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Com.O2Bionics.Utils;
using Com.O2Bionics.Utils.Network;
using Jil;
using log4net;
using NUnit.Framework;
using pair = System.Collections.Generic.KeyValuePair<string, string>;

namespace Com.O2Bionics.ErrorTracker.Tests.Elastic
{
    [TestFixture]
    public sealed class Log4ElasticTest : BaseElasticTest
    {
        private const int TimeoutMilliseconds = 20 * 1000;
        private const uint CustomerId = 1234567890;
        private const uint UserId = 2345678908;
        private const long VisitorId = 34567890123456789;
        private const string ClientIp = "127.1.2.3", Url = "http://some/address", UserAgent = "A browser";
        private const string SuperError = "SuperError";

        [Test]
        [MaxTime(TimeoutMilliseconds)]
        public async Task WriteViaLog4AndThenReadViaElastic()
        {
            var exception = M4();

            var now = DateTime.UtcNow;
            var time = now.ToString(CultureInfo.InvariantCulture);
            var log = LogManager.GetLogger(typeof(Log4ElasticTest));

            const int messageCount = 2;
            var message1 = "MessageOne" + time;
            var message2 = "MessageTwo" + time;
            var headers = CreateHeaders();
            using (WcfHelper.CreateContext(headers))
                log.Error(message1, exception);
            log.Error(message2);

            var actuals = await RetrieveErrorInfos(messageCount);
            Assert.IsNotNull(actuals, "Returned errorInfos");
            Assert.AreEqual(messageCount, actuals.Count, "errorInfos.Length");

            if (null == actuals[1].ExceptionMessage)
            {
                var t = actuals[0];
                actuals[0] = actuals[1];
                actuals[1] = t;
            }

            var expected = BuildExpected(message1, message2, exception);
            for (int i = 0; i < messageCount; i++)
            {
                //Time might differ by several milliseconds.
                AreClose(now, actuals[i].Timestamp, i.ToString());
                expected[i].Timestamp = actuals[i].Timestamp;

                var exp = JSON.Serialize(expected[i], JsonSerializerBuilder.DefaultJilOptions);
                var act = JSON.Serialize(actuals[i], JsonSerializerBuilder.DefaultJilOptions);
                Assert.AreEqual(exp, act, $"ErrorInfos[{i}].");
            }
        }

        private static List<pair> CreateHeaders()
        {
            var headers = new List<pair>
                {
                    new pair(ServiceConstants.CustomerId, CustomerId.ToString()),
                    new pair(ServiceConstants.UserId, UserId.ToString()),
                    new pair(ServiceConstants.VisitorId, VisitorId.ToString()),

                    new pair(ServiceConstants.ClientIp, ClientIp),
                    new pair(ServiceConstants.Url, Url),
                    new pair(ServiceConstants.UserAgent, UserAgent)
                };
            return headers;
        }

        private async Task<List<ErrorInfo>> RetrieveErrorInfos(int messageCount)
        {
            var client = GetClient();
            for (int i = 0; i < TimeoutMilliseconds; ++i)
            {
                var searchResponse = await client.SearchAsync<ErrorInfo>(
                    IndexName,
                    s => s
                        .Index(IndexName)
                        .From(0)
                        .Size(messageCount + 1)
                        .Query(q => q.Match(m => m.Field(f => f.Application).Query(TestConstants.ApplicationName))));

                if (searchResponse.Documents.Count < messageCount)
                {
//It takes some time for log4net to write to the Elastic.
                    Thread.Sleep(1);
                    continue;
                }

                return searchResponse.Documents.ToList();
            }

            throw new Exception("Elastic must have returned the documents.");
        }

        private static ErrorInfo[] BuildExpected(string message1, string message2, Exception exception)
        {
            var assemblyName = Assembly.GetAssembly(typeof(Log4ElasticTest))?.GetName().Name;
            Assert.That(assemblyName, Is.Not.Null, nameof(assemblyName));
            Assert.That(assemblyName, Is.Not.Empty, nameof(assemblyName));

            var hostName = Dns.GetHostName();
            Assert.That(hostName, Is.Not.Null, nameof(hostName));
            Assert.That(hostName, Is.Not.Empty, nameof(hostName));

            var exceptionStack = GetExceptionStack(exception);

            var expected = new[]
                {
                    new ErrorInfo
                        {
                            Application = TestConstants.ApplicationName,
                            LoggerName = typeof(Log4ElasticTest).FullName,
                            Message = message2,
                            HostName = hostName
                        },
                    new ErrorInfo
                        {
                            Application = TestConstants.ApplicationName,
                            CustomerId = CustomerId,
                            UserId = UserId,
                            VisitorId = VisitorId,
                            ExceptionMessage = SuperError,
                            ExceptionSource = assemblyName,
                            ExceptionStack = exceptionStack,
                            ExceptionType = typeof(AggregateException).FullName,
                            LoggerName = typeof(Log4ElasticTest).FullName,
                            Message = message1,
                            HostName = hostName,
                            ClientIp = ClientIp,
                            Url = Url,
                            UserAgent = UserAgent
                        }
                };
            return expected;
        }

        private static string GetExceptionStack(Exception exception)
        {
            var exceptionStack = exception.ToString();
            var expectedStrings = new[]
                {
                    "System.AggregateException: SuperError",
                    "at Com.O2Bionics.ErrorTracker.Tests.Elastic.Log4ElasticTest.M1(Int32 value) in",
                    "(Inner Exception #0) System.ArgumentException: TestMethodOne1",
                    "(Inner Exception #1) System.Exception: TestMethodTwo ---> System.ArgumentException: TestMethodOne2"
                };

            foreach (var expected in expectedStrings)
            {
                var index = exceptionStack.IndexOf(expected, StringComparison.Ordinal);
                Assert.GreaterOrEqual(index, 0, expected);
            }
            return exceptionStack;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void M1(int value)
        {
            throw new ArgumentException("TestMethodOne" + value.ToString());
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void M2(int value)
        {
            try
            {
                M1(value);
            }
            catch (Exception e)
            {
                throw new Exception("TestMethodTwo", e);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void M3()
        {
            var exceptions = new Exception[2];
            try
            {
                M1(1);
            }
            catch (Exception e)
            {
                exceptions[0] = e;
            }

            try
            {
                M2(2);
            }
            catch (Exception e)
            {
                exceptions[1] = e;
            }

            throw new AggregateException(SuperError, exceptions);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Exception M4()
        {
            Exception result = null;
            try
            {
                M3();
            }
            catch (Exception e)
            {
                result = e;
            }

            return result;
        }

        private static void AreClose(DateTime a, DateTime b, string message)
        {
            var seconds = Math.Abs((a - b).TotalSeconds);
            const double maxSeconds = 60 * 5;
            Assert.Less(seconds, maxSeconds, message);
        }
    }
}