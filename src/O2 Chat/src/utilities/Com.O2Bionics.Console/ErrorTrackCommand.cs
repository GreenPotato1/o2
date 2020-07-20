using System.Collections.Generic;
using Com.O2Bionics.ErrorTracker;
using Com.O2Bionics.Utils.JsonSettings;

namespace Com.O2Bionics.Console
{
    public sealed class ErrorTrackCommand : IndexBaseCommand, ICommand
    {
        private ErrorTrackerSettings m_settings;

        public ErrorTrackCommand() : base("--create-index-error-tracking", "--recreate-index-error-tracking")
        {
        }

        protected override EsConnectionSettings ElasticConnection => m_settings.ElasticConnection;
        protected override List<string> Indices => new List<string> { m_settings.Index.Name };

        protected override void ReadSettings(JsonSettingsReader reader)
        {
            m_settings = reader.ReadFromFile<ErrorTrackerSettings>();
        }

        protected override void CreateIndex()
        {
            ErrorTrackerIndexHelper.CreateIndex(Client, m_settings.Index);
        }
    }
}