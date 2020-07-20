using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using Com.O2Bionics.AuditTrail.Client;
using Com.O2Bionics.AuditTrail.Contract;
using Com.O2Bionics.ChatService.Contract.AuditTrail;
using Com.O2Bionics.ChatService.Web.Console.Properties;
using Com.O2Bionics.Utils;
using JetBrains.Annotations;
using Filter = Com.O2Bionics.AuditTrail.Contract.Filter;

namespace Com.O2Bionics.ChatService.Web.Console.Controllers
{
    [Authorize(Roles = RoleNames.Admin)]
    public sealed class AuditTrailController : ManagementControllerBase
    {
        [HttpPost]
        public async Task<ActionResult> AuditTrailEvents(Filter filter)
        {
            var result = await FetchFacets(false, filter);
            return result;
        }

        [HttpPost]
        public async Task<ActionResult> LoginEvents(Filter filter)
        {
            var result = await FetchFacets(true, filter);
            return result;
        }

        private bool CheckFilter(long customerId, [CanBeNull] Filter filter)
        {
            string error;
            if (null == filter)
            {
                error = "Empty filter";
            }
            else
            {
                filter.ProductCode = ProductCodes.Chat;
                filter.CustomerId = customerId.ToString();
                filter.SetDates(true);
                error = filter.Validate(false);
            }

            if (string.IsNullOrEmpty(error))
                return true;

            Write((int)HttpStatusCode.BadRequest, error);
            return false;
        }
        
        private static void FixOperations([NotNull] List<string> operationSet, Filter filter)
        {
            Debug.Assert(0 < operationSet.Count, "0 < operationSet.Count");
            if (null == filter.Operations || 0 == filter.Operations.Count)
            {
                filter.Operations = operationSet;
                return;
            }

            for (var i = 0; i < filter.Operations.Count;)
            {
                var op = filter.Operations[i];
                var pos = operationSet.BinarySearch(op);
                if (0 <= pos)
                {
                    ++i;
                    continue;
                }

                //Bad operation.
                filter.Operations.SwapLastAndRemove(i);
#if DEBUG
                throw new Exception($"Unwanted operation '{op}' in {nameof(AuditTrailController)}.");
#endif
            }

            if (0 == filter.Operations.Count)
            {
                filter.Operations = operationSet;
            }
        }

        private async Task<ActionResult> FetchFacets(bool isLogin, Filter filter)
        {
            var customerId = CustomerId;
            if (!CheckFilter(customerId, filter))
                return null;
            if (!await CheckDays(isLogin, filter, customerId))
                return null;

            var isRequired = IsWholeFilterRequired(filter);
            var operationSet = isLogin ? OperationKindGroups.Logins : OperationKindGroups.AuditTrails;
            FixOperations(operationSet, filter);

            var client = GlobalContainer.Resolve<IAuditTrailClient>();
            var response = await (isRequired ? SelectMergeFacets(operationSet, filter, client) : client.SelectFacets(filter));
            return JilJson(response);
        }
        
        private async Task<bool> CheckDays(bool isLogin, [NotNull] Filter filter, uint customerId)
        {
            var days = await FeatureServiceHelper.FetchVisibleDays(isLogin, customerId);
            if (days <= 0)
            {
                var error = isLogin ? Resources.LoginEventPlanError : Resources.AuditEventPlanError;
                Write((int)HttpStatusCode.BadRequest, error);
                return false;
            }

            var nowProvider = GlobalContainer.Resolve<INowProvider>();
            var now = nowProvider.UtcNow;
            var minTime = now.AddDays(-days);
            if (filter.ToTime <= minTime)
            {
                ForceEmptyDocs(filter);
            }
            else
            {
                if (filter.FromTime < minTime)
                    filter.FromTime = minTime;
            }

            return true;
        }

        private static void ForceEmptyDocs(Filter filter)
        {
            var zeroDate = new DateTime(2000, 1, 1);
            filter.FromTime = zeroDate;
            filter.ToTime = zeroDate.AddHours(1);
        }

        private static bool IsWholeFilterRequired([NotNull] Filter filter)
        {
            var result =
                0 == filter.FromRow &&
                (!string.IsNullOrEmpty(filter.Substring)
                 || filter.FromTime != filter.ToTime
                 || null != filter.Operations && 0 < filter.Operations.Count
                 || null != filter.Statuses && 0 < filter.Statuses.Count
                 || null != filter.SearchPosition
                 || null != filter.AuthorIds && 0 < filter.AuthorIds.Count);
            return result;
        }

        private static async Task<FacetResponse> SelectMergeFacets(
            [NotNull] List<string> operationSet,
            [NotNull] Filter filter,
            [NotNull] IAuditTrailClient client)
        {
            var wholeFilter = new Filter(filter.ProductCode) { CustomerId = filter.CustomerId, Operations = operationSet };
            Debug.Assert(
                !IsWholeFilterRequired(wholeFilter),
                $"Debug. Inner error. The whole filter '{wholeFilter}' is not whole. Source filter: '{filter}'.");
            Debug.Assert(
                null == wholeFilter.Validate(false),
                $"Debug. Inner error. Filter '{filter}' created an invalid whole filter '{wholeFilter}', error: '{wholeFilter.Validate(false)}'.");

            var tasks = new[] { client.SelectFacets(filter), client.SelectFacets(wholeFilter) };
            await Task.WhenAll(tasks);

            FacetResponse result = tasks[0].Result, wholeResult = tasks[1].Result;
            if (null == wholeResult)
                return result;
            if (null == result)
            {
                FixWholeResponse(wholeResult);
                return wholeResult;
            }

            MergeFacets(wholeResult.Operations, ref result.Operations);
            MergeFacets(wholeResult.Statuses, ref result.Statuses);
            MergeFacets(wholeResult.Authors, ref result.Authors);
            return result;
        }

        private static void FixWholeResponse([NotNull] FacetResponse response)
        {
            response.RawDocuments = null;

            var lists = response.GetFacets();
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var index = 0; index < lists.Length; index++)
            {
                var facets = lists[index].Value;
                if (null != facets)
                    SetZeroCounts(facets);
            }
        }

        private static void SetZeroCounts([NotNull] List<Facet> facets)
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < facets.Count; i++)
                facets[i].Count = 0;
        }

        private static void MergeFacets([CanBeNull] List<Facet> source, [CanBeNull] ref List<Facet> target)
        {
            if (null == source || 0 == source.Count)
                return;

            if (null == target || 0 == target.Count)
            {
                SetZeroCounts(source);
                target = source;
                return;
            }

            var set = target.Select(f => f.Id).ToHashSet();
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < source.Count; i++)
            {
                var facet = source[i];
                if (set.Add(facet.Id))
                {
                    facet.Count = 0;
                    target.Add(facet);
                }
            }
        }
    }
}