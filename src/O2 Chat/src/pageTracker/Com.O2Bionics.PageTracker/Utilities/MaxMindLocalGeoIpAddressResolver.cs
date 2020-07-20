using System;
using System.Collections.Generic;
using System.Net;
using Com.O2Bionics.PageTracker.Contract;
using JetBrains.Annotations;
using log4net;
using MaxMind.Db;

namespace Com.O2Bionics.PageTracker.Utilities
{
    public sealed class MaxMindLocalGeoIpAddressResolver : IGeoIpAddressResolver
    {
        private const string DefaultLang = "en";

        private const FileAccessMode AccessMode = FileAccessMode.Memory;

        private static readonly ILog m_log = LogManager.GetLogger(typeof(MaxMindLocalGeoIpAddressResolver));

        private readonly Reader m_reader;

        public MaxMindLocalGeoIpAddressResolver(PageTrackerSettings settings)
        {
            var path = settings.MaxMindGeoIpDatabasePath;
            if (string.IsNullOrWhiteSpace(path))
                throw new Exception("MaxMind local database path is not configured");

            m_reader = new Reader(path);
            if (m_log.IsDebugEnabled)
                m_log.Debug($"{nameof(settings.MaxMindGeoIpDatabasePath)}='{path}'.");

            m_reader = new Reader(path, AccessMode);
        }

        public GeoLocation ResolveAddress(IPAddress ip)
        {
            var response = m_reader.Find<GeoLocationData>(ip);
            if (response == null)
                return null;

            var result = new GeoLocation
                {
                    Country = response.Country?.Names[DefaultLang],
                    City = response.City?.Names[DefaultLang]
                };
            var location = response.Location;
            if (location?.HasCoordinates == true)
                result.Point = new Point
                    {
                        // ReSharper disable PossibleInvalidOperationException
                        lat = location.Latitude.Value,
                        lon = location.Longitude.Value
                        // ReSharper restore PossibleInvalidOperationException
                    };

            return result;
        }

        private class NamedEntity
        {
            [Constructor]
            protected NamedEntity(
                IDictionary<string, string> names = null)
            {
                Names = names != null ? new Dictionary<string, string>(names) : new Dictionary<string, string>();
            }

            public IReadOnlyDictionary<string, string> Names { get; }
        }

        [UsedImplicitly]
        private sealed class Country : NamedEntity
        {
            [Constructor]
            public Country(
                IDictionary<string, string> names = null)
                : base(names)
            {
            }
        }

        [UsedImplicitly]
        private sealed class Location
        {
            [Constructor]
            public Location(
                double? latitude = null,
                double? longitude = null)
            {
                Latitude = latitude;
                Longitude = longitude;
            }

            public bool HasCoordinates => Latitude.HasValue && Longitude.HasValue;

            public double? Latitude { get; }

            public double? Longitude { get; }
        }

        [UsedImplicitly]
        private sealed class GeoLocationData
        {
            [Constructor]
            public GeoLocationData(
                Country country,
                NamedEntity city,
                Location location)
            {
                Country = country;
                City = city;
                Location = location;
            }

            public Country Country { get; }
            public NamedEntity City { get; }
            public Location Location { get; }
        }

        public void Dispose()
        {
            m_reader.Dispose();
        }
    }
}