using System;
using System.Collections.Generic;
using Com.O2Bionics.AuditTrail.Contract;
using Com.O2Bionics.ChatService.Contract.AuditTrail;
using Com.O2Bionics.Tests.Common;
using JetBrains.Annotations;
using NUnit.Framework;

namespace Com.O2Bionics.AuditTrail.Tests.Utils
{
    public static class FacetHelper
    {
        public static void CheckFacets([NotNull] this FacetResponse response, string operation, bool shouldExist = true)
        {
            if (string.IsNullOrEmpty(operation))
                throw new ArgumentNullException(nameof(operation));

            CheckFacet(response.Operations, shouldExist, nameof(response.Operations), operation);
            CheckFacet(response.Statuses, shouldExist, nameof(response.Statuses), OperationStatus.SuccessKey);
            CheckFacet(response.Authors, shouldExist, nameof(response.Authors), TestConstants.FakeUserId.ToString(), TestConstants.FakeUserName);
        }

        private static void CheckFacet(
            [CanBeNull] List<Facet> facets,
            bool shouldExist,
            string propertyName,
            string facetId,
            string facetName = null)
        {
            Assert.IsNotNull(facets, propertyName);
            Assert.AreEqual(1, facets.Count, propertyName + ".Count");
            var facet = facets[0];
            Assert.IsNotNull(facet, propertyName + ".Facet");

            Assert.AreEqual(shouldExist ? 1 : 0, facet.Count, propertyName + ".Count");
            Assert.AreEqual(facetId, facet.Id, propertyName + ".facetId");
            Assert.AreEqual(facetName, facet.Name, propertyName + ".facetName");
        }
    }
}