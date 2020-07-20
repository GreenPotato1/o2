using System;
using System.Linq;
using Com.O2Bionics.ChatService.DataModel;
using FluentAssertions;
using NUnit.Framework;

namespace Com.O2Bionics.ChatService.Tests.SettingsTests
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public sealed class TestSettingsTests
    {
        public class Property
        {
            private static readonly Type m_roType = typeof(TestSettings);
            private static readonly Type m_rwType = typeof(WritableTestSettings);

            public string TypeName { get; set; }
            public string Name { get; set; }
            public object Default { get; set; }
            public object[] Values { get; set; }
            public string[] DbValues { get; set; }

            public override string ToString()
            {
                return Name;
            }

            public object GetRoValue(TestSettings s)
            {
                // ReSharper disable once PossibleNullReferenceException
                return m_roType.GetProperties().FirstOrDefault(x => x.Name == Name).GetMethod.Invoke(s, new object[0]);
            }

            public object GetRwValue(WritableTestSettings s)
            {
                // ReSharper disable once PossibleNullReferenceException
                return m_rwType.GetProperties().FirstOrDefault(x => x.Name == Name).GetMethod.Invoke(s, new object[0]);
            }

            public object SetRwValue(WritableTestSettings s, object v)
            {
                // ReSharper disable once PossibleNullReferenceException
                return m_rwType.GetProperties().FirstOrDefault(x => x.Name == Name).SetMethod.Invoke(s, new[] { v });
            }
        }

        #region Properties

        public static readonly Property[] Properties =
            {
                new Property
                    {
                        TypeName = "Bool",
                        Name = "BoolProperty",
                        Default = true,
                        Values = new object[] { true, false },
                        DbValues = new[] { "True", "False" },
                    },
                new Property
                    {
                        TypeName = "Bool?",
                        Name = "NullableBoolProperty1",
                        Default = true,
                        Values = new object[] { true, null },
                        DbValues = new[] { "True", null },
                    },
                new Property
                    {
                        TypeName = "Bool?",
                        Name = "NullableBoolProperty2",
                        Default = null,
                        Values = new object[] { null, true },
                        DbValues = new[] { null, "True" },
                    },
                new Property
                    {
                        TypeName = "Int32",
                        Name = "IntProperty",
                        Default = 20,
                        Values = new object[] { 1, 2 },
                        DbValues = new[] { "1", "2" },
                    },
                new Property
                    {
                        TypeName = "Int32?",
                        Name = "NullableIntProperty1",
                        Default = 20,
                        Values = new object[] { 3, null },
                        DbValues = new[] { "3", null },
                    },
                new Property
                    {
                        TypeName = "Int32?",
                        Name = "NullableIntProperty2",
                        Default = null,
                        Values = new object[] { null, 4 },
                        DbValues = new[] { null, "4" },
                    },
                new Property
                    {
                        TypeName = "Single",
                        Name = "FloatProperty",
                        Default = 20f,
                        Values = new object[] { 3f, 4f },
                        DbValues = new[] { "3", "4" },
                    },
                new Property
                    {
                        TypeName = "Single?",
                        Name = "NullableFloatProperty1",
                        Default = 20f,
                        Values = new object[] { null, 5f },
                        DbValues = new[] { null, "5" },
                    },
                new Property
                    {
                        TypeName = "Single?",
                        Name = "NullableFloatProperty2",
                        Default = null,
                        Values = new object[] { 6f, null },
                        DbValues = new[] { "6", null },
                    },
                new Property
                    {
                        TypeName = "Double",
                        Name = "DoubleProperty",
                        Default = 20d,
                        Values = new object[] { 3d, 4d },
                        DbValues = new[] { "3", "4" },
                    },
                new Property
                    {
                        TypeName = "Double?",
                        Name = "NullableDoubleProperty1",
                        Default = 20d,
                        Values = new object[] { null, 5d },
                        DbValues = new[] { null, "5" },
                    },
                new Property
                    {
                        TypeName = "Double?",
                        Name = "NullableDoubleProperty2",
                        Default = null,
                        Values = new object[] { 6d, null },
                        DbValues = new[] { "6", null },
                    },
                new Property
                    {
                        TypeName = "Decimal",
                        Name = "DecimalProperty",
                        Default = 20m,
                        Values = new object[] { 3m, 4m },
                        DbValues = new[] { "3", "4" },
                    },
                new Property
                    {
                        TypeName = "Decimal?",
                        Name = "NullableDecimalProperty1",
                        Default = 20m,
                        Values = new object[] { null, 5m },
                        DbValues = new[] { null, "5" },
                    },
                new Property
                    {
                        TypeName = "Decimal?",
                        Name = "NullableDecimalProperty2",
                        Default = null,
                        Values = new object[] { 6m, null },
                        DbValues = new[] { "6", null },
                    },
                new Property
                    {
                        TypeName = "String",
                        Name = "StringProperty1",
                        Default = "te\"stTEST1",
                        Values = new object[] { "test1", "test2" },
                        DbValues = new[] { "test1", "test2" },
                    },
                new Property
                    {
                        TypeName = "String",
                        Name = "StringProperty2",
                        Default = null,
                        Values = new object[] { null, "test3" },
                        DbValues = new[] { null, "test3" },
                    },
                new Property
                    {
                        TypeName = "DateTime",
                        Name = "DateTimeProperty",
                        Default = new DateTime(2014, 1, 2, 3, 4, 5),
                        Values = new object[] { new DateTime(2015, 3, 4, 5, 6, 7), new DateTime(2016, 8, 9, 10, 11, 12) },
                        DbValues = new[] { "2015-03-04T05:06:07.0000000", "2016-08-09T10:11:12.0000000" },
                    },
                new Property
                    {
                        TypeName = "DateTime?",
                        Name = "NullableDateTimeProperty1",
                        Default = new DateTime(2014, 1, 2, 3, 4, 5),
                        Values = new object[] { null, new DateTime(2017, 8, 9, 10, 11, 12) },
                        DbValues = new[] { null, "2017-08-09T10:11:12.0000000" },
                    },
                new Property
                    {
                        TypeName = "DateTime?",
                        Name = "NullableDateTimeProperty2",
                        Default = null,
                        Values = new object[] { new DateTime(2018, 3, 4, 5, 6, 7), null },
                        DbValues = new[] { "2018-03-04T05:06:07.0000000", null },
                    },
                new Property
                    {
                        TypeName = "TimeSpan",
                        Name = "TimeSpanProperty",
                        Default = TimeSpan.FromHours(2),
                        Values = new object[] { TimeSpan.FromMinutes(2), TimeSpan.FromSeconds(2) },
                        DbValues = new[] { "00:02:00", "00:00:02" },
                    },
                new Property
                    {
                        TypeName = "TimeSpan?",
                        Name = "NullableTimeSpanProperty1",
                        Default = TimeSpan.FromHours(12),
                        Values = new object[] { null, TimeSpan.FromSeconds(3) },
                        DbValues = new[] { null, "00:00:03" },
                    },
                new Property
                    {
                        TypeName = "TimeSpan?",
                        Name = "NullableTimeSpanProperty2",
                        Default = null,
                        Values = new object[] { TimeSpan.FromMinutes(4), null },
                        DbValues = new[] { "00:04:00", null },
                    },
            };

        #endregion

        [Test]
        public void TestRoDefaultValue([ValueSource(nameof(Properties))] Property p)
        {
            p.GetRoValue(new TestSettings(new PROPERTY_BAG[0])).Should().Be(p.Default);
        }

        [Test]
        public void TestRwDefaultValue([ValueSource(nameof(Properties))] Property p)
        {
            p.GetRwValue(new WritableTestSettings(new TestSettings(new PROPERTY_BAG[0]))).Should().Be(p.Default);
            p.GetRwValue(new WritableTestSettings()).Should().Be(p.Default);
        }

        [Test]
        public void TestRoReadValue([ValueSource(nameof(Properties))] Property p)
        {
            var record = new PROPERTY_BAG
                {
                    PROPERTY_BAG_SKEY = 1,
                    PROPERTY_NAME = p.Name,
                    PROPERTY_TYPE = p.TypeName,
                    PROPERTY_VALUE = p.DbValues[0],
                };
            p.GetRoValue(new TestSettings(new[] { record })).Should().Be(p.Values[0]);
        }

        [Test]
        public void TestRwReadValue([ValueSource(nameof(Properties))] Property p)
        {
            var record = new PROPERTY_BAG
                {
                    PROPERTY_BAG_SKEY = 1,
                    PROPERTY_NAME = p.Name,
                    PROPERTY_TYPE = p.TypeName,
                    PROPERTY_VALUE = p.DbValues[0],
                };
            p.GetRwValue(new WritableTestSettings(new TestSettings(new[] { record }))).Should().Be(p.Values[0]);
        }

        [Test]
        public void TestRwWriteValue([ValueSource(nameof(Properties))] Property p)
        {
            var roSettings = new TestSettings(new PROPERTY_BAG[0]);
            var rwSettings = new WritableTestSettings(roSettings);
            p.SetRwValue(rwSettings, p.Values[0]);

            // original object should not be affected
            p.GetRoValue(roSettings).Should().Be(p.Default);

            // writable class should be updated
            p.GetRwValue(rwSettings).Should().Be(p.Values[0]);

            var dirty = rwSettings.GetDirtyRecords().ToList();
            dirty.Should().HaveCount(1);
            dirty[0].Should().BeEquivalentTo(
                new
                    {
                        PROPERTY_NAME = p.Name,
                        PROPERTY_TYPE = p.TypeName,
                        PROPERTY_VALUE = p.DbValues[0],
                    },
                x => x.ExcludingMissingMembers());
            dirty[0].PROPERTY_BAG_SKEY.Should().BeLessThan(0);

            // save updated value to ro instance
            p.GetRoValue(new TestSettings(rwSettings)).Should().Be(p.Values[0]);
        }

        [Test]
        public void TestRwOverwriteValue([ValueSource(nameof(Properties))] Property p)
        {
            var record = new PROPERTY_BAG
                {
                    PROPERTY_BAG_SKEY = 1,
                    PROPERTY_NAME = p.Name,
                    PROPERTY_TYPE = p.TypeName,
                    PROPERTY_VALUE = p.DbValues[0],
                };
            var roSettings = new TestSettings(new[] { record });
            var rwSettings = new WritableTestSettings(roSettings);
            p.SetRwValue(rwSettings, p.Values[1]);

            // original object should not be affected
            record.PROPERTY_VALUE.Should().Be(p.DbValues[0]);
            p.GetRoValue(roSettings).Should().Be(p.Values[0]);

            // writable class should be updated
            p.GetRwValue(rwSettings).Should().Be(p.Values[1]);

            var dirty = rwSettings.GetDirtyRecords().ToList();
            dirty.Should().HaveCount(1);
            dirty[0].Should().BeEquivalentTo(
                new PROPERTY_BAG
                    {
                        PROPERTY_BAG_SKEY = 1,
                        PROPERTY_NAME = p.Name,
                        PROPERTY_TYPE = p.TypeName,
                        PROPERTY_VALUE = p.DbValues[1],
                    });

            // save updated value to ro instance
            p.GetRoValue(new TestSettings(rwSettings)).Should().Be(p.Values[1]);
        }

        [Test]
        public void TestBadValue()
        {
            var p = Properties.Single(x => x.Name == "IntProperty");

            var record = new PROPERTY_BAG
                {
                    PROPERTY_BAG_SKEY = 1,
                    PROPERTY_NAME = p.Name,
                    PROPERTY_TYPE = p.TypeName,
                    PROPERTY_VALUE = "asd",
                };
            var settings = new TestSettings(new[] { record });
            settings.IntProperty.Should().Be((int)p.Default);
        }

        [Test]
        public void TestUnknownValue()
        {
            var record = new PROPERTY_BAG
                {
                    PROPERTY_BAG_SKEY = 1,
                    PROPERTY_NAME = "Unknown",
                    PROPERTY_TYPE = "Some",
                    PROPERTY_VALUE = "asd",
                };
            Action action = () => new TestSettings(new[] { record });
            action.Should().NotThrow();
        }
    }
}