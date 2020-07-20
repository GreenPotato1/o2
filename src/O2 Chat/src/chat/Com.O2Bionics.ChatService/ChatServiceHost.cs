using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Com.O2Bionics.AuditTrail.Client;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.Contract.AuditTrail;
using Com.O2Bionics.ChatService.Impl;
using Com.O2Bionics.ChatService.Impl.Storage;
using Com.O2Bionics.FeatureService.Client;
using Com.O2Bionics.MailerService.Client;
using Com.O2Bionics.Utils;
using Com.O2Bionics.Utils.JsonSettings;
using log4net;
using Unity;
using Unity.Lifetime;

namespace Com.O2Bionics.ChatService
{
    public class ChatServiceHost : IDisposable
    {
        private static readonly ILog m_log = LogManager.GetLogger(typeof(ChatServiceHost));

        private readonly List<TcpServiceHost> m_hosts = new List<TcpServiceHost>();

        private IUnityContainer Container { get; }

        public ChatServiceHost(IUnityContainer container = null)
        {
            Container = container ?? new UnityContainer();

            RegisterSingletonIfMissing(() => new JsonSettingsReader().ReadFromFile<ChatServiceSettings>());
            var settings = Container.Resolve<ChatServiceSettings>();
            RegisterSingletonIfMissing(() => settings.FeatureServiceClient);

            RegisterSingletonIfMissing(() => settings.MailerServiceClient);
            RegisterSingletonIfMissing<IMailerServiceClient, MailerServiceClient>();
            RegisterSingletonIfMissing<INowProvider, DefaultNowProvider>();
            RegisterSingletonIfMissing<IChatDatabaseFactory, ChatDatabaseFactory>();

            RegisterSingletonIfMissing<ISettingsStorage, SettingsStorage>();
            RegisterSingletonIfMissing<ICustomerStorage, CustomerStorage>();
            RegisterSingletonIfMissing<IUserStorage, UserStorage>();
            RegisterSingletonIfMissing<IDepartmentStorage, DepartmentStorage>();
            RegisterSingletonIfMissing<IVisitorStorage, VisitorStorage>();
            RegisterSingletonIfMissing<IAgentSessionStorage, AgentSessionStorage>();
            RegisterSingletonIfMissing<IChatSessionStorage, ChatSessionStorage>();
            RegisterSingletonIfMissing<IObjectResolver, ObjectResolver>();

            RegisterSingletonIfMissing<IDbUpdater, DbUpdater>();

            RegisterSingletonIfMissing<ISubscriptionManager, SubscriptionManager>();

            RegisterSingletonIfMissing<IAgentManager, AgentManager>();
            RegisterSingletonIfMissing<IAccessManager, AccessManager>();
            RegisterSingletonIfMissing<IUserManager, UserManager>();
            RegisterSingletonIfMissing<IChatSessionManager, ChatSessionManager>();

            RegisterSingletonIfMissing<IVisitorChatService, VisitorChatService>();
            RegisterSingletonIfMissing<IAgentConsoleService, AgentConsoleService>();
            RegisterSingletonIfMissing<IManagementService, ManagementService>();

            RegisterSingletonIfMissing<IChatWidgetAppearanceManager, ChatWidgetAppearanceManager>();
            RegisterSingletonIfMissing<ICannedMessageStorage, CannedMessageStorage>();

            var featureServiceClient = new FeatureServiceClient(
                settings.FeatureServiceClient,
                MemoryCache.Default,
                Container.Resolve<INowProvider>());
            Container.RegisterInstance<IFeatureServiceClient>(featureServiceClient);

            var auditSettings = Container.Resolve<ChatServiceSettings>().AuditTrailClient;
            RegisterSingletonIfMissing(() => auditSettings);

            var auditTrailClient = new AuditTrailClient(auditSettings, Container.Resolve<INowProvider>(), ProductCodes.Chat);
            Container.RegisterInstance<IAuditTrailClient>(auditTrailClient);

            RegisterSingletonIfMissing<IUnknownDomainLoader, UnknownDomainLoader>();
            RegisterSingletonIfMissing(() => settings.WidgetLoadLimiterSettings);
            RegisterSingletonIfMissing<ICustomerWidgetLoadStorage, CustomerWidgetLoadStorage>();
            RegisterSingletonIfMissing<IWidgetLoadUnknownDomainStorage, WidgetLoadUnknownDomainStorage>();
            RegisterSingletonIfMissing<IWidgetLoadCounterStorage, WidgetLoadCounterStorage>();
            RegisterSingletonIfMissing<ICustomerCacheNotifier, CustomerCacheNotifier>();
        }

