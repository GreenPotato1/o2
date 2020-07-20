using System;
using System.Runtime.Serialization;
using Com.O2Bionics.Utils;

namespace Com.O2Bionics.ChatService.Contract.Widget
{
    [DataContract]
    public sealed class WidgetLoadRequest
    {
        [DataMember]
        public DateTime BeginDate { get; set; }

        [DataMember]
        public DateTime EndDate { get; set; }

        [DataMember]
        public string BeginDateStr { get; set; }

        [DataMember]
        public string EndDateStr { get; set; }

        public void SetDates(bool clearAfterSet = false)
        {
            if (!string.IsNullOrEmpty(BeginDateStr))
                BeginDate = DateUtilities.ParseDate(BeginDateStr);

            if (!string.IsNullOrEmpty(EndDateStr))
                EndDate = DateUtilities.ParseDate(EndDateStr).AddDays(1);

            if (clearAfterSet)
                BeginDateStr = EndDateStr = null;
        }

        public void SetStrings()
        {
            BeginDateStr = BeginDate.DateToString();
            EndDateStr = EndDate.DateToString();
        }

        public override string ToString()
        {
            return $"{BeginDate} - {EndDate}";
        }
    }
}