using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Com.O2Bionics.PageTracker.Storage;
using Com.O2Bionics.Tests.Common;
using Com.O2Bionics.Utils;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace Com.O2Bionics.PageTracker.Tests
{
    [TestFixture]
    public sealed class IdGeneratorTests
    {
        [Test]
        public async Task TestNewId([Values(0, 117)] int initialIdStorageValue)
        {
            const int blocks = 3;
            const int blockSize = 10;

            var storage = CreateStorageMock(blockSize, (ulong)initialIdStorageValue);

            var generator = new IdGenerator(storage);

            var result = new List<ulong>();
            for (var i = 0; i < blockSize * blocks + 3; i++)
            {
                result.Add(await generator.NewId(IdScope.Visitor));
            }

            result.Should().BeEquivalentTo(
                Enumerable.Range(1, blockSize * blocks + 3).Select(i => (ulong)i + (ulong)initialIdStorageValue),
                o => o.WithStrictOrdering());
            _idStorageCallNumber.Should().Be(blocks + 1);
        }

        [Test]
        public void TestParallelNewId([Values(2, 100)] int blockSize)
        {
            const int threadsNumber = 7;
            const int iterationsNumber = 2001;

            var storage = CreateStorageMock((ulong)blockSize);
            var generator = new IdGenerator(storage);

            var allThreadsResult = Enumerable.Range(0, threadsNumber)
                .ToDictionary(i => i, i => new List<ulong>(iterationsNumber));
            var threads = new BackgroundThreads(
                threadsNumber,
                threadIndex =>
                    {
                        var results = allThreadsResult[threadIndex];
                        for (var i = 0; i < iterationsNumber; i++)
                        {
                            results.Add(generator.NewId(IdScope.Visitor).WaitAndUnwrapException());
                        }
                    });
            threads.StartAndJoin();

            const int totalIdsGenerated = threadsNumber * iterationsNumber;
            var actual = allThreadsResult.Values.SelectMany(x => x);
            var expected = Enumerable.Range(1, totalIdsGenerated).Select(i => (ulong)i);
            actual.OrderBy(x => x).ToList()
                .Should().BeEquivalentTo(
                    expected.OrderBy(x => x).ToList(),
                    s => s.WithStrictOrderingFor(x => x));
            var expectedStorageCalls = totalIdsGenerated / blockSize + (totalIdsGenerated % blockSize == 0 ? 0 : 1);
            _idStorageCallNumber.Should().Be(expectedStorageCalls);
        }

        [Test]
        [Explicit]
        public void TestNewIdPerformance(
            [Values(1, 2, 3)] int iteration,
            [Values(10, 100, 1000)] int blockSize)
        {
            const int threadsNumber = 40;
            const int iterationsNumber = 16001;

            var storage = CreateStorageMock((ulong)blockSize);

            var generator = new IdGenerator(storage);

            var threads = new BackgroundThreads(
                threadsNumber,
                async threadIndex =>
                    {
                        for (var i = 0; i < iterationsNumber; i++)
                        {
                            await generator.NewId(IdScope.Visitor);
                        }
                    });
            threads.Measure(nameof(IdGeneratorTests), nameof(TestParallelNewId), iterationsNumber);
        }

        private static long _idStorageCallNumber;

        private static IIdStorage CreateStorageMock(ulong blockSize, ulong initialValue = 0ul)
        {
            _idStorageCallNumber = 0L;

            var storage = Substitute.For<IIdStorage>();
            storage.BlockSize.Returns(blockSize);
            storage.Add(Arg.Any<IdScope>())
                .Returns(
                    ci =>
                        {
                            var n = initialValue
                                    + blockSize * (ulong)Interlocked.Increment(ref _idStorageCallNumber);
                            return Task.FromResult(n);
                        });
            return storage;
        }
    }
}