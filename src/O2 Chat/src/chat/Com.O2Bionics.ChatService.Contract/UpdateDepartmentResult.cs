using System.Runtime.Serialization;

namespace Com.O2Bionics.ChatService.Contract
{
    [DataContract]
    public class UpdateDepartmentResult
    {
        public UpdateDepartmentResult(CallResultStatus status, DepartmentInfo dept = null)
        {
            Status = status;
            Department = dept;
        }

        [DataMember]
        public CallResultStatus Status { get; set; }

        [DataMember]
        public DepartmentInfo Department { get; set; }
    }
}