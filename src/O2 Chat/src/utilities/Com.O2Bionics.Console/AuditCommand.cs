using System.Collections.Generic;
using Com.O2Bionics.AuditTrail.Contract.Names;
using Com.O2Bionics.AuditTrail.Contract.Settings;
using Com.O2Bionics.ChatService.Contract.AuditTrail;
using Com.O2Bionics.ChatService.Impl.AuditTrail;
using Com.O2Bionics.Utils.JsonSettings;

namespace Com.O2Bionics.Console
{
    public sealed class AuditCommand : IndexBaseCommand, ICommand
    {
        private string m_index;
        private AuditTrailServiceSettings m_settings;

        public AuditCommand() : base("--create-index-audit", "--recreate-index-audit")
        {
        }

        protected override EsConnectionSettings ElasticConnection => m_settings.ElasticConnection;
        protected override List<string> Indices => new List<string> { m_index };

        protected override void ReadSettings(JsonSettingsReader reader)
        {
            m_settings = reader.ReadFromFile<AuditTrailServiceSettings>();
            m_index = IndexNameFormatter.FormatWithValidation(m_settings.Index.Name, ProductCodes.Chat);
        }

        protected override void CreateIndex()
        {
            var indexSettings = new EsIndexSettings(m_settings.Index, m_index);
            AuditIndexHelper.CreateIndex(Client, indexSettings);
        }
    }
}