        public void Start()
        {
            // create instances and initialize in required order
            var settings = Container.Resolve<ChatServiceSettings>();

            m_log.DebugFormat(
                "starting host on port={1}{0}  db={2}",
                Environment.NewLine,
                settings.WcfBindPort,
                settings.Database);

            using (var dc = Container.Resolve<IChatDatabaseFactory>().CreateContext())
            {
                Container.Resolve<ISettingsStorage>().Load(dc);
                Container.Resolve<ISubscriptionManager>().Load(dc);
                dc.Commit();
            }

            LoadData();

            OpenHost<IVisitorChatService>(Container, settings.WcfBindPort);
            OpenHost<IAgentConsoleService>(Container, settings.WcfBindPort);
            OpenHost<IManagementService>(Container, settings.WcfBindPort);
        }

        private void LoadData()
        {
            var loadTasks = new[]
                {
                    Task.Run(
                        () =>
                            {
                                var dbUpdater = Container.Resolve<IDbUpdater>();
                                dbUpdater.Load();

                                Container.Resolve<IAgentManager>().Start();
                                dbUpdater.Start();
                            }),
                    Task.Run(
                        () =>
                            {
                                var unknownDomainAuditService = Container.Resolve<IWidgetLoadUnknownDomainStorage>();
                                unknownDomainAuditService.Load().WaitAndUnwrapException();
                            }),
                    Task.Run(
                        () =>
                            {
                                var pageLoadService = Container.Resolve<IWidgetLoadCounterStorage>();
                                pageLoadService.Load().WaitAndUnwrapException();
                            }),
                };
            Task.WhenAll(loadTasks).WaitAndUnwrapException();

            var customerCacheNotify = Container.Resolve<ICustomerCacheNotifier>();
            {
                var unknownDomainStorage = Container.Resolve<IWidgetLoadUnknownDomainStorage>();
                unknownDomainStorage.SetNotifier(customerCacheNotify);
            }
            {
                var widgetLoadStorage = Container.Resolve<IWidgetLoadCounterStorage>();
                widgetLoadStorage.SetNotifier(customerCacheNotify);
            }
        }

        public void Stop()
        {
            var settings = Container.Resolve<ChatServiceSettings>();
            m_log.InfoFormat("stopping host on port={0}", settings.WcfBindPort);

            foreach (var host in m_hosts) host.Dispose();
            m_hosts.Clear();

            var agentManager = Container.Resolve<IAgentManager>();
            agentManager.Stop();

            var dbUpdater = Container.Resolve<IDbUpdater>();
            dbUpdater.Stop();
        }

        public void Dispose()
        {
            Container.Dispose();
        }

        private void RegisterSingletonIfMissing<T1, T2>() where T2 : T1
        {
            if (!Container.IsRegistered<T1>())
                Container.RegisterType<T1, T2>(new ContainerControlledLifetimeManager());
        }

        private void RegisterSingletonIfMissing<T1>(Func<T1> factory)
        {
            if (!Container.IsRegistered<T1>())
                Container.RegisterInstance(factory());
        }

        private void OpenHost<T>(IUnityContainer container, int port) where T : class
        {
            var instance = container.Resolve<T>();
            m_hosts.Add(new TcpServiceHost<T>(instance, port));
        }
    }
}