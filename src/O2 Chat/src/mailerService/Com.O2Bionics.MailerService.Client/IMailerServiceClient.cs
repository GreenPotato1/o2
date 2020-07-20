using System;
using System.Threading.Tasks;
using Com.O2Bionics.MailerService.Contract;
using JetBrains.Annotations;

namespace Com.O2Bionics.MailerService.Client
{
    public interface IMailerServiceClient : IDisposable
    {
        /// <summary>
        /// Return error message.
        /// </summary>
        [ItemCanBeNull]
        Task<string> Send([NotNull] MailRequest request);
    }
}