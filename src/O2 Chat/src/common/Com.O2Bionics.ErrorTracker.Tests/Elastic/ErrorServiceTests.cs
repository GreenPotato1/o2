using System;
using System.Threading;
using System.Threading.Tasks;
using Nest;
using NUnit.Framework;

namespace Com.O2Bionics.ErrorTracker.Tests.Elastic
{
    [TestFixture]
    public sealed class ErrorServiceTests : BaseElasticTest
    {
        private const int TimeoutMilliseconds = 20 * 1000;
        private const string NotExistingMessage = "Not existing";

        private const string X1 = "ErrorOne";
        private const string X2 = "ErrorTwo";

        private const int HalfCount = 51;

        [Test]
        public async Task NotExistingMessageTest()
        {
            await SelectByMessage(NotExistingMessage, 0, "Not existing message");
        }

        [Test]
        public async Task OneMessageTest()
        {
            await SelectByMessage(ExistingMessage, 1, "Existing message");
        }

        [Test]
        public async Task HalfMessagesTest()
        {
            QueryContainer Query(QueryContainerDescriptor<ErrorInfo> q) => q.Match(m => m.Field(f => f.ExceptionMessage).Query(X1));
            await SelectTest(Query, HalfCount, "Half messages");
        }

        [Test]
        public async Task AllMessagesTest()
        {
            QueryContainer Query(QueryContainerDescriptor<ErrorInfo> q) => q;
            await SelectTest(Query, HalfCount * 2, "All messages");
        }

        protected override void CreateIndex()
        {
            base.CreateIndex();

            var dateTime = DateTime.UtcNow;

            const int size = 2 * HalfCount;
            var errorInfos = new ErrorInfo[size];
            for (int i = 0; i < size; i++)
                errorInfos[i] = NewItem(i, dateTime);

            Service.Save(errorInfos);
            WaitForSaving();

            var client = GetClient();
            client.Flush(IndexName);
        }

        private static ErrorInfo NewItem(int i, DateTime dateTime)
        {
            var result = new ErrorInfo
                {
                    CustomerId = 10,
                    VisitorId = i < HalfCount ? 200u : 0u,
                    UserId = i < HalfCount ? 0u : 3000u,
                    Message = Prefix + i,
                    ExceptionMessage = i < HalfCount ? X1 : X2,
                    ExceptionSource = "ExceptionSource1",
                    ExceptionStack = "Stack here",
                    ExceptionType = typeof(NullReferenceException).FullName,
                    LoggerName = typeof(ErrorServiceTests).FullName,
                    Url = "https://localhost/err123",
                    TimeZoneOffset = -180,
                    TimeZoneName = "Russia Standard Time",
                    Application = "Other",
                    ClientIp = "127.0.0.1",
                    HostName = "localhost",
                    UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:55.0) Gecko/20100101 Firefox/55.0",
                    Timestamp = dateTime
                };
            return result;
        }

        private void WaitForSaving()
        {
            for (int i = 0; 0 < Service.QueueSize && i < TimeoutMilliseconds; ++i)
                Thread.Sleep(1);

            var queueSize = Service.QueueSize;
            if (0 < queueSize)
                throw new Exception($"After {TimeoutMilliseconds} ms there are {queueSize} error infos in the queue.");

            var client = GetClient();
            client.Flush(IndexName);
        }
    }
}