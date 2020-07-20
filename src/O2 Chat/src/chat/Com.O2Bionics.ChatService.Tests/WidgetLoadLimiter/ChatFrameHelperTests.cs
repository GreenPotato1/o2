using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Web;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.Contract.WidgetAppearance;
using Com.O2Bionics.ChatService.Widget;
using Com.O2Bionics.ChatService.Widget.Properties;
using Com.O2Bionics.Tests.Common;
using Com.O2Bionics.Utils;
using FluentAssertions;
using JetBrains.Annotations;
using NSubstitute;
using NUnit.Framework;
using TErrorValue = System.Collections.Generic.KeyValuePair<string, Com.O2Bionics.ChatService.Contract.ChatFrameLoadResult>;
using TCacheEntry = System.ValueTuple<System.DateTime, uint, Com.O2Bionics.ChatService.Contract.CustomerEntry>;

namespace Com.O2Bionics.ChatService.Tests.WidgetLoadLimiter
{
    [TestFixture]
    [Parallelizable]
    public sealed class ChatFrameHelperTests
    {
        private const string GoodDomain = "some.Site.COM",
            Protocol = "http://",
            GoodUrl = Protocol + GoodDomain,
            BadDomainSuffix = "pot",
            BadDomain = "some.site.com" + BadDomainSuffix,
            BadUrl = Protocol + BadDomain;

        private readonly DateTime m_date = DateTime.UtcNow.RemoveTime();

        private readonly CustomerSettingsInfo m_goodSettingsInfo = new CustomerSettingsInfo
            {
                IsEnabled = true,
                ChatWidgetAppearanceInfo = new ChatWidgetAppearanceInfo { Domains = GoodDomain, AppearanceData = new ChatWidgetAppearance() }
            };

        private readonly List<TCacheEntry> m_customerCacheListExpected = new List<TCacheEntry>();
        private readonly List<TCacheEntry> m_customerCacheList = new List<TCacheEntry>();
        private readonly ICustomerCache m_customerCacheSubstitute = Substitute.For<ICustomerCache>();
        private readonly CustomerCache m_customerCache = new CustomerCache();
        private readonly Uri m_goodUri = new Uri(GoodUrl);
        private readonly HttpRequestBase m_request = Substitute.For<HttpRequestBase>();
        private readonly HttpResponseBase m_response = Substitute.For<HttpResponseBase>();
        private readonly ITcpServiceClient<IVisitorChatService> m_client = Substitute.For<ITcpServiceClient<IVisitorChatService>>();

        private readonly ValueHolder<Uri> m_uriHolder = new ValueHolder<Uri>();
        private readonly ValueHolder<string> m_errorHolder = new ValueHolder<string>();
        private readonly ValueHolder<ChatFrameLoadResult> m_returnValueHolder = new ValueHolder<ChatFrameLoadResult>();
        private readonly ValueHolder<HttpStatusCode> m_statusCodeHolder = new ValueHolder<HttpStatusCode>();

        public ChatFrameHelperTests()
        {
            m_request.UrlReferrer.Returns(_ => m_uriHolder.Instance);
            m_client.Call(Arg.Any<Func<IVisitorChatService, ChatFrameLoadResult>>()).Returns(s => m_returnValueHolder.Instance);
            m_client.Call(Arg.Any<Func<IVisitorChatService, List<uint>>>()).Returns(
                s =>
                    new List<uint> { TestConstants.CustomerId });

            m_response.WhenForAnyArgs(s => s.Write(Arg.Any<string>())).Do(
                s =>
                    {
                        var er = s.Arg<string>();
                        Assert.IsNotNull(er, nameof(er));
                        Assert.IsNotEmpty(er, nameof(er));
                        m_errorHolder.Instance += er;
                    });
            m_response.StatusCode = Arg.Do<int>(s => { m_statusCodeHolder.Instance = (HttpStatusCode)s; });

            m_customerCacheSubstitute.WhenForAnyArgs(
                s => s.SetSoft(Arg.Any<DateTime>(), Arg.Any<uint>(), Arg.Any<Action<CustomerEntry>>())).Do(
                s =>
                    {
                        var date = s.Arg<DateTime>();
                        var customerId = s.Arg<uint>();
                        var action = s.Arg<Action<CustomerEntry>>();
                        var entry = new CustomerEntry { Active = true };
                        action(entry);

                        m_customerCacheList.Add(new TCacheEntry(date, customerId, entry));
                        m_customerCache.SetSoft(date, customerId, action);
                    });
            m_customerCacheSubstitute.IsActive(Arg.Any<uint>()).Returns(
                s =>
                    {
                        var customerId = s.Arg<uint>();
                        return TestConstants.CustomerId == customerId;
                    });

            GlobalContainer.RegisterInstance(m_customerCacheSubstitute);
            GlobalContainer.RegisterInstance(m_client);
        }

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            await m_customerCache.Load();
        }

