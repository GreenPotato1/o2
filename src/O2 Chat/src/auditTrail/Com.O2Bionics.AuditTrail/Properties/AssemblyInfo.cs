using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle(
#if DEBUG
    "Com.O2Bionics.AuditTrail(Debug)"
#else
    "Com.O2Bionics.AuditTrail(Release)"
#endif
)]
[assembly: AssemblyDescription("Audit trail implementation")]
[assembly: AssemblyProduct("Com.O2Bionics.AuditTrail")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("5bd1b58f-2ded-4050-8c0e-4982939cf861")]
[assembly: InternalsVisibleTo("Com.O2Bionics.AuditTrail.Tests")]
