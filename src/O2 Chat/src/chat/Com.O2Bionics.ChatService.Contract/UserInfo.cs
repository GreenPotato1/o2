using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Jil;

namespace Com.O2Bionics.ChatService.Contract
{
    [DataContract]
    public class UserInfo : IHasCompositeFullName
    {
        //If you change this class, make changes to the "SpecificClassDiff" class.

        [DataMember]
        public uint Id { get; set; }

        [DataMember]
        public uint CustomerId { get; set; }

        [DataMember]
        public DateTime AddTimestampUtc { get; set; }

        [DataMember]
        public DateTime UpdateTimestampUtc { get; set; }

        [DataMember]
        [JilDirective(TreatEnumerationAs = typeof(int))]
        public ObjectStatus Status { get; set; }

        [DataMember]
        public string FirstName { get; set; }

        [DataMember]
        public string LastName { get; set; }

        [DataMember]
        public string Email { get; set; }

        [DataMember]
        public bool IsOwner { get; set; }

        [DataMember]
        public bool IsAdmin { get; set; }

        [DataMember]
        public HashSet<uint> AgentDepartments { get; set; }

        [DataMember]
        public HashSet<uint> SupervisorDepartments { get; set; }

        [DataMember]
        public string Avatar { get; set; }
    }
}