using System.Reflection;
using System.Runtime.InteropServices;
using BundlerUi;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle( "Noise.Installation.BundlerUi" )]
[assembly: AssemblyDescription( "The Noise Music System installer application" )]
[assembly: AssemblyProduct( "Noise Music System" )]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("843a3675-14a2-4525-a092-ea81c747171d")]

[assembly: BootstrapperApplication( typeof( BootstrapperApp ))]