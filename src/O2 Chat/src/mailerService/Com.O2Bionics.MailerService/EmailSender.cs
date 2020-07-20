using System;
using System.Diagnostics;
using System.Net.Mail;
using System.Threading.Tasks;
using Com.O2Bionics.Utils.JsonSettings;
using JetBrains.Annotations;
using log4net;

namespace Com.O2Bionics.MailerService
{
    public sealed class EmailSender : IEmailSender
    {
        private static readonly ILog m_log = LogManager.GetLogger(typeof(EmailSender));

        private readonly string m_smtpServerHost;
        private readonly int m_smtpServerPort;
        private readonly string m_from;

        public EmailSender([NotNull] SmtpClientSettings settings)
        {
            if (null == settings)
                throw new ArgumentNullException(nameof(settings));

            m_smtpServerHost = settings.Host;
            m_smtpServerPort = settings.Port;
            m_from = settings.From;

            m_log.DebugFormat("initialized server={0}:{1}, from={2}", m_smtpServerHost, m_smtpServerPort, m_from);
        }

        public async Task Send(string to, string subject, string bodyHtml)
        {
            m_log.DebugFormat("sending to='{0}', subj='{1}', msg=!!!'{2}'!!!", to, subject, bodyHtml);
            Debug.Assert(!string.IsNullOrEmpty(to));
            Debug.Assert(!string.IsNullOrEmpty(subject));
            Debug.Assert(!string.IsNullOrEmpty(bodyHtml));

            using (var client = new SmtpClient())
            {
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.Host = m_smtpServerHost;
                client.Port = m_smtpServerPort;

                using (var message = new MailMessage(m_from, to))
                {
                    message.Subject = subject;
                    message.IsBodyHtml = true;
                    message.Body = bodyHtml;
                    await client.SendMailAsync(message).ConfigureAwait(false);
                }
            }
        }
    }
}