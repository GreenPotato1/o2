using System;
using Com.O2Bionics.AuditTrail.Client.Settings;
using Com.O2Bionics.AuditTrail.Contract.Names;
using Com.O2Bionics.AuditTrail.Contract.Settings;
using Com.O2Bionics.ChatService.Contract.AuditTrail;
using Com.O2Bionics.ChatService.Impl.AuditTrail;
using Com.O2Bionics.Elastic;
using Com.O2Bionics.Tests.Common;
using Com.O2Bionics.Utils.JsonSettings;
using NUnit.Framework;

namespace Com.O2Bionics.AuditTrail.Tests
{
    public abstract class BaseElasticTest
    {
        protected static readonly DateTime UtcNow = TestNowProvider.UtcNowWithoutMilliseconds();

        protected readonly AuditTrailService Service;
        protected readonly EsClient ElasticClient;
        private readonly EsIndexSettings m_auditIndexSettings;
        private AuditTrailClientSettings m_clientSettings;

        protected BaseElasticTest()
        {
            ServiceSettings = new JsonSettingsReader().ReadFromFile<AuditTrailServiceSettings>();
            IndexName = IndexNameFormatter.Format(ServiceSettings.Index.Name, ProductCodes.Chat);
            m_auditIndexSettings = new EsIndexSettings(ServiceSettings.Index, IndexName);
            ElasticClient = new EsClient(ServiceSettings.ElasticConnection);
            Service = new AuditTrailService(ServiceSettings, ElasticClient);
        }

        protected AuditTrailServiceSettings ServiceSettings { get; }

        protected AuditTrailClientSettings ClientSettings =>
            m_clientSettings ?? (m_clientSettings = new JsonSettingsReader().ReadFromFile<AuditTrailClientSettings>());

        protected string IndexName { get; }

        [OneTimeSetUp]
        public void Setup()
        {
            Clear();

            AuditIndexHelper.CreateIndex(ElasticClient, m_auditIndexSettings);
            ContinueSetup();
        }

        protected virtual void ContinueSetup()
        {
        }

        protected void Clear()
        {
            ElasticClient.DeleteIndex(IndexName);
        }
    }
}