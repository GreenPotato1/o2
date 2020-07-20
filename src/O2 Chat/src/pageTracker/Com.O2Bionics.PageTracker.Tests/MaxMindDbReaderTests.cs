using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Com.O2Bionics.PageTracker.Tests.Utilities;
using Com.O2Bionics.PageTracker.Utilities;
using Com.O2Bionics.Utils;
using CsvHelper;
using CsvHelper.Configuration;
using JetBrains.Annotations;
using MaxMind.Db;
using NUnit.Framework;

namespace Com.O2Bionics.PageTracker.Tests
{
    public sealed class MaxMindDbReaderTests : PageTrackerTestsBase
    {
        private const string Language = "en";

        [Test]
        public void Test()
        {
            using (var reader = new Reader(Path.Combine(Settings.MaxMindGeoIpDatabasePath)))
            {
                const string address = "8.8.8.8";
                var ipAddress = IPAddress.Parse(address);
                var response = reader.Find<Dictionary<string, object>>(ipAddress);
                Assert.IsNotNull(response, nameof(response));

                Console.WriteLine(reader.Metadata.JsonStringify());
                Console.WriteLine(response.ToString());

                var country = GetName(response, "country", Language);
                var city = GetName(response, "city", Language);
                var subdivisions = GetListValue(response, "subdivisions");
                double lat = 0;
                double lon = 0;
                if (response.TryGetValue("location", out var v) && v is Dictionary<string, object> location)
                {
                    lat = (double)location["latitude"];
                    lon = (double)location["longitude"];
                }

                Console.WriteLine("{0}|{1}|{2} {3}:{4}", country, string.Join("/", subdivisions), city, lat, lon);
            }
        }

        private static List<string> GetListValue([NotNull] Dictionary<string, object> dict, [NotNull] string name)
        {
            return dict.TryGetValue(name, out var raw) && raw is List<object> list && 0 < list.Count
                ? list.OfType<Dictionary<string, object>>()
                    .Select(x => x.TryGetValue("names", out var names) ? names : null)
                    .Where(x => null != x)
                    .OfType<Dictionary<string, object>>()
                    .Select(x => x.TryGetValue(Language, out var val) ? val : null)
                    .Where(x => null != x)
                    .OfType<string>()
                    .ToList()
                : new List<string>();
        }

        [Test]
        [Explicit]
        public void Test3()
        {
            var directoryName = Path.GetDirectoryName(Settings.MaxMindGeoIpDatabasePath);
            if (string.IsNullOrEmpty(directoryName))
                throw new Exception($"{nameof(Path.GetDirectoryName)}({Settings.MaxMindGeoIpDatabasePath}) must have returned not empty string.");

            var path = Path.Combine(directoryName, "GeoLite2-City-Locations-en.csv");

            using (var r = File.OpenText(path))
            using (var r2 = new CsvReader(r, new CsvConfiguration { HasHeaderRecord = true, IgnoreBlankLines = true, }))
            {
                int l = 0;
                var count = 0;
                int[] ml = null;
                int[] mlt = null;
                HashSet<string>[] dv = null;

                while (r2.Read())
                {
                    count++;

                    if (ml == null)
                    {
                        l = r2.FieldHeaders.Length;
                        ml = new int[l];
                        mlt = new int[l];
                        dv = Enumerable.Range(0, l).Select(i => new HashSet<string>()).ToArray();
                    }

                    var rec = r2.CurrentRecord;
                    for (var i = 0; i < l; i++)
                    {
                        var v = rec[i];
                        if (v != null)
                        {
                            dv[i].Add(v);
                            if (v.Length > ml[i])
                                ml[i] = v.Length;
                            if (v.Trim().Length > mlt[i])
                                mlt[i] = v.Trim().Length;
                        }
                    }
                }

                Console.WriteLine(count);

                for (var i = 0; i < l; i++)
                    Console.WriteLine("{0} {1} {2} {3}", r2.FieldHeaders[i], ml[i], mlt[i], dv[i].Count);

                Console.WriteLine();
                foreach (var x in dv[3].OrderBy(x => x)) Console.WriteLine(x);

                Console.WriteLine();
                foreach (var x in dv[5].OrderBy(x => x)) Console.WriteLine(x);
            }
        }

        [Test]
        [Explicit]
        public void Test2()
        {
            using (var reader = new Reader(Settings.MaxMindGeoIpDatabasePath))
            {
                Parallel.For(
                    0,
                    256 * 256 * 256,
                    new ParallelOptions { MaxDegreeOfParallelism = 6 },
                    i =>
                        {
                            var b = new byte[4];

                            b[2] = (byte)(i & 0xff);
                            b[1] = (byte)((i >> 8) & 0xff);
                            b[0] = (byte)((i >> 16) & 0xff);

                            if (b[0] == 0 || b[0] == 255
                                          || b[1] == 0 || b[1] == 255
                                          || b[2] == 0 || b[2] == 255) return;

                            for (var j = 1; j < 255; j++)
                            {
                                b[3] = (byte)j;
                                var ip = string.Format("{0}.{1}.{2}.{3}", b[0], b[1], b[2], b[3]);

                                var ipAddress = IPAddress.Parse(ip);
                                var response = reader.Find<Dictionary<string, object>>(ipAddress);

                                if (response == null)
                                {
                                    Console.WriteLine("{0} - null", ip);
                                }
                                else
                                    try
                                    {
                                        var country = GetName(response, "country", Language);
                                        var hasCountry = country != null;
                                        // var country = response["country"]["names"].Value<string>("en");

                                        var subdivisions = GetListValue(response, "subdivisions");
                                        var hasSubdivisions = subdivisions != null;
                                        var subdivisionsCount = hasSubdivisions ? subdivisions.Count() : 0;
                                        // .Select(x => x["names"].Value<string>("en")).ToList();

                                        var city = GetName(response, "city", Language);
                                        var hasCity = city != null;
                                        // ["names"].Value<string>("en");

                                        response.TryGetValue("location", out var location);
                                        var hasLocation = location != null;
                                        //                if (location != null)
                                        //                {
                                        //                    var lat = location.Value<double>("latitude");
                                        //                    var lon = location.Value<double>("longitude");
                                        //                }

                                        if (!hasCountry /*|| !hasSubdivisions || !hasCity */ || !hasLocation)
                                            Console.WriteLine("{0} {1} {2} {3} {4} {5}", i, ip, hasCountry, hasSubdivisions, hasCity, hasLocation);
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine("{0}:{1}", ip, e);
                                    }
                            }

                            if (i % 100000 == 0)
                                Console.WriteLine("{0} {1}.{2}.{3}.", i, b[0], b[1], b[2]);
                        });
            }
        }

        [CanBeNull]
        private static string GetName([NotNull] Dictionary<string, object> dict, [NotNull] string name, [NotNull] string language)
        {
            return dict.TryGetValue(name, out var value) && value is Dictionary<string, object> dict2 &&
                   dict2.TryGetValue("names", out var rawNames) && rawNames is Dictionary<string, object> names &&
                   names.TryGetValue(language, out var rawResult) && rawResult is string result
                ? result
                : null;
        }
    }
}