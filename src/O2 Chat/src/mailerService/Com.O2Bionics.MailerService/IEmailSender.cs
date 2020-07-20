using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Com.O2Bionics.MailerService
{
    public interface IEmailSender
    {
        Task Send([NotNull] string to, [NotNull] string subject, [NotNull] string bodyHtml);
    }
}