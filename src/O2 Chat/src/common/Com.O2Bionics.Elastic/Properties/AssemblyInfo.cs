using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle(
#if DEBUG
    "Com.O2Bionics.Elastic(Debug)"
#else
    "Com.O2Bionics.Elastic(Release)"
#endif
)]
[assembly: AssemblyDescription("Elastic search")]
[assembly: AssemblyProduct("Com.O2Bionics.Elastic")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("32182b64-c191-488b-8e4b-11aeaa6a5c0d")]
[assembly: InternalsVisibleTo("Com.O2Bionics.ErrorTracker.Tests")]
[assembly: InternalsVisibleTo("Com.O2Bionics.PageTracker.Tests")]
