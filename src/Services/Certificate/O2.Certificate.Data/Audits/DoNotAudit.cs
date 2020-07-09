using System;

namespace O2.Certificate.Data.Audits
{
    [AttributeUsage(AttributeTargets.Property)] 
    public class DoNotAudit : Attribute 
    { 
    }
}