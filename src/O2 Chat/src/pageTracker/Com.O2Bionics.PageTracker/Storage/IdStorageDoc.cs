using JetBrains.Annotations;
using Nest;

namespace Com.O2Bionics.PageTracker.Storage
{
    [UsedImplicitly]
    [ElasticsearchType(IdProperty = nameof(id))]
    public sealed class IdStorageDoc
    {
        // ReSharper disable InconsistentNaming
        [Number(NumberType.Integer, Store = false, DocValues = false)]
        public int id { get; [UsedImplicitly] set; }

        [Number(NumberType.Long, Index = false)]
        public long last { get; [UsedImplicitly] set; }
        // ReSharper restore InconsistentNaming

        public override string ToString()
        {
            return $"{id}:{last}";
        }
    }
}