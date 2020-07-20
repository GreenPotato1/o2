using log4net.Config;
using NUnit.Framework;

namespace Com.O2Bionics.Tests.Common
{
    /// <summary>
    /// Base class for all tests using "log4net".
    /// <example>
    /// <code lang="C#">
    /// [SetUpFixture]
    /// public sealed class SetUpFixture : BaseSetUpFixture
    /// {}
    /// </code>
    /// </example>
    /// </summary>
    public class BaseSetUpFixture
    {
        [OneTimeSetUp]
        public virtual void SetUp()
        {
            XmlConfigurator.Configure();
        }
    }
}