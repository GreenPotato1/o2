using System.Threading.Tasks;
using Com.O2Bionics.Chat.App.Tests.Utilities;
using Com.O2Bionics.ChatService;
using Com.O2Bionics.Tests.Common;
using Com.O2Bionics.Utils.JsonSettings;
using JetBrains.Annotations;
using NUnit.Framework;

namespace Com.O2Bionics.Chat.App.Tests
{
    public class BaseControllerTest
    {
        protected readonly string Server;
        protected readonly ChatServiceSettings Settings = new JsonSettingsReader().ReadFromFile<ChatServiceSettings>();

        private CookiesAndToken m_cookieAndTokenCache;

        protected BaseControllerTest()
        {
            Assert.IsNotNull(Settings, nameof(Settings));

            Server = Settings.WorkspaceUrl.Host;
            Assert.IsNotEmpty(Server, nameof(Server));
        }

        [ItemNotNull]
        protected async Task<CookiesAndToken> Login(bool skipCache = false, bool shallSendToken = true)
        {
            if (skipCache || null == m_cookieAndTokenCache)
            {
                var result = await ControllerClient.Login(Server, TestConstants.TestUserEmail1, TestConstants.TestUserPassword1, shallSendToken).ConfigureAwait(false);
                if (skipCache)
                    return result;

                m_cookieAndTokenCache = result;
            }

            return m_cookieAndTokenCache;
        }
    }
}