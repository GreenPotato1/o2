using System.Runtime.Serialization;

namespace Com.O2Bionics.ChatService.Contract
{
    [DataContract]
    public enum MediaSupport
    {
        [EnumMember] NotSupported = 0,
        [EnumMember] Audio = 1,
        [EnumMember] Video = 2,
    }

    [DataContract]
    public class MediaSupportInfo
    {
        [DataMember]
        public bool HasWebcam { get; set; }

        [DataMember]
        public bool HasMicrophone { get; set; }

        [DataMember]
        public bool IsWebRtcSupported { get; set; }
    }
}