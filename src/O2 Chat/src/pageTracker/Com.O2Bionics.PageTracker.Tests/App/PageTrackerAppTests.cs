using System;
using System.Net.Http;
using System.Threading.Tasks;
using Com.O2Bionics.PageTracker.Contract;
using Com.O2Bionics.PageTracker.Tests.App.Contract;
using Com.O2Bionics.Tests.Common;
using Com.O2Bionics.Utils;
using Com.O2Bionics.Utils.JsonSettings;
using Com.O2Bionics.Utils.Network;
using Com.O2Bionics.Utils.Web.Settings;
using JetBrains.Annotations;
using log4net;
using NUnit.Framework;

namespace Com.O2Bionics.PageTracker.Tests.App
{
    [TestFixture]
    public sealed class PageTrackerAppTests
    {
        private readonly ILog m_log = LogManager.GetLogger(typeof(PageTrackerAppTests));

        private readonly PageTrackerClientSettings m_clientSettings =
            new JsonSettingsReader().ReadFromFile<PageTrackerClientSettings>();

        private readonly PageTrackerSettings m_settings =
            new JsonSettingsReader().ReadFromFile<PageTrackerSettings>();

        [Test]
        public async Task Test()
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("origin", m_settings.WidgetUrl.AbsoluteUri);
                var referrer = new Uri(
                    new Uri(m_settings.WidgetUrl.AbsoluteUri),
                    TestConstants.ChatFramePathQuery + TestConstants.CustomerIdString);
                httpClient.DefaultRequestHeaders.Add("referrer", referrer.ToString());

                var request = new AddRequest
                    {
                        cid = TestConstants.CustomerId,
                        ct = "some+custom+text, +1, +2",
                        tzde = "GMT+0300+(Russia+TZ+2+Standard+Time)",
                        tzof = 180,
                        u = "https://net.customer/",
                        vid = 0,
                    };

                var addResponse1 = await CallAdd(httpClient, request);
                Assert.IsNotNull(addResponse1, nameof(addResponse1));
                Assert.Greater(addResponse1.vid, 0, nameof(addResponse1.vid));
                Assert.IsNotNull(addResponse1.hid, nameof(addResponse1.hid));
                Assert.IsNotEmpty(addResponse1.hid, nameof(addResponse1.hid));

                request.vid = addResponse1.vid;
                var addResponse2 = await CallAdd(httpClient, request);
                Assert.IsNotNull(addResponse2, "second " + nameof(addResponse2));
                Assert.AreEqual(addResponse1.vid, addResponse2.vid, "second " + nameof(addResponse2.vid));
                Assert.AreNotEqual(addResponse2.hid, addResponse1.hid, "second " + nameof(addResponse2.hid));
            }
        }

        [NotNull]
        private async Task<AddResponse> CallAdd([NotNull] HttpClient httpClient, [NotNull] AddRequest request)
        {
            if (null == request)
                throw new ArgumentNullException(nameof(request));

            var response = await HttpHelper.PostFirstSuccessfulForm(
                httpClient,
                new[] { m_clientSettings.Url.AbsoluteUri },
                PageTrackerConstants.AddCommand,
                request,
                (url, exception) => m_log.Error($"{nameof(url)}='{url}'.", exception),
                "{0} attempts to call tr." + PageTrackerConstants.AddCommand + "  have failed.");
            if (string.IsNullOrEmpty(response))
                throw new Exception("PageTracker.Add must have returned not empty response.");

            m_log.DebugFormat("response: {0}", response);
            var result = response.JsonUnstringify2<AddResponse>();
            if (null == result)
                throw new Exception($"PageTracker.Add must have returned not null {nameof(AddResponse)}.");
            if (result.vid <= 0 || string.IsNullOrWhiteSpace(result.hid))
                throw new Exception($"PageTracker.Add must have returned valid {nameof(AddResponse)}({result}).");
            return result;
        }
    }
}