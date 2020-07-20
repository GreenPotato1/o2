using System.Threading.Tasks;
using Com.O2Bionics.AuditTrail.Contract;
using JetBrains.Annotations;

namespace Com.O2Bionics.AuditTrail
{
    public interface IAuditTrailService
    {
        /// <summary>
        ///     Save to the Elastic:
        ///     /index-<paramref name="productCode" />/<seealso cref="Com.O2Bionics.Utils.FieldConstants.PreferredTypeName"/> <paramref name="serializedJson" />
        /// </summary>
        Task Save([NotNull] string productCode, [NotNull] string serializedJson);

        /// <summary>
        ///     Select the documents and facets (e.g. author S key and last edited name) from the Elastic.
        ///     The <paramref name="filter" /> must have been validated.
        /// </summary>
        Task<FacetResponse> SelectFacets([NotNull] Filter filter);
    }
}