using System.Runtime.Serialization;

namespace Com.O2Bionics.ChatService.Contract
{
    [DataContract]
    public class SaveChatWidgetAppearanceJsonResult
    {
        public SaveChatWidgetAppearanceJsonResult(CallResultStatus callResultStatus)
        {
            Status = callResultStatus;
        }

        [DataMember]
        public CallResultStatus Status { get; set; }
    }
}