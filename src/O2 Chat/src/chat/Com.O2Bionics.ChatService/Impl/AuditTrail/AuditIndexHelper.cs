using Com.O2Bionics.AuditTrail.Contract;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.Contract.Widget;
using Com.O2Bionics.Elastic;
using Com.O2Bionics.Utils;
using Com.O2Bionics.Utils.JsonSettings;
using JetBrains.Annotations;
using AuditEvent_WidgetUnknownDomain = Com.O2Bionics.AuditTrail.Contract.AuditEvent<Com.O2Bionics.ChatService.Contract.Widget.WidgetUnknownDomain>;

namespace Com.O2Bionics.ChatService.Impl.AuditTrail
{
    public static class AuditIndexHelper
    {
        public static void CreateIndex([NotNull] IEsClient client, [NotNull] EsIndexSettings indexSettings)
        {
            client.NotNull(nameof(client));
            indexSettings.NotNull(nameof(indexSettings));
            indexSettings.Name.IsCorrectEsIndexName(nameof(indexSettings.Name));

            client.CreateIndex(
                indexSettings,
                d => d
                    //Document types from AuditEventBuilder:
                    // CustomerInfo, DepartmentInfo, ChatWidgetAppearance, UserInfo
                    // and WidgetDailyViewCountExceededEvent, WidgetUnknownDomain.
                    //
                    .Map<AuditEvent<UserInfo>>( //Map only UserInfo. Others will map themselves on saving.
                        m => m.AutoMap(EsClient.MaxAutoMapRecursion)
                            .Properties(
                                p => p.Text(
                                    //WidgetUnknownDomain.Name must be mapped
                                    //because it is used in WidgetLoadUnknownDomainStorage on the service start.
                                    t => t.Name(nameof(AuditEvent_WidgetUnknownDomain.NewValue) + "." + nameof(WidgetUnknownDomain.Name)).Fields(
                                        ff => ff.Keyword(k => k.Name(FieldConstants.Keyword).IgnoreAbove(FieldConstants.IgnoreAbove)))))
                    )
            );
        }
    }
}