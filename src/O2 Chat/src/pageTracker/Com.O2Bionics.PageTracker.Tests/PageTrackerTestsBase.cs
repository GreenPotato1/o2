using Com.O2Bionics.PageTracker.Tests.Settings;

namespace Com.O2Bionics.PageTracker.Tests
{
    public abstract class PageTrackerTestsBase
    {
        protected virtual TestPageTrackerSettings Settings => new TestPageTrackerSettings();
    }
}