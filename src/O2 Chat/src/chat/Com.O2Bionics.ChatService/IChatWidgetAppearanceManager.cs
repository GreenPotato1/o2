using System.Collections.Generic;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.Contract.WidgetAppearance;
using JetBrains.Annotations;

namespace Com.O2Bionics.ChatService
{
    public interface IChatWidgetAppearanceManager
    {
        void Save(uint customerId, ChatWidgetAppearance widgetAppearance);

        [NotNull]
        ChatWidgetAppearanceInfo Get(uint customerId);

        HashSet<string> GetEnabledFeatures(uint customerId);
    }
}