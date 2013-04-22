using System;
using System.Reflection;

namespace ReusableBits.Platform {
	public static class VersionInformation {
		public static Version Version {
			get { return Assembly.GetEntryAssembly().GetName().Version; }
		}

		public static string Title {
			get {
				object[] attributes = Assembly.GetEntryAssembly().GetCustomAttributes( typeof( AssemblyTitleAttribute ), false );

				if( attributes.Length > 0 ) {
					var titleAttribute = (AssemblyTitleAttribute)attributes[0];

					if( titleAttribute.Title.Length > 0 ) {
						return titleAttribute.Title;
					}
				}

				return System.IO.Path.GetFileNameWithoutExtension( Assembly.GetExecutingAssembly().CodeBase );
			}
		}

		public static string ProductName {
			get {
				object[] attributes = Assembly.GetEntryAssembly().GetCustomAttributes( typeof( AssemblyProductAttribute ), false );

				return attributes.Length == 0 ? "" : ( (AssemblyProductAttribute)attributes[0] ).Product;
			}
		}

		public static string Description {
			get {
				object[] attributes = Assembly.GetEntryAssembly().GetCustomAttributes( typeof( AssemblyDescriptionAttribute ), false );

				return attributes.Length == 0 ? "" : ( (AssemblyDescriptionAttribute)attributes[0] ).Description;
			}
		}

		public static string CopyrightHolder {
			get {
				object[] attributes = Assembly.GetEntryAssembly().GetCustomAttributes( typeof( AssemblyCopyrightAttribute ), false );

				return attributes.Length == 0 ? "" : ( (AssemblyCopyrightAttribute)attributes[0] ).Copyright;
			}
		}

		public static string CompanyName {
			get {
				object[] attributes = Assembly.GetEntryAssembly().GetCustomAttributes( typeof( AssemblyCompanyAttribute ), false );

				return attributes.Length == 0 ? "" : ( (AssemblyCompanyAttribute)attributes[0] ).Company;
			}
		}
	}
}
