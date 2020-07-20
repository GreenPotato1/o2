using System;
using System.Diagnostics;
using System.IO;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.Utils;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Com.O2Bionics.ChatService.Tests
{
    // ReSharper disable UnusedAutoPropertyAccessor.Local
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class JsonTests
    {
        [Test, Explicit]
        public void TestJilPerformance()
        {
            const int n = 1000000;

            var obj = new ChatSessionInfo();

            var sw1 = Stopwatch.StartNew();
            for (var i = 0; i < 100; i++)
                SerializeJil(obj);
            sw1.Stop();

            var sw = Stopwatch.StartNew();
            for (var i = 0; i < n; i++)
                SerializeJil(obj);
            sw.Stop();
            Console.WriteLine("total: {0}ms, {1}", sw.Elapsed.TotalMilliseconds, sw.Elapsed.TotalMilliseconds / n);
        }

        private static void SerializeJil(ChatSessionInfo obj)
        {
            var json = obj.JsonStringify2();
            json.JsonUnstringify2<ChatSessionInfo>();
        }

        [Test, Explicit]
        public void TestNewtonsoftPerformance()
        {
            const int n = 1000000;

            var obj = new ChatSessionInfo();

            var sw1 = Stopwatch.StartNew();
            for (var i = 0; i < 100; i++)
                SerializeNewtonsoft(obj);
            sw1.Stop();

            var sw = Stopwatch.StartNew();
            for (var i = 0; i < n; i++)
                SerializeNewtonsoft(obj);
            sw.Stop();
            Console.WriteLine("total: {0}ms, {1}", sw.Elapsed.TotalMilliseconds, sw.Elapsed.TotalMilliseconds / n);
        }

        private static void SerializeNewtonsoft(ChatSessionInfo obj)
        {
            var json = obj.JsonStringify();

            var serializer = JsonSerializerBuilder.Default;
            using (var sr = new StringReader(json))
                serializer.Deserialize(sr, typeof(JsonSerializer));
        }

        private class TestClassJilGuid
        {
            public Guid Guid { get; set; }
            public Guid? NullableGuid1 { get; set; }
            public Guid? NullableGuid2 { get; set; }
        }

        [Test]
        public void TestJilGuid()
        {
            var obj = new TestClassJilGuid { Guid = Guid.NewGuid(), NullableGuid1 = null, NullableGuid2 = Guid.NewGuid(), };

            var jsonNewtonsoft = obj.JsonStringify();
            Console.WriteLine(jsonNewtonsoft);

            var json = obj.JsonStringify2();
            Console.WriteLine(json);

            var obj2 = json.JsonUnstringify2<TestClassJilGuid>();
            obj2.Should().BeEquivalentTo(obj);

            jsonNewtonsoft.Should().Be(
                $"{{\"Guid\":\"{obj.Guid:N}\",\"NullableGuid1\":null,\"NullableGuid2\":\"{obj.NullableGuid2:N}\"}}");
            json.Should().Be(
                $"{{\"Guid\":\"{obj.Guid:D}\",\"NullableGuid2\":\"{obj.NullableGuid2:D}\",\"NullableGuid1\":null}}");
            json.Should().NotBe(jsonNewtonsoft);
        }

        private class TestClassJilDateTime
        {
            public DateTime Timestamp1 { get; set; }
            public DateTime? Timestamp2 { get; set; }
            public DateTime? Timestamp3 { get; set; }
        }

        [Test]
        //
        // jil doesn't trim trailing zeros in ISO8601 fractional seconds but newtonsoft does.
        // it looks like:
        //                               636479820918656110L
        //      -> (newtonsoft) 2017-12-04T11:01:31.865611Z
        //      -> (jil)        2017-12-04T11:01:31.8656110Z
        //
        // see also TestNowProvider.UtcNowWithoutMilliseconds()
        //
        public void TestDateTimeFormatting([Values(636479820918656115L, 636479820918656110L)] long ticks)
        {
            var now = new DateTime(ticks);
            var obj = new TestClassJilDateTime { Timestamp1 = now, Timestamp2 = null, Timestamp3 = now, };

            var jsonNewtonsoft = obj.JsonStringify();
            Console.WriteLine(jsonNewtonsoft);

            var jsonJil = obj.JsonStringify2();
            Console.WriteLine(jsonJil);

            var obj2Jil = jsonJil.JsonUnstringify2<TestClassJilDateTime>();
            obj2Jil.Should().BeEquivalentTo(obj);

            var partsJil = jsonJil.Trim('{', '}').Split(',');
            var partsNewtonsoft = jsonNewtonsoft.Trim('{', '}').Split(',');
            if (ticks % 10L == 0)
                partsJil.Should().NotBeEquivalentTo(partsNewtonsoft);
            else
                partsJil.Should().BeEquivalentTo(partsNewtonsoft);
        }

        private class TestClassJilUnknownProperty
        {
            public string A { get; set; }
        }

        [Test]
        public void TestJilUnknownProperty()
        {
            var json = "{\"B\":\"asdfB\",\"A\":\"asdfA\"}";

            var obj2 = json.JsonUnstringify2<TestClassJilUnknownProperty>();
            obj2.Should().BeEquivalentTo(new TestClassJilUnknownProperty { A = "asdfA" });
        }

        private class TestClassJilMissingProperty
        {
            public string A { get; set; }
            public string B { get; set; }
            public string C { get; set; }
        }

        [Test]
        public void TestJilMissingProperty()
        {
            var json = "{\"C\":\"asdfC\",\"A\":\"asdfA\"}";

            var obj2 = json.JsonUnstringify2<TestClassJilMissingProperty>();
            obj2.Should().BeEquivalentTo(new TestClassJilMissingProperty { A = "asdfA", B = null, C = "asdfC" });
        }
    }
}