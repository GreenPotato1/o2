using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using JetBrains.Annotations;
using NameCollection = System.Collections.Generic.KeyValuePair<string, System.Collections.Generic.List<Com.O2Bionics.AuditTrail.Contract.Facet>>;

namespace Com.O2Bionics.AuditTrail.Contract
{
    [DataContract]
    public sealed class FacetResponse
    {
        [DataMember(Name = "RawDocuments")] public List<string> RawDocuments;

        [IgnoreDataMember]
        public long Count
        {
            get
            {
                var lists = GetFacets();
                long result = RawDocuments?.Count ?? 0;

                // ReSharper disable once LoopCanBeConvertedToQuery
                // ReSharper disable once ForCanBeConvertedToForeach
                for (var i = 0; i < lists.Length; i++)
                    if (null != lists[i].Value)
                        result += lists[i].Value.Count;

                return result;
            }
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            var lists = GetFacets();

            const char equalSeparator = '=';

            if (null != RawDocuments && 0 < RawDocuments.Count)
                builder.Append(nameof(RawDocuments)).Append(equalSeparator).Append(RawDocuments.Count);

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < lists.Length; i++)
            {
                if (0 < builder.Length)
                    builder.Append(", ");
                builder.Append(lists[i].Key).Append(equalSeparator).Append(lists[i].Value?.Count ?? 0);
            }

            return builder.ToString();
        }

        [NotNull]
        public NameCollection[] GetFacets()
        {
            var lists = new[]
                {
                    new NameCollection(nameof(Operations), Operations),
                    new NameCollection(nameof(Statuses), Statuses),
                    new NameCollection(nameof(Authors), Authors)
                };
            return lists;
        }

        #region Facets

        [DataMember(Name = "Operations")] public List<Facet> Operations;

        [DataMember(Name = "Statuses")] public List<Facet> Statuses;

        /// <summary>
        ///     See <seealso cref="Author" />.
        /// </summary>
        [DataMember(Name = "Authors")] public List<Facet> Authors;

        #endregion
    }
}