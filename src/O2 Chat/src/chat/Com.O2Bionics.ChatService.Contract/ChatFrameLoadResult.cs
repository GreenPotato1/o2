using System.Runtime.Serialization;

namespace Com.O2Bionics.ChatService.Contract
{
    [DataContract]
    public class ChatFrameLoadResult
    {
        [DataMember]
        public CustomerSettingsInfo CustomerSettings { get; set; }

        [DataMember]
        public bool HasActiveSession { get; set; }

        [DataMember]
        public WidgetLoadSatusCode Code { get; set; }

        public override string ToString()
        {
            return $"{nameof(Code)}={Code}, {nameof(HasActiveSession)}={HasActiveSession}, Settings={CustomerSettings}";
        }
    }
}