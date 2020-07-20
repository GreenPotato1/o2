using Com.O2Bionics.PageTracker.Contract;
using JetBrains.Annotations;

namespace Com.O2Bionics.PageTracker
{
    public interface IUserAgentParser
    {
        [NotNull]
        UserAgentInfo Parse([NotNull] string userAgentString);
    }
}