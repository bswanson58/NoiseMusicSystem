using System;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace ReusableBits.Configuration {
	public interface ISettingsUpgrade {
		XmlElement Upgrade( XmlElement userSettings, Version oldSettingsVersion, Version currentVersion );
	}

	/// <summary>
	/// This class should be derived and have added methods named to facilitate updates from various past versions.
	/// Each appropriate upgrade method from the old version to the current version will be called successively.
	/// 
	/// An example method might be:
	///		private void UpgradeFrom_0_9_0_0( XmlElement userSettings ) {
	///			var savedSearches = userSettings.SelectNodes( "//SavedSearch" );
	///
	///			foreach( XmlElement savedSearch in savedSearches ) {
	///				string xml = savedSearch.InnerXml;
	///				xml = xml.Replace( "IRuleOfGame", "RuleOfGame" );
	///				xml = xml.Replace( "Field>", "FieldName>" );
	///				xml = xml.Replace( "Type>", "Comparison>" );
	///				savedSearch.InnerXml = xml;
	///
	///				if( savedSearch["Name"].InnerText == "Tournament" )
	///					savedSearch.AppendNewElement( "ShowTournamentColumn", "true" );
	///				else
	///					savedSearch.AppendNewElement( "ShowTournamentColumn", "false" );
	///				}
	///			}
	///		}
	/// </summary>
	public abstract class BaseConfigurationUpgrader : ISettingsUpgrade {
		protected	Version	MinimumVersion = new Version( 0, 0, 0, 0 );

		public XmlElement Upgrade( XmlElement userSettings, Version oldSettingsVersion, Version currentVersion ) {
			if( oldSettingsVersion < MinimumVersion )
				throw new Exception( "The minimum required version for upgrade is " + MinimumVersion );

			var upgradeMethods = from method in GetType().GetMethods( BindingFlags.NonPublic )
								 where method.Name.StartsWith( "UpgradeFrom_" )
								 let methodVer = new { Version = new Version( method.Name.Substring( 12 ).Replace( '_', '.' ) ), Method = method }
								 where methodVer.Version >= oldSettingsVersion && methodVer.Version < currentVersion
								 orderby methodVer.Version ascending
								 select methodVer;

			foreach( var methodVer in upgradeMethods ) {
				try {
					methodVer.Method.Invoke( null, new object[] { userSettings } );
				}
				catch( TargetInvocationException ex ) {
					throw new Exception( string.Format( "Failed to upgrade user setting from version {0} to version {1}",
															methodVer.Version, ex.InnerException.Message ), ex.InnerException );
				}
			}

			return userSettings;
		}
	}
}
