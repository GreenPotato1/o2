using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace Com.O2Bionics.Utils.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class EntityCreateUtilityTests
    {
        [Test]
        public void TestCreateId()
        {
            var list = Enumerable.Range(0, 3).Select(i => EntityCreateUtility.GenerateId()).ToList();
            list.Should().Contain(x => x > 0);
        }

        [Test]
        [Explicit]
        public void TestMassiveGeneration()
        {
            var lists = Enumerable.Range(0, 10)
                .AsParallel()
                .Select(_ => Enumerable.Range(0, 1000000).Select(i => EntityCreateUtility.GenerateId()).ToList())
                .ToList();
            var r = lists.SelectMany(x => x).ToList();
            Console.WriteLine(r.Count);
            Console.WriteLine(r.Count - r.Distinct().Count());
            Console.WriteLine(r.Take(1000).Select(x => x.ToString()).JoinAsString());
        }
    }
}