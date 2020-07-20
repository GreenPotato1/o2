using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Com.O2Bionics.ChatService.Contract
{
    [DataContract]
    public enum ObjectStatus
    {
        [EnumMember] [Display(Name = "Active")]
        Active = 0,

        [EnumMember] [Display(Name = "Disabled")]
        Disabled = 1,

        [EnumMember] [Display(Name = "Deleted")]
        Deleted = 2,

        [EnumMember] [Display(Name = "Not confirmed")]
        NotConfirmed = 3,
    }

    public static class ObjectStatusExtensions
    {
        public static int ToDb(this ObjectStatus v)
        {
            return (int)v;
        }

        public static ObjectStatus FromDb(this int v)
        {
            return (ObjectStatus)v;
        }
    }
}