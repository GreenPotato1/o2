using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Com.O2Bionics.ChatService.Contract
{
    [DataContract]
    public class GetDepartmentsResult
    {
        public GetDepartmentsResult(CallResultStatus status)
        {
            Status = status;
        }

        public GetDepartmentsResult(List<DepartmentInfo> departments, int maxDepartments)
        {
            Status = new CallResultStatus(CallResultStatusCode.Success);
            Departments = departments;
            MaxDepartments = maxDepartments;
        }

        [DataMember]
        public CallResultStatus Status { get; set; }

        [DataMember]
        public List<DepartmentInfo> Departments { get; set; }

        [DataMember]
        public int MaxDepartments { get; set; }
    }
}