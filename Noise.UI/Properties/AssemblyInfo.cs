using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle( "Noise.UI" )]
[assembly: AssemblyDescription( "User Interface components used by the Noise Music System" )]
[assembly: AssemblyProduct( "Noise Music System" )]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible( false )]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid( "e7b86cb1-1400-4d84-983d-6148257c0033" )]

// Allow testing classes to access internal classes.
[assembly:InternalsVisibleTo("Noise.UI.Tests")]
[assembly:InternalsVisibleTo("Noise.UI.Blendable")]
