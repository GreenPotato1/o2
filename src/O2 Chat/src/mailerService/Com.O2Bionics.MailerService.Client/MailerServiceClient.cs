using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Com.O2Bionics.MailerService.Client.Settings;
using Com.O2Bionics.MailerService.Contract;
using Com.O2Bionics.Utils;
using Com.O2Bionics.Utils.Network;
using JetBrains.Annotations;
using log4net;

namespace Com.O2Bionics.MailerService.Client
{
    public sealed class MailerServiceClient : IMailerServiceClient
    {
        private static readonly ILog m_log = LogManager.GetLogger(typeof(MailerServiceClient));
        private readonly HttpClient m_httpClient;
        private readonly string[] m_uris;

        public MailerServiceClient([NotNull] MailerServiceClientSettings settings)
        {
            if (null == settings)
                throw new ArgumentNullException(nameof(settings));
            m_uris = settings.Urls.Select(u => u.AbsoluteUri).ToArray();
            m_httpClient = new HttpClient();
        }

        public void Dispose()
        {
            m_httpClient.Dispose();
        }

        public async Task<SubjectBody> GenerateSubjectAndBody(MailRequest request)
        {
            if (null == request)
                throw new ArgumentNullException(nameof(request));

            const string path = "home/GenerateSubjectAndBody";
            var response = await HttpHelper.PostFirstSuccessfulForm(
                m_httpClient,
                m_uris,
                path,
                request,
                (url, exception) => m_log.Error($"{path} at '{url}'.", exception),
                "{0} attempts to call " + path + " have failed.");
            var result = response.JsonUnstringify2<SubjectBody>();
            return result;
        }

        public async Task<string> Send(MailRequest request)
        {
            if (null == request)
                throw new ArgumentNullException(nameof(request));

            const string path = "home/Send";
            try
            {
                var response = await HttpHelper.PostFirstSuccessfulForm(
                    m_httpClient,
                    m_uris,
                    path,
                    request,
                    (url, exception) => m_log.Error($"{path} at '{url}'.", exception),
                    "{0} attempts to call " + path + " have failed.");

                return string.IsNullOrEmpty(response) ? null : response;
            }
            catch (PostException e) when ((int)HttpStatusCode.BadRequest == e.HttpCode)
            {
                var result1 = e.Message;
                Debug.Assert(!string.IsNullOrEmpty(result1));
                return result1;
            }
        }
    }
}