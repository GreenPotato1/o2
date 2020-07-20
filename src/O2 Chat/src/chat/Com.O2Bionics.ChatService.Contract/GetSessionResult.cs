using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Com.O2Bionics.ChatService.Contract
{
    [DataContract]
    public class GetSessionResult
    {
        [DataMember]
        public CallResultStatus Status { get; set; }

        [DataMember]
        public ChatSessionInfo Session { get; set; }

        [DataMember]
        public GetSessionMessagesResult Messages { get; set; }

        [DataMember]
        public VisitorInfo Visitor { get; set; }

        [DataMember]
        public List<UserInfo> Users { get; set; }

        [DataMember]
        public List<DepartmentInfo> Departments { get; set; }


        public GetSessionResult(CallResultStatus status)
        {
            Status = status;
        }

        public GetSessionResult(
            ChatSessionInfo session,
            GetSessionMessagesResult messages,
            VisitorInfo visitor,
            List<UserInfo> users,
            List<DepartmentInfo> departments)
        {
            Status = new CallResultStatus(CallResultStatusCode.Success);
            Session = session;
            Messages = messages;
            Visitor = visitor;
            Users = users;
            Departments = departments;
        }
    }
}