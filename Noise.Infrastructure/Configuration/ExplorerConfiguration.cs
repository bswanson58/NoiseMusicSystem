using System.Configuration;

namespace Noise.Infrastructure.Configuration {
	public class ExplorerConfiguration : ConfigurationSection {
		public	const string		SectionName = "explorerConfiguration";

		private const string	cEnableLibraryExplorerProperty				= "enableLibraryExplorer";
		private const string	cEnableBackgroundContentExplorerProperty	= "enableBackgroundContentExplorer";
		private const string	cMinimizeToTrayProperty						= "minimizeToTray";
		private const string	cDisplayPlayTimeElapsedProperty				= "displayPlayTimeElapsed";

		[ConfigurationPropertyAttribute( cEnableLibraryExplorerProperty, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = "true" )]
		public bool EnableLibraryExplorer {
			get { return ((bool)( base[cEnableLibraryExplorerProperty] ) ); }
			set { base[cEnableLibraryExplorerProperty] = value; }
		}

		[ConfigurationPropertyAttribute( cEnableBackgroundContentExplorerProperty, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = "true" )]
		public bool EnableBackgroundContentExplorer {
			get { return ((bool)( base[cEnableBackgroundContentExplorerProperty] ) ); }
			set { base[cEnableBackgroundContentExplorerProperty] = value; }
		}

		[ConfigurationPropertyAttribute( cMinimizeToTrayProperty, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = "false" )]
		public bool MinimizeToTray {
			get { return ((bool)( base[cMinimizeToTrayProperty] ) ); }
			set { base[cMinimizeToTrayProperty] = value; }
		}

		[ConfigurationPropertyAttribute( cDisplayPlayTimeElapsedProperty, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = "false" )]
		public bool DisplayPlayTimeElapsed {
			get { return ((bool)( base[cDisplayPlayTimeElapsedProperty] ) ); }
			set { base[cDisplayPlayTimeElapsedProperty] = value; }
		}
	}
}
