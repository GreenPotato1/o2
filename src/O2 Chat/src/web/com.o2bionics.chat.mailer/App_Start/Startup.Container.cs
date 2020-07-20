using Com.O2Bionics.ErrorTracker;
using Com.O2Bionics.Utils;
using Com.O2Bionics.Utils.JsonSettings;

namespace Com.O2Bionics.MailerService.Web
{
    public static class Startup
    {
        public static void ConfigureContainer()
        {
            var mailerServiceSettings = new JsonSettingsReader().ReadFromFile<MailerServiceSettings>();
            GlobalContainer.RegisterInstance(mailerServiceSettings);
            GlobalContainer.RegisterInstance(mailerServiceSettings.ErrorTracker);

            GlobalContainer.RegisterInstance<IIdentifierReader>(new FakeIdentifierReader());
            LogConfigurator.Configure(mailerServiceSettings.ErrorTracker, "MailerService");

            GlobalContainer.RegisterType<INowProvider, DefaultNowProvider>();

            var emailSender = new EmailSender(mailerServiceSettings.Smtp);
            GlobalContainer.RegisterInstance<IEmailSender>(emailSender);
        }
    }
}