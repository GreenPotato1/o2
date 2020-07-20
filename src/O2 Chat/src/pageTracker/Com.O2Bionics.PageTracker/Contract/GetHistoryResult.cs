using System.Collections.Generic;
using Com.O2Bionics.Utils;
using JetBrains.Annotations;

namespace Com.O2Bionics.PageTracker.Contract
{
    public sealed class GetHistoryResult
    {
        [CanBeNull]
        public PageHistoryVisitorInfo Visitor { get; set; }

        [CanBeNull]
        public List<PageHistoryRecord> Items { get; set; }

        public bool HasMore { get; set; }

        [CanBeNull]
        public SearchPositionInfo SearchPosition { get; set; }
    }
}