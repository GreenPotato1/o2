using Com.O2Bionics.Tests.Common;
using Com.O2Bionics.Utils.JsonSettings;
using NUnit.Framework;

namespace Com.O2Bionics.ErrorTracker.Tests
{
    [SetUpFixture]
    public sealed class SetUpFixture : BaseSetUpFixture
    {
        [OneTimeSetUp]
        public override void SetUp()
        {
            var jsonSettingsReader = new JsonSettingsReader();
            var settings = jsonSettingsReader.ReadFromFile<TestSettings>();
            LogConfigurator.Configure(settings.ErrorTracker, TestConstants.ApplicationName);
        }
    }
}