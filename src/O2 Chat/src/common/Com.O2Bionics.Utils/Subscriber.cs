using System;

namespace Com.O2Bionics.Utils
{
    public class Subscriber : IEquatable<Subscriber>
    {
        public Subscriber(string host, int port)
        {
            if (string.IsNullOrWhiteSpace(host))
                throw new ArgumentException("Can't be null or whitespace", "host");
            if (port <= 0)
                throw new IndexOutOfRangeException("Port value can't be less or equal to 0 but is " + port);

            Host = host;
            Port = port;
        }

        public Subscriber(string endpoint)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
                throw new ArgumentException("Can't be null or whitespace", "endpoint");
            var parts = endpoint.Split(':');
            if (parts.Length != 2)
                throw new ArgumentException("Invalid format", endpoint);
            if (string.IsNullOrWhiteSpace(parts[0]))
                throw new ArgumentException("Host can't be null or whitespace", "endpoint");
            int port;
            if (!Int32.TryParse(parts[1], out port))
                throw new ArgumentException("Invalid Port value", "endpoint");
            if (port <= 0)
                throw new IndexOutOfRangeException("Port value can't be less or equal to 0 but is " + port);

            Host = parts[0];
            Port = port;
        }

        public string Host { get; private set; }
        public int Port { get; private set; }

        public override string ToString()
        {
            return Host + ":" + Port;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Host.ToLowerInvariant().GetHashCode() * 397) ^ Port;
            }
        }

        public bool Equals(Subscriber other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return StringComparer.OrdinalIgnoreCase.Equals(Host, other.Host) && Port == other.Port;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Subscriber)obj);
        }
    }
}