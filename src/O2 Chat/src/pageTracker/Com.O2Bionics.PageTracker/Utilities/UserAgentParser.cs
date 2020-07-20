using System;
using Com.O2Bionics.PageTracker.Contract;
using JetBrains.Annotations;
using UAParser;

namespace Com.O2Bionics.PageTracker.Utilities
{
    [UsedImplicitly]
    public sealed class UserAgentParser : IUserAgentParser
    {
        private static readonly Parser m_parser = Parser.GetDefault();

        public UserAgentInfo Parse(string userAgentString)
        {
            if (string.IsNullOrWhiteSpace(userAgentString))
                throw new ArgumentException("Can't be null or whitespace", nameof(userAgentString));

            var result = m_parser.Parse(userAgentString);

            return new UserAgentInfo
                {
                    Device = result.Device.ToString(),
                    Os = result.OS.ToString(),
                    UserAgent = result.UserAgent.ToString(),
                    UserAgentString = userAgentString,
                };
        }
    }
}