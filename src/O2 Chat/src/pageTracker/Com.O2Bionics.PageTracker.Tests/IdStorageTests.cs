using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Com.O2Bionics.Elastic;
using Com.O2Bionics.PageTracker.Storage;
using Com.O2Bionics.PageTracker.Tests.Settings;
using Com.O2Bionics.Tests.Common;
using Com.O2Bionics.Utils;
using FluentAssertions;
using Nest;
using NUnit.Framework;

namespace Com.O2Bionics.PageTracker.Tests
{
    [TestFixture]
    public sealed class IdStorageTests : PageTrackerTestsBase
    {
        [SetUp]
        public void SetUp()
        {
            PageTrackerIndexHelper.DeleteIndices(Settings);
            PageTrackerIndexHelper.CreateIndices(Settings);
        }

        [Test]
        public void TestCtorNoIndex()
        {
            PageTrackerIndexHelper.DeleteIndices(Settings);

            var client = new EsClient(Settings.ElasticConnection);
            Action a = () => new IdStorage(Settings, client);
            var expectedMessage = $"Id storage index {client.ClusterName}/'{Settings.IdStorageIndex.Name}' doesn't exists";
            a.Should().Throw<Exception>()
                .Where(e => e.Message.StartsWith(expectedMessage));
        }

        [Test]
        public void TestCtorNoDocument()
        {
            var client = new EsClient(Settings.ElasticConnection);

            client.Client.Delete(
                DocumentPath<IdStorageDoc>.Id((int)IdScope.Visitor),
                x => x.Index(Settings.IdStorageIndex.Name));

            Action a = () => new IdStorage(Settings, client);
            var expectedMessage = $"Get id={(int)IdScope.Visitor} failed on {client.ClusterName}/{Settings.IdStorageIndex.Name}:";
            a.Should().Throw<Exception>()
                .Where(e => e.Message.StartsWith(expectedMessage));
        }

        [Test]
        public async Task TestMaxValue()
        {
            const int blockSize = 10;
            const ulong initial = ulong.MaxValue - blockSize;
            var settings = new TestPageTrackerSettings { IdStorageBlockSize = blockSize };

            var client = new EsClient(settings.ElasticConnection);

            client.Client.Delete(
                DocumentPath<IdStorageDoc>.Id((int)IdScope.Visitor),
                d => d.Index(settings.IdStorageIndex.Name));
            client.Flush(settings.IdStorageIndex.Name);

            PageTrackerIndexHelper.AddIdDocument(client, settings.IdStorageIndex.Name, IdScope.Visitor, initial);
            client.Flush(settings.IdStorageIndex.Name);
            var storage = new IdStorage(settings, client);

            var r1 = await storage.Add(IdScope.Visitor);
            r1.Should().Be(initial + blockSize);
        }

        [Test]
        public async Task TestAdd([Values(null, 1, 100)] int? blockSizeValue)
        {
            var settings = new TestPageTrackerSettings();
            if (blockSizeValue.HasValue) settings.IdStorageBlockSize = blockSizeValue.Value;
            var expectedBlockSize = settings.IdStorageBlockSize;


            var client = new EsClient(settings.ElasticConnection);
            var storage = new IdStorage(settings, client);

            var r1 = await storage.Add(IdScope.Visitor);
            r1.Should().Be((ulong)expectedBlockSize);

            var r2 = await storage.Add(IdScope.Visitor);
            r2.Should().Be((ulong)expectedBlockSize * 2);
        }

        [Test]
        public void TestParallelGet()
        {
            const int threadsNumber = 5;
            const int iterationsNumber = 100;

            var client = new EsClient(Settings.ElasticConnection);
            var storage = new IdStorage(Settings, client);

            var allThreadsResult = Enumerable.Range(0, threadsNumber)
                .ToDictionary(i => i, i => new List<ulong>(iterationsNumber));
            var threads = new BackgroundThreads(
                threadsNumber,
                threadIndex =>
                    {
                        var results = allThreadsResult[threadIndex];
                        for (var i = 0; i < iterationsNumber; i++)
                        {
                            var last = storage.Add(IdScope.Visitor).WaitAndUnwrapException();
                            results.Add(last);
                        }
                    });
            threads.StartAndJoin();

            var actual = allThreadsResult.Values.SelectMany(x => x).OrderBy(x => x);
            var blockSize = (ulong)Settings.IdStorageBlockSize;
            var expected = Enumerable
                .Range(1, threadsNumber * iterationsNumber)
                .Select(i => blockSize * (ulong)i)
                .OrderBy(x => x);
            actual.Should().BeEquivalentTo(expected);
        }
    }
}