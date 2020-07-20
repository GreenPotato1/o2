using System;
using System.Collections.Generic;
using System.Linq;
using Com.O2Bionics.FeatureService.Impl;
using FluentAssertions;
using KellermanSoftware.CompareNetObjects;
using NUnit.Framework;
using Oracle.ManagedDataAccess.Client;

namespace Com.O2Bionics.FeatureService.Tests
{
    [TestFixture]
    public class FeatureAggregationMethodTests
    {
        private readonly DatabaseHelper m_dbh = new DatabaseHelper(new FeatureServiceTestSettings());

        [Test]
        public void TestEnumMembers()
        {
            var enumItems = GetEnumItems<FeatureValueAggregationMethod>();
            var dbItems = GetDatabaseItems();

            var compareResult = new CompareLogic().Compare(enumItems, dbItems);
            compareResult.AreEqual.Should().BeTrue(compareResult.DifferencesString);
        }

        private static Dictionary<int, string> GetEnumItems<TEnum>()
        {
            return Enum.GetNames(typeof(TEnum))
                .Where(x => x != "Default")
                .ToDictionary(x => (int)Enum.Parse(typeof(TEnum), x), x => x);
        }

        private Dictionary<int, string> GetDatabaseItems()
        {
            return m_dbh.Query(
                db =>
                    {
                        using (var cmd = new OracleCommand("select AGGREGATION_METHOD_ID, DESCRIPTION from FEATURE_AGGREGATION_METHOD"))
                        {
                            using (var reader = db.ExecuteReader(cmd))
                            {
                                var items = new Dictionary<int, string>();
                                while (reader.Read())
                                {
                                    items.Add(reader.GetInt32(0), reader.GetString(1));
                                }
                                return items;
                            }
                        }
                    });
        }
    }
}