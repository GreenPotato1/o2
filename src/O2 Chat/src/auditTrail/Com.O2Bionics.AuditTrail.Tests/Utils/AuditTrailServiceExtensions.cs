using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Com.O2Bionics.AuditTrail.Contract;
using Com.O2Bionics.ChatService.Contract.AuditTrail;
using Com.O2Bionics.Tests.Common;
using Com.O2Bionics.Utils;
using Com.O2Bionics.Utils.Properties;
using JetBrains.Annotations;
using NUnit.Framework;

namespace Com.O2Bionics.AuditTrail.Tests.Utils
{
    public static class AuditTrailServiceExtensions
    {
        public static async Task FetchAndParse<T>(
            [NotNull] this IAuditTrailService service,
            [NotNull] string operation,
            int expectedSize,
            [NotNull] AuditEvent<T>[] buffer,
            [CanBeNull] Action<Filter> filterAction = null,
            int maxAttempts = 1000,
            string customerId = TestConstants.CustomerIdString)
        {
            if (null == service)
                throw new ArgumentNullException(nameof(service));
            if (string.IsNullOrEmpty(operation))
                throw new ArgumentNullException(nameof(operation));
            if (expectedSize <= 0)
                throw new ArgumentException(string.Format(Resources.ArgumentMustBePositive2, nameof(expectedSize), expectedSize));
            if (null == buffer)
                throw new ArgumentNullException(nameof(buffer));
            Assert.GreaterOrEqual(buffer.Length, expectedSize, "The buffer must be large enough.");
            if (maxAttempts <= 0)
                throw new ArgumentException(string.Format(Resources.ArgumentMustBePositive2, nameof(maxAttempts), maxAttempts));
            if (string.IsNullOrEmpty(customerId))
                throw new ArgumentNullException(nameof(customerId));

            var rawDocuments = await FetchRawDocuments(service, operation, expectedSize, filterAction, maxAttempts, customerId);
            ParseDocuments(expectedSize, operation, rawDocuments, buffer);
        }

        private static async Task<List<string>> FetchRawDocuments(
            [NotNull] IAuditTrailService service,
            [NotNull] string operation,
            int size,
            [CanBeNull] Action<Filter> filterAction,
            int maxAttempts,
            string customerId)
        {
            var stopwatch = Stopwatch.StartNew();

            var result = new List<string>(size);
            var filter = new Filter(ProductCodes.Chat, size)
                {
                    CustomerId = customerId,
                    Operations = new List<string> { operation }
                };
            filterAction?.Invoke(filter);
            filter.Validate();

            for (var i = 0; i < maxAttempts; i++)
            {
                if (0 < i)
                {
                    var report =
                        $"Sleep because selected {result?.Count ?? 0} out of the expected {size} {operation}, elapsed {stopwatch.ElapsedMilliseconds} ms.";
                    Console.WriteLine(report);
                    Thread.Sleep(1);
                }

                var response = await service.SelectFacets(filter);
                result = response?.RawDocuments;
                if (null != result && filter.PageSize <= result.Count)
                {
                    var report = $"{nameof(service.SelectFacets)} of {size} {operation} took {stopwatch.ElapsedMilliseconds} ms.";
                    Console.WriteLine(report);
                    break;
                }
            }

            Assert.NotNull(result, "result");
            Assert.AreEqual(filter.PageSize, result.Count, $"Response.RawDocuments.Count of {size} {operation}.");
            return result;
        }

        private static void ParseDocuments<T>(
            int expectedSize,
            string operation,
            List<string> rawDocuments,
            [NotNull] AuditEvent<T>[] buffer)
        {
            var stopwatch = Stopwatch.StartNew();

            Enumerable.Range(0, expectedSize).AsParallel().ForAll(
                i =>
                    {
                        buffer[i] = rawDocuments[i].JsonUnstringify2<AuditEvent<T>>();
                        Assert.NotNull(buffer[i], "buffer[{0}] {1}", i, operation);
                    });
            stopwatch.Stop();

            var report = $"Parse {expectedSize} {operation} documents took {stopwatch.ElapsedMilliseconds} ms.";
            Console.WriteLine(report);
        }
    }
}