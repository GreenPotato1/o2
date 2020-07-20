using System.Collections.Generic;
using Com.O2Bionics.PageTracker;
using Com.O2Bionics.PageTracker.Storage;
using Com.O2Bionics.Utils.JsonSettings;

namespace Com.O2Bionics.Console
{
    public sealed class PageTrackCommand : IndexBaseCommand, ICommand
    {
        private PageTrackerSettings m_settings;

        public PageTrackCommand() : base("--create-index-page-tracking", "--recreate-index-page-tracking")
        {
        }

        protected override EsConnectionSettings ElasticConnection => m_settings.ElasticConnection;
        protected override List<string> Indices => PageTrackerIndexHelper.GetIndices(m_settings);

        protected override void ReadSettings(JsonSettingsReader reader)
        {
            m_settings = reader.ReadFromFile<PageTrackerSettings>();
        }

        protected override void CreateIndex()
        {
            PageTrackerIndexHelper.CreateIndices(m_settings);
        }
    }
}