        [SetUp]
        public void Setup()
        {
            m_customerCache.Clear();
            m_customerCacheList.Clear();
            m_customerCacheSubstitute.Clear();

            m_uriHolder.Instance = m_goodUri;
            m_errorHolder.Instance = null;
            m_returnValueHolder.Instance = null;
            m_statusCodeHolder.Instance = HttpStatusCode.OK;
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            GlobalContainer.Clear();
        }

        [Test]
        public void DisabledCustomer()
        {
            m_returnValueHolder.Instance = new ChatFrameLoadResult { Code = WidgetLoadSatusCode.DisabledCustomer, };
            var message = string.Format(Resources.CustomerMustBeActiveError1, TestConstants.CustomerId);
            var expected = new TErrorValue(ChatFrameHelper.FormatError(message), null);
            SetExpectedCache(new CustomerEntry { Active = false });
            Call(expected);
        }

        [Test]
        public void NoDomain()
        {
            m_uriHolder.Instance = null;
            var expected = new TErrorValue(ChatFrameHelper.FormatError(Resources.CannotDetermineDomainError), null);
            Call(expected);
        }

        [Test]
        public void Overload()
        {
            m_returnValueHolder.Instance = new ChatFrameLoadResult { Code = WidgetLoadSatusCode.ViewCounterExceeded, CustomerSettings = m_goodSettingsInfo };
            var expected = new TErrorValue(ChatFrameHelper.FormatError(Resources.TooManyChatWidgetLoads), null);
            SetExpectedCache(new CustomerEntry { Active = true, ViewCounterExceeded = true });
            Call(expected);
        }

        [Test]
        public void UnknownDomain()
        {
            m_uriHolder.Instance = new Uri(BadUrl);
            m_returnValueHolder.Instance = new ChatFrameLoadResult { Code = WidgetLoadSatusCode.UnknownDomain, CustomerSettings = m_goodSettingsInfo };
            var message = string.Format(Resources.WidgetLoadedFromUnregisteredDomainError1, BadDomain);
            var expected = new TErrorValue(ChatFrameHelper.FormatError(message), null);
            var customerEntry = new CustomerEntry
                {
                    Active = true,
                    Domains = new[] { GoodDomain },
                    UnknownDomains = new ConcurrentHashSet<string>(BadDomain)
                };

            SetExpectedCache(customerEntry);
            Call(expected);
        }

        [Test]
        public void UnknownDomainTooMany()
        {
            m_returnValueHolder.Instance =
                new ChatFrameLoadResult { Code = WidgetLoadSatusCode.UnknownDomainNumberExceeded, CustomerSettings = m_goodSettingsInfo };
            var expected = new TErrorValue(ChatFrameHelper.FormatError(Resources.TooManyUnknownDomainsError), null);
            SetExpectedCache(new CustomerEntry { Active = true, UnknownDomainNumberExceeded = true });
            Call(expected);
        }

        [Test]
        public void GoodCase()
        {
            m_returnValueHolder.Instance =
                new ChatFrameLoadResult { Code = WidgetLoadSatusCode.Allowed, HasActiveSession = true, CustomerSettings = m_goodSettingsInfo };
            var expected = new TErrorValue(null, new ChatFrameLoadResult { HasActiveSession = true, CustomerSettings = m_goodSettingsInfo });
            Call(expected, HttpStatusCode.OK);
        }

        private void SetExpectedCache([NotNull] CustomerEntry customerEntry)
        {
            m_customerCacheListExpected.Clear();
            m_customerCacheListExpected.Add(new TCacheEntry(m_date, TestConstants.CustomerId, customerEntry));
        }

        private void Call(TErrorValue expected, HttpStatusCode statusCode = HttpStatusCode.BadRequest, [CallerMemberName] string memberName = "")
        {
            var name = "_" + memberName;
            var load = ChatFrameHelper.Load(m_request, TestConstants.CustomerId, 123789, m_date, false, null, m_response);
            var actual = new TErrorValue(m_errorHolder.Instance, load);
            actual.Should().BeEquivalentTo(expected, nameof(actual) + name);
            RemoveWhenFixed(name);
            m_customerCacheListExpected.Clear();
            Assert.AreEqual(statusCode, m_statusCodeHolder.Instance, nameof(statusCode) + name);
        }

        private void RemoveWhenFixed(string name)
        {
            //FluentAssertions does not work with ValueTuple - remove when fixed.
            var isLegacy = 1 == m_customerCacheList.Count && 1 == m_customerCacheListExpected.Count;
            if (isLegacy)
            {
                (var date, var id, var entry) = m_customerCacheList[0];
                (var dateExpected, var idExpected, var entryExpected) = m_customerCacheListExpected[0];

                date.Should().Be(dateExpected, nameof(date) + name);
                id.Should().Be(idExpected, nameof(id) + name);
                entry.Should().BeEquivalentTo(entryExpected, nameof(entry) + name);
                return;
            }

            m_customerCacheList.Should().BeEquivalentTo(m_customerCacheListExpected, nameof(m_customerCacheList) + name);
        }
    }
}