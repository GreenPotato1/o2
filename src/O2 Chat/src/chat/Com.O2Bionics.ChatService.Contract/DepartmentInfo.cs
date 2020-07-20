using System.Runtime.Serialization;
using Jil;

namespace Com.O2Bionics.ChatService.Contract
{
    [DataContract]
    public class DepartmentInfo
    {
        //If you change this class, make changes to the "SpecificClassDiff" class.

        [DataMember]
        public uint Id { get; set; }

        [DataMember]
        public uint CustomerId { get; set; }

        [DataMember]
        [JilDirective(TreatEnumerationAs = typeof(int))]
        public ObjectStatus Status { get; set; }

        [DataMember]
        public bool IsPublic { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Description { get; set; }
    }
}