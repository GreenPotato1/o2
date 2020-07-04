using System;

namespace O2.Business.Data.Audits
{
    [AttributeUsage(AttributeTargets.Property)] 
    public class DoNotAudit : Attribute 
    { 
    }
}