using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle( "Noise.BlobStorage" )]
[assembly: AssemblyDescription( "Blob storage support for the Noise Music System" )]
[assembly: AssemblyProduct( "Noise Music System" )]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible( false )]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid( "c38189ce-764d-4bff-ac93-f73fb37f224d" )]

// Allow testing classes to access internal classes.
[assembly:InternalsVisibleTo("Noise.Core.IntegrationTests")]
[assembly:InternalsVisibleTo("Noise.EloqueraDatabase.Tests")]
[assembly:InternalsVisibleTo("Noise.EntityFrameworkDatabase.Tests")]
