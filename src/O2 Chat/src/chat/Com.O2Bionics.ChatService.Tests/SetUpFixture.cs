using Com.O2Bionics.Tests.Common;
using log4net;
using NUnit.Framework;

namespace Com.O2Bionics.ChatService.Tests
{
    [SetUpFixture]
    public sealed class SetUpFixture : BaseSetUpFixture
    {
        public override void SetUp()
        {
            base.SetUp();

            var log = LogManager.GetLogger(GetType());
            log.Info("Start recreating the Test Chat Database.");

            var database = new DatabaseTestBase();
            database.SetUp();

            log.Info("Done recreating the Test Chat Database.");
        }
    }
}