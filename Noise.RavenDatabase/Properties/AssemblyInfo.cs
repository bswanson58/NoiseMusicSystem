using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Noise.RavenDatabase")]
[assembly: AssemblyDescription("RavenDB implmentation for the Noise Music System")]
[assembly: AssemblyProduct( "Noise Music System" )]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("fb4430f6-6bf0-4b5d-abef-50137f824eae")]

// Allow testing classes to access internal classes.
[assembly:InternalsVisibleTo("Noise.RavenDatabase.Tests")]
