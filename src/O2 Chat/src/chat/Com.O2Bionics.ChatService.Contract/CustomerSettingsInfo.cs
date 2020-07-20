using System.Runtime.Serialization;

namespace Com.O2Bionics.ChatService.Contract
{
    [DataContract]
    public class CustomerSettingsInfo
    {
        [DataMember]
        public bool IsEnabled { get; set; }

        [DataMember]
        public bool IsVisitorCaptchaRequired { get; set; }

        [DataMember]
        public bool IsProactiveChatEnabled { get; set; }

        [DataMember]
        public ChatWidgetAppearanceInfo ChatWidgetAppearanceInfo { get; set; }

        public override string ToString()
        {
            return
                $"Enabled={IsEnabled}, CaptchaRequired={IsVisitorCaptchaRequired}, ProactiveChat={IsProactiveChatEnabled}, {nameof(ChatWidgetAppearanceInfo)}={ChatWidgetAppearanceInfo}";
        }
    }
}