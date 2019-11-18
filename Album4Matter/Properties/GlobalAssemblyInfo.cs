using System.Reflection;

//
// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
//
[assembly: AssemblyCompany("Secret Squirrel Software")]
[assembly: AssemblyCopyright("Copyright © 2010-2018 Secret Squirrel Software")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Make it easy to distinguish Debug and Release (i.e. Retail) builds;
// for example, through the file properties window.
#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Retail")]
#endif


//
// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
//	!!! Also update the version number in the installer Version.wxi !!!

// Assembly Version is only used by .Net Framework to indentify the assembly
// for locating, linking and loading the assemblies.
[assembly: AssemblyVersion( "0.1.0.0" )]

// The AssemblyFileVersionAttribute is incremented with every build in order
// to distinguish one build from another. It is displayed as File version in
// the Explorer property dialog.
[assembly: AssemblyFileVersion( "0.1.0.0" )]

// Displayed as Product version in the Explorer property dialog.
// By default, the "Product version" shown in the file properties window is
// the same as the value specified for AssemblyFileVersionAttribute.
// Set AssemblyInformationalVersionAttribute to be the same as
// AssemblyVersionAttribute so that the "Product version" in the file
// properties window matches the version displayed in the GAC shell extension.
[assembly: AssemblyInformationalVersion("0.1.0.0")]
