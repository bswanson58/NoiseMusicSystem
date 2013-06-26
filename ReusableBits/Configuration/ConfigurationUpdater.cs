﻿using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;

// from: http://stackoverflow.com/questions/1599945/how-do-you-upgrade-settings-settings-when-the-stored-data-type-changes

namespace ReusableBits.Configuration {
	internal static class ExtensionMethods {
		public static XmlNode AppendNewElement( this XmlNode element, string name ) {
			return AppendNewElement( element, name, null );
		}

		public static XmlNode AppendNewElement( this XmlNode element, string name, string value ) {
			var doc = element.OwnerDocument ?? (XmlDocument)element;
			var addedElement = doc.CreateElement( name );

			if( value != null ) {
				addedElement.InnerText = value;
			}

			element.AppendChild( addedElement );

			return addedElement;
		}

		public static XmlNode AppendNewAttribute( this XmlNode element, string name, string value ) {
			if(( element != null ) &&
			   ( element.Attributes != null ) &&
			   ( element.OwnerDocument != null )) {
				var attr = element.OwnerDocument.CreateAttribute( name );

				attr.Value = value;
				element.Attributes.Append( attr );
			}

			return element;
		}
	}

	public static class ConfigurationUpdater {
		public static void UpdateConfiguration( Assembly forAssembly ) {
			UpdateConfiguration( forAssembly, null );
		}

		public static void UpdateConfiguration( Assembly forAssembly, ISettingsUpgrade upgrader ) {
			var assemblyName = forAssembly.GetName();

			UpdateConfiguration( assemblyName.Name, assemblyName.Version, upgrader );			
		}

		public static void UpdateConfiguration( string applicationName, Version currentVersion, ISettingsUpgrade upgrader ) {
			try {
				// This works for both ClickOnce and non-ClickOnce applications, whereas
				// ApplicationDeployment.CurrentDeployment.DataDirectory only works for ClickOnce applications
				var currentSettingsDir = new FileInfo( ConfigurationManager.OpenExeConfiguration( ConfigurationUserLevel.PerUserRoamingAndLocal ).FilePath ).Directory;

				if( currentSettingsDir == null )
					throw new Exception( "Unable to determine the location of the user configuration file." );

				if( !currentSettingsDir.Exists ) {
					currentSettingsDir.Create();
				}

				if( currentSettingsDir.Parent != null ) {
					var previousSettings = ( from dir in currentSettingsDir.Parent.GetDirectories()
											 let dirVer = new { Dir = dir, Ver = new Version( dir.Name ) }
											 where dirVer.Ver < currentVersion
											 orderby dirVer.Ver descending
											 select dirVer ).FirstOrDefault();

					if( previousSettings != null ) {
						var userSettings = ReadUserSettings( previousSettings.Dir.GetFiles( "user.config" ).Single().FullName, applicationName );

						if( upgrader != null ) {
							userSettings = upgrader.Upgrade( userSettings, previousSettings.Ver, currentVersion );
						}

						WriteUserSettings( userSettings, currentSettingsDir.FullName + @"\user.config", applicationName, false );
					}
				}
			}
			catch( Exception ex ) {
				throw new Exception( "An error occurred while upgrading the user configuration settings from the previous version", ex );
			}
		}

		private static XmlElement ReadUserSettings( string configFile, string applicationName ) {
			// PreserveWhitespace required for unencrypted files due to https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=352591
			var doc = new XmlDocument { PreserveWhitespace = true };

			doc.Load( configFile );

			var settingsNode = doc.SelectSingleNode( string.Format( "configuration/userSettings/{0}.Properties.Settings", applicationName ));

			if( settingsNode != null ) {
				var encryptedDataNode = settingsNode["EncryptedData"];

				if( encryptedDataNode != null ) {
					var provider = new RsaProtectedConfigurationProvider();

					provider.Initialize( "userSettings", new NameValueCollection());

					return (XmlElement)provider.Decrypt( encryptedDataNode );
				}
			}
			
			return (XmlElement)settingsNode;
		}

		private static void WriteUserSettings( XmlElement settingsNode, string configFile, string applicationName, bool encrypt ) {
			XmlDocument	doc;
			XmlNode		appSettings = default( XmlNode );

			if( encrypt ) {
				var provider = new RsaProtectedConfigurationProvider();

				provider.Initialize( "userSettings", new NameValueCollection());

				var encryptedSettings = provider.Encrypt( settingsNode );

				doc = encryptedSettings.OwnerDocument;
				if( doc != null ) {
					appSettings = doc.CreateElement( string.Format( "{0}.Properties.Settings", applicationName ) ).AppendNewAttribute( "configProtectionProvider", provider.GetType().Name );
					appSettings.AppendChild( encryptedSettings );
				}
			}
			else {
				doc = settingsNode.OwnerDocument;
				appSettings = settingsNode;
			}

			if(( doc != null ) &&
			   ( appSettings != null )) {
				doc.RemoveAll();
				doc.AppendNewElement( "configuration" )
					.AppendNewElement( "userSettings" )
					.AppendChild( appSettings );

				using( var writer = new XmlTextWriter( configFile, Encoding.UTF8 ) { Formatting = Formatting.Indented, Indentation = 4 } ) {
					doc.Save( writer );
				}
			}
		}
	}
}
