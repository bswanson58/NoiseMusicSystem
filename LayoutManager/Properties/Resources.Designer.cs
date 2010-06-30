﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Composite.Layout.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Composite.Layout.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No layouts have been defined or loaded.  If your layouts are defined in a configuration file, ensure there are no configuration errors..
        /// </summary>
        internal static string EmptyLayoutsCollectionErrorMessage {
            get {
                return ResourceManager.GetString("EmptyLayoutsCollectionErrorMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to parse fully qualified name: {0}..
        /// </summary>
        internal static string FaultyQualifiedNameErrorMessage {
            get {
                return ResourceManager.GetString("FaultyQualifiedNameErrorMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Type or Filename must be specified for a layout, but not both..
        /// </summary>
        internal static string LayoutConfigurationErrorMessage {
            get {
                return ResourceManager.GetString("LayoutConfigurationErrorMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Layout Control must be of type UserControl..
        /// </summary>
        internal static string LayoutControlNotUserControlErrorMessage {
            get {
                return ResourceManager.GetString("LayoutControlNotUserControlErrorMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The layout file &apos;{0}&apos; could not be found.  Ensure the path and filename is correct..
        /// </summary>
        internal static string LayoutFileNotFoundErrorMessage {
            get {
                return ResourceManager.GetString("LayoutFileNotFoundErrorMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No layout with the name &apos;{0}&apos; exists in the Layouts collection..
        /// </summary>
        internal static string LayoutNotFoundErrorMessage {
            get {
                return ResourceManager.GetString("LayoutNotFoundErrorMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No default layout has been set.  Ensure one layout has the IsDefault property set to true..
        /// </summary>
        internal static string NoDefaultLayoutErrorMessage {
            get {
                return ResourceManager.GetString("NoDefaultLayoutErrorMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to LayoutManager has not been initialized.  Execute the Initialize method before attempting to load a layout..
        /// </summary>
        internal static string NotInitializedErrorMessage {
            get {
                return ResourceManager.GetString("NotInitializedErrorMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to dependencyObject parameter of ClearRegions method must not be null..
        /// </summary>
        internal static string NullDependencyObjectErrorMessage {
            get {
                return ResourceManager.GetString("NullDependencyObjectErrorMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to LayoutManager cannot be null.  Check your configuration settings for errors..
        /// </summary>
        internal static string NullLayoutManagerErrorMessage {
            get {
                return ResourceManager.GetString("NullLayoutManagerErrorMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to LayoutProvider could not be created. .
        /// </summary>
        internal static string NullLayoutProviderErrorMessage {
            get {
                return ResourceManager.GetString("NullLayoutProviderErrorMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to RegionName property must have a value..
        /// </summary>
        internal static string NullRegionNameErrorMessage {
            get {
                return ResourceManager.GetString("NullRegionNameErrorMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ShellName property must be set to load the default layout..
        /// </summary>
        internal static string NullShellNameErrorMessage {
            get {
                return ResourceManager.GetString("NullShellNameErrorMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to view.Type &apos;{0}&apos; is null and could not be loaded or located..
        /// </summary>
        internal static string NullViewTypeErrorMessage {
            get {
                return ResourceManager.GetString("NullViewTypeErrorMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A region with name &apos;{0}&apos; does not exist in layout &apos;{1}&apos;..
        /// </summary>
        internal static string RegionNotFoundInLayoutErrorMessage {
            get {
                return ResourceManager.GetString("RegionNotFoundInLayoutErrorMessage", resourceCulture);
            }
        }
    }
}
