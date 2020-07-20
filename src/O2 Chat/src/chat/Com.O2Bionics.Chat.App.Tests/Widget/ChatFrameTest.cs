using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Com.O2Bionics.AuditTrail.Client;
using Com.O2Bionics.AuditTrail.Contract;
using Com.O2Bionics.AuditTrail.Contract.Names;
using Com.O2Bionics.AuditTrail.Contract.Settings;
using Com.O2Bionics.Chat.App.Tests.Utilities;
using Com.O2Bionics.ChatService;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.Contract.AuditTrail;
using Com.O2Bionics.ChatService.Contract.Widget;
using Com.O2Bionics.ChatService.Impl;
using Com.O2Bionics.ChatService.Widget;
using Com.O2Bionics.ChatService.Widget.Properties;
using Com.O2Bionics.Tests.Common;
using Com.O2Bionics.Utils.JsonSettings;
using Com.O2Bionics.ChatService.Impl.AuditTrail;
using Com.O2Bionics.Elastic;
using Com.O2Bionics.Utils;
using FluentAssertions;
using JetBrains.Annotations;
using NUnit.Framework;

namespace Com.O2Bionics.Chat.App.Tests.Widget
{
    /// <summary>
    /// Call "chatframe.cshtml" via HTTP.
    /// </summary>
    [TestFixture]
    public sealed class ChatFrameTest : IDisposable
    {
        private const string GoodRefererHost = "some.name." + TestConstants.CustomerMainDomain;

        [SettingsRoot("workspace")]
        [UsedImplicitly]
        private sealed class Settings
        {
            [Required]
            public Uri WidgetUrl { get; [UsedImplicitly] set; }
        }

        private readonly ChatServiceSettings m_chatServiceSettings = new JsonSettingsReader().ReadFromFile<ChatServiceSettings>();
        private readonly AuditTrailServiceSettings m_auditTrailServiceSettings = new JsonSettingsReader().ReadFromFile<AuditTrailServiceSettings>();
        private readonly Settings m_settings = new JsonSettingsReader().ReadFromFile<Settings>();
        private readonly WidgetSettings m_widgetSettings = new JsonSettingsReader().ReadFromFile<WidgetSettings>();
        private readonly string m_indexName;
        private readonly ChatDatabaseFactory m_databaseFactory;
        private readonly TcpServiceClient<IVisitorChatService> m_tcpServiceClient;

        public ChatFrameTest()
        {
            m_indexName = IndexNameFormatter.Format(m_auditTrailServiceSettings.Index.Name, ProductCodes.Chat);
            m_databaseFactory = new ChatDatabaseFactory(m_chatServiceSettings);

            m_tcpServiceClient = new TcpServiceClient<IVisitorChatService>(
                m_widgetSettings.ChatServiceClient.Host,
                m_widgetSettings.ChatServiceClient.Port);
        }

        public void Dispose()
        {
            m_tcpServiceClient.Dispose();
        }

        [OneTimeSetUp]
        public void Setup()
        {
            RecreateElasticIndex();
            TearDown();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            ResetLoads();
        }

        [Test]
        [Order(1)]
        public async Task UnknownDomain()
        {
            var host = m_settings.WidgetUrl.Host + ".oh.yes";
            await UnknownDomainDirectWidgetRequest(host, "First");
            var firstEvent = UnknownDomainFromElastic(host);
            await UnknownDomainDirectWidgetRequest(host, "Second");
            CallReset();
            await UnknownDomainFromAts(firstEvent);
        }

        [NotNull]
        private AuditEvent<WidgetUnknownDomain> UnknownDomainFromElastic(string host)
        {
            var elasticClient = new EsClient(m_auditTrailServiceSettings.ElasticConnection);

            var result = elasticClient.FetchFirstDocument<AuditEvent<WidgetUnknownDomain>>(
                m_indexName,
                OperationKind.WidgetUnknownDomainKey).Value;
            var expected = new AuditEvent<WidgetUnknownDomain>(result)
                {
                    Operation = OperationKind.WidgetUnknownDomainKey,
                    Status = OperationStatus.AccessDeniedKey,
                    CustomerId = TestConstants.CustomerIdString,
                    NewValue = new WidgetUnknownDomain
                        {
                            Domains = TestConstants.CustomerDomains,
                            Name = host
                        },
                    Author = null,
                    FieldChanges = null,
                    Changed = null,
                    ObjectNames = null,
                    OldValue = null,
                };
            result.Should().BeEquivalentTo(expected, "Saved document.");
            return result;
        }

        private async Task UnknownDomainDirectWidgetRequest([NotNull] string host, [NotNull] string name)
        {
            var response = await ControllerClient.GetWidgetChatFrame(
                m_settings.WidgetUrl,
                host,
                TestConstants.CustomerIdString,
                HttpStatusCode.BadRequest);

            var message = string.Format(Resources.WidgetLoadedFromUnregisteredDomainError1, host);
            var expected = ChatFrameHelper.FormatError(message);
            Assert.AreEqual(expected, response, name + " response");
        }

