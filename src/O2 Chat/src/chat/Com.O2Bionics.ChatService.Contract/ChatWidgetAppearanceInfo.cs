using System.Collections.Generic;
using System.Runtime.Serialization;
using Com.O2Bionics.ChatService.Contract.WidgetAppearance;
using Com.O2Bionics.Utils;

namespace Com.O2Bionics.ChatService.Contract
{
    [DataContract]
    public class ChatWidgetAppearanceInfo
    {
        [DataMember]
        public ChatWidgetAppearance AppearanceData { get; set; }

        [DataMember]
        public HashSet<string> EnabledFeatures { get; set; }

        [DataMember]
        public string Domains { get; set; }

        public override string ToString()
        {
            return
                $"{nameof(EnabledFeatures)}={EnabledFeatures.JoinAsString()}, {nameof(AppearanceData)}={AppearanceData}, {nameof(Domains)}={Domains}";
        }
    }
}