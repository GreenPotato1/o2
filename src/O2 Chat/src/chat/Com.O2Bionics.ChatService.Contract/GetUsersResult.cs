using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Com.O2Bionics.ChatService.Contract
{
    [DataContract]
    public class GetUsersResult
    {
        public GetUsersResult()
        {
        }

        public GetUsersResult(CallResultStatus status)
        {
            Status = status;
        }

        public GetUsersResult(
            List<UserInfo> users,
            List<DepartmentInfo> departments,
            bool areAvatarsAllowed,
            int maxUsers)
        {
            Status = new CallResultStatus(CallResultStatusCode.Success);
            Users = users;
            Departments = departments;
            AreAvatarsAllowed = areAvatarsAllowed;
            MaxUsers = maxUsers;
        }

        [DataMember]
        public CallResultStatus Status { get; set; }

        [DataMember]
        public List<UserInfo> Users { get; set; }

        [DataMember]
        public List<DepartmentInfo> Departments { get; set; }

        [DataMember]
        public bool AreAvatarsAllowed { get; set; }

        [DataMember]
        public int MaxUsers { get; set; }
    }
}