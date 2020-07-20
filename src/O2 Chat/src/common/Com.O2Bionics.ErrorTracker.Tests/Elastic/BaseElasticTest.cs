using System;
using System.Threading.Tasks;
using Com.O2Bionics.Elastic;
using Com.O2Bionics.Tests.Common;
using Com.O2Bionics.Utils.JsonSettings;
using Nest;
using NUnit.Framework;

namespace Com.O2Bionics.ErrorTracker.Tests.Elastic
{
    public class BaseElasticTest : IDisposable
    {
        protected const string Prefix = "Message";
        protected const string ExistingMessage = Prefix + "1";

        protected ErrorService Service { get; private set; }
        protected string IndexName => m_settings.ErrorTracker.Index.Name;
        private readonly TestSettings m_settings = new JsonSettingsReader().ReadFromFile<TestSettings>();
        private IEmergencyWriter m_emergencyWriter;

        public void Dispose()
        {
            var service = Service;
            if (null != service)
            {
                service.Dispose();
                Service = null;
            }
        }

        [OneTimeSetUp]
        public void SetUp()
        {
            DeleteIndex();
            CreateIndex();
        }

        protected virtual void CreateIndex()
        {
            var settings = m_settings.ErrorTracker;
            ErrorTrackerIndexHelper.CreateIndex(new EsClient(settings.ElasticConnection), settings.Index);
        }

        private void DeleteIndex()
        {
            Dispose();

            if (null == m_emergencyWriter)
                m_emergencyWriter = EmergencyWriterFactory.CreateAndRegister(m_settings.ErrorTracker, TestConstants.ApplicationName);

            var client = new EsClient(m_settings.ErrorTracker.ElasticConnection);
            Service = new ErrorService(m_emergencyWriter, m_settings.ErrorTracker, client, TestConstants.ApplicationName);

            client.DeleteIndex(m_settings.ErrorTracker.Index.Name);
        }

        protected IEsClient GetClient()
        {
            var client = Service.Client;
            Assert.IsNotNull(client, nameof(GetClient));
            return client;
        }

        protected async Task SelectByMessage(string message, int expectedCount, string errorMessage)
        {
            QueryContainer Query(QueryContainerDescriptor<ErrorInfo> q) => q.Match(m => m.Field(f => f.Message).Query(message));
            await SelectTest(Query, expectedCount, errorMessage);
        }

        protected async Task SelectTest(Func<QueryContainerDescriptor<ErrorInfo>, QueryContainer> query, int expectedCount, string errorMessage)
        {
            const int maxCount = 1000;
            var client = Service.Client;
            var searchResponse = await client.SearchAsync<ErrorInfo>(
                IndexName,
                s => s
                    .Index(IndexName)
                    .From(0)
                    .Size(maxCount)
                    .Query(query));
            Assert.AreEqual(expectedCount, searchResponse.Documents.Count, errorMessage);
        }
    }
}