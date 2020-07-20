using System;
using System.Collections.Generic;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Com.O2Bionics.ChatService.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class Class1Tests
    {
        [Test]
        public void T()
        {
            var list1 = new List<string> { "s1", "", null, "12" };
            var r1 = JsonConvert.SerializeObject(list1);
            Console.WriteLine(r1);
            var list2 = JsonConvert.DeserializeObject<List<string>>(r1);
            list2.Should().Equal(list1);
        }

        [Test]
        public void T2()
        {
            var list1 = JsonConvert.DeserializeObject<List<string>>("[]");
            Console.WriteLine(list1);
        }
    }
}