        private async Task UnknownDomainFromAts([NotNull] AuditEvent<WidgetUnknownDomain> firstEvent)
        {
            using (var auditTrailClient = new AuditTrailClient(m_chatServiceSettings.AuditTrailClient, new DefaultNowProvider(), ProductCodes.Chat))
            {
                var filter = new Filter(ProductCodes.Chat, 123)
                    {
                        Operations = new List<string> { OperationKind.WidgetUnknownDomainKey },
                        CustomerId = TestConstants.CustomerIdString,
                    };
                var facets = await auditTrailClient.SelectFacets(filter);
                Assert.IsNotNull(facets, nameof(facets));
                Assert.IsNotNull(facets.RawDocuments, nameof(facets.RawDocuments));
                if (1 != facets.RawDocuments.Count)
                    throw new Exception(
                        $"Cache might be not working - Facets.RawDocuments ({facets.RawDocuments.Count}) must have 1 document: '{facets.RawDocuments.JoinAsString()}'.");

                var rawDocument = facets.RawDocuments[0];
                AuditEvent<WidgetUnknownDomain> actual;
                try
                {
                    actual = rawDocument.JsonUnstringify2<AuditEvent<WidgetUnknownDomain>>();
                }
                catch (Exception e)
                {
                    throw new Exception($"Error parsing rawDocument='{rawDocument}'.", e);
                }

                firstEvent.All = firstEvent.Changed = null;
                actual.Should().BeEquivalentTo(firstEvent, "Events");
            }
        }

        [Test]
        [Order(2)]
        public async Task Success()
        {
            //ValuesAttribute won't run the "Setup()" for each run.
            await SuccessImpl(false);
        }

        [Test]
        [Order(2)]
        public async Task SuccessDemoMode()
        {
            await SuccessImpl(true);
        }

        private async Task SuccessImpl(bool isDemoMode)
        {
            var actual = await ControllerClient.GetWidgetChatFrame(
                m_settings.WidgetUrl,
                isDemoMode ? m_chatServiceSettings.WorkspaceUrl.Authority : GoodRefererHost,
                TestConstants.CustomerIdString,
                HttpStatusCode.OK,
                isDemoMode);
            var words = new[]
                {
                    "<title>O2Chat</title>",
                    "isDemo: " + isDemoMode.AsJavaScriptString(),
                    "customerId: " + TestConstants.CustomerIdString,
                    "visitorId: '0'",
                    "isSessionStarted: false",
                    "pageTrackerUrl: '" + m_widgetSettings.PageTrackerClient.Url.AbsoluteUri + "'",
                    "isChatEnabled: true",
                    "AvatarConstants.defaultAvatarsBaseUrl = '" + m_widgetSettings.WorkspaceUrl.AbsoluteUri + "'",
                };

            var builder = new StringBuilder();
            // ReSharper disable once ForCanBeConvertedToForeach
            // ReSharper disable once LoopCanBeConvertedToQuery
            for (var i = 0; i < words.Length; i++)
            {
                var word = words[i];
                if (!actual.Contains(word))
                    builder.AppendLine(word);
            }

            var errors = builder.ToString();
            errors.Should().BeEquivalentTo(string.Empty, nameof(words));

            CallReset();
            var widgetLoads = m_databaseFactory.SumWidgetLoads();
            Assert.AreEqual(isDemoMode ? 0 : 1, widgetLoads, nameof(widgetLoads));
        }

        [Test]
        [Order(2)]
        public async Task DisabledCustomer()
        {
            const string customerId = "123789456";
            var actual = await ControllerClient.GetWidgetChatFrame(m_settings.WidgetUrl, GoodRefererHost, customerId, HttpStatusCode.BadRequest);
            var message = string.Format(Resources.CustomerMustBeActiveError1, customerId);
            var expected = ChatFrameHelper.FormatError(message);
            Assert.AreEqual(expected, actual, "Response");
        }

        private void RecreateElasticIndex()
        {
            var client = new EsClient(m_auditTrailServiceSettings.ElasticConnection);
            client.DeleteIndex(m_indexName);
            var indexSettings = new EsIndexSettings(m_auditTrailServiceSettings.Index, m_indexName);
            AuditIndexHelper.CreateIndex(client, indexSettings);
        }

        private void ResetLoads()
        {
            const int resetCount = 2;
            for (var i = 0; i < resetCount; i++)
            {
                m_databaseFactory.ClearWidgetLoadTable();
                CallReset();
            }
        }

        private void CallReset()
        {
            var customerIds = new[] { TestConstants.CustomerId };
            long errors = 0;
            m_tcpServiceClient.Call(s => { errors += s.ResetWidgetLoads(customerIds); });
            Assert.AreEqual(0, errors, nameof(errors));
        }
    }
}