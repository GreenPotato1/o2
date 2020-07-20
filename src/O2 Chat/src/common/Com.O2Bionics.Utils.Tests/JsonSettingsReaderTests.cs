using System;
using System.Linq;
using Com.O2Bionics.Utils.JsonSettings;
using FluentAssertions;
using NUnit.Framework;

// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Com.O2Bionics.Utils.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class JsonSettingsReaderTests
    {
        [Test]
        public void TestMissingFile()
        {
            Action a = () => new JsonSettingsReader().ReadFromFile<TestIntSettings>("x:\\asd");
            a.Should().Throw<JsonSettingsErrorsException>().Where(x => x.Message.StartsWith("Can't read file"));
        }

        [Test]
        public void TestInvalidJson()
        {
            Action a = () => new JsonSettingsReader().ReadFromString<TestIntSettings>(@"{123");
            a.Should().Throw<JsonSettingsErrorsException>().Where(x => x.Message.StartsWith("Can't parse json"));
        }


        [SettingsRoot("test")]
        // ReSharper disable once ClassNeverInstantiated.Local
        private class TestIntSettings
        {
            public int Field1 { get; private set; }

            [Default(123)]
            public int Field2 { get; private set; }
        }

        [Test]
        public void TestInt()
        {
            const string json = @"{ test: { field1: 21, field2: 43 }}";

            var r = new JsonSettingsReader().ReadFromString<TestIntSettings>(json);
            r.Field1.Should().Be(21);
            r.Field2.Should().Be(43);
        }

        [Test]
        public void TestIntDefault()
        {
            const string json = @"{ test: { field1: 21, }}";

            var r = new JsonSettingsReader().ReadFromString<TestIntSettings>(json);
            r.Field2.Should().Be(123);
        }

        [Test]
        public void TestIntUndefined()
        {
            const string json = @"{ test: { }}";

            Action a = () => new JsonSettingsReader().ReadFromString<TestIntSettings>(json);
            a.Should().Throw<JsonSettingsErrorsException>()
                .Where(
                    x => x.Errors.Any(
                        y => y == "TestIntSettings.Field1 is of a value type but value is not provided nor default value is specified"));
        }

        [Test]
        public void TestIntInvalid()
        {
            const string json = @"{ test: { field1: 'aaa' }}";

            Action a = () => new JsonSettingsReader().ReadFromString<TestIntSettings>(json);
            a.Should().Throw<JsonSettingsErrorsException>()
                .Where(
                    x => x.Errors.Any(
                        y => y == "[test.field1] value 'aaa' is not an integer value"));
        }

        [SettingsRoot("test")]
        // ReSharper disable once ClassNeverInstantiated.Local
        private class TestIntRangeSettings
        {
            [IntRange(5, 10)]
            // ReSharper disable once UnusedMember.Local
            public int Field1 { get; private set; }
        }

        [Test]
        public void TestIntRange()
        {
            var a = new Action[]
                {
                    () => new JsonSettingsReader().ReadFromString<TestIntRangeSettings>(@"{ test: { field1: 4, }}"),
                    () => new JsonSettingsReader().ReadFromString<TestIntRangeSettings>(@"{ test: { field1: 5, }}"),
                    () => new JsonSettingsReader().ReadFromString<TestIntRangeSettings>(@"{ test: { field1: 6, }}"),
                    () => new JsonSettingsReader().ReadFromString<TestIntRangeSettings>(@"{ test: { field1: 9, }}"),
                    () => new JsonSettingsReader().ReadFromString<TestIntRangeSettings>(@"{ test: { field1: 10, }}"),
                    () => new JsonSettingsReader().ReadFromString<TestIntRangeSettings>(@"{ test: { field1: 11, }}"),
                };

            a[0].Should().Throw<JsonSettingsErrorsException>();
            a[1].Should().NotThrow();
            a[2].Should().NotThrow();
            a[3].Should().NotThrow();
            a[4].Should().NotThrow();
            a[5].Should().Throw<JsonSettingsErrorsException>();
        }

        [SettingsRoot("test")]
        // ReSharper disable once ClassNeverInstantiated.Local
        private class TestStringSettings
        {
            public string Field1 { get; private set; }

            [Default("asdf")]
            public string Field2 { get; private set; }
        }

        [Test]
        public void TestString()
        {
            const string json = @"{ test: { field1: 'aaa1', field2: 'bbb2' }}";

            var r = new JsonSettingsReader().ReadFromString<TestStringSettings>(json);
            r.Field1.Should().Be("aaa1");
            r.Field2.Should().Be("bbb2");
        }

        [Test]
        public void TestStringDefault()
        {
            const string json = @"{ test: { field1: '21', }}";

            var r = new JsonSettingsReader().ReadFromString<TestStringSettings>(json);
            r.Field2.Should().Be("asdf");
        }

        [Test]
        public void TestStringUndefined()
        {
            const string json = @"{ test: { }}";

            var r = new JsonSettingsReader().ReadFromString<TestStringSettings>(json);
            r.Field1.Should().BeNull();
        }

        [SettingsRoot("test")]
        // ReSharper disable once ClassNeverInstantiated.Local
        private class TestBoolSettings
        {
            public bool Field1 { get; private set; }

            [Default(true)]
            public bool Field2 { get; private set; }
        }

        [Test]
        public void TestBool()
        {
            const string json = @"{ test: { field1: true, }}";

            var r = new JsonSettingsReader().ReadFromString<TestBoolSettings>(json);
            r.Field1.Should().Be(true);
            r.Field2.Should().Be(true);
        }

        [SettingsRoot("test")]
        // ReSharper disable once ClassNeverInstantiated.Local
        private class TestTimeSpanSettings
        {
            public TimeSpan Field1 { get; private set; }

            [Default("1:2:3")]
            public TimeSpan Field2 { get; private set; }
        }

        [Test]
        public void TestTimeSpan()
        {
            const string json = @"{ test: { field1: '4:3:2', }}";

            var r = new JsonSettingsReader().ReadFromString<TestTimeSpanSettings>(json);
            r.Field1.Should().Be(TimeSpan.Parse("4:3:2"));
            r.Field2.Should().Be(TimeSpan.Parse("1:2:3"));
        }

        [SettingsRoot("test")]
        // ReSharper disable once ClassNeverInstantiated.Local
        private class TestUriSettings
        {
            public Uri Field1 { get; private set; }

            [Default("http://a.b.c")]
            public Uri Field2 { get; private set; }
        }

        [Test]
        public void TestUri()
        {
            const string json = @"{ test: { field1: 'https://q.w.e/', }}";

            var r = new JsonSettingsReader().ReadFromString<TestUriSettings>(json);
            r.Field1.Should().Be(new Uri("https://q.w.e/"));
            r.Field2.Should().Be(new Uri("http://a.b.c/"));
        }

        [SettingsRoot("test")]
        // ReSharper disable once ClassNeverInstantiated.Local
        private class TestEsConnectionSettingsClass
        {
            public EsConnectionSettings Field1 { get; private set; }
        }

        [Test]
        public void TestEsConnectionSettings()
        {
            const string json = @"{ test: { field1: { uris: ['http://a.b.c/', 'http://c.b.a'] }, }}";

            var r = new JsonSettingsReader().ReadFromString<TestEsConnectionSettingsClass>(json);
            r.Field1.Uris.Should().BeEquivalentTo(new Uri("http://a.b.c/"), new Uri("http://c.b.a"));
        }


        [SettingsRoot("test")]
        // ReSharper disable once ClassNeverInstantiated.Local
        private class TestParent1Settings
        {
            public Child1Settings Field1 { get; private set; }
            public Child1Settings Field2 { get; private set; }
        }

        [SettingsClass]
        // ReSharper disable once ClassNeverInstantiated.Local
        private class Child1Settings
        {
            public string Field { get; private set; }
        }

        [Test]
        public void TestParentChild()
        {
            const string json = @"{ test: { field1: { field: 'asdf' }, }}";

            var r = new JsonSettingsReader().ReadFromString<TestParent1Settings>(json);
            r.Field1.Should().NotBeNull();
            r.Field1.Field.Should().Be("asdf");
            r.Field2.Should().BeNull();
        }


        [SettingsRoot("test")]
        // ReSharper disable once ClassNeverInstantiated.Local
        private class TestParent2Settings
        {
            [SettingsRoot("test2")]
            public Child2Settings Field1 { get; private set; }

            [SettingsRoot("test2")]
            public Child2Settings Field2 { get; private set; }
        }

        [SettingsClass]
        // ReSharper disable once ClassNeverInstantiated.Local
        private class Child2Settings
        {
            public string Field { get; private set; }
        }

        [Test]
        public void TestParentChildReference()
        {
            const string json = @"{ test: { field1: { field: 'asdf1' }, field2: { field: 'asdf3' } }, test2: { field: 'asdf2' } }";

            var r = new JsonSettingsReader().ReadFromString<TestParent2Settings>(json);
            r.Field1.Should().NotBeNull();
            r.Field1.Field.Should().Be("asdf2");
            r.Field2.Field.Should().Be("asdf2");
        }
    }
}