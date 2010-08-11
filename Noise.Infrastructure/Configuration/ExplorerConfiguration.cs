using System.Configuration;

namespace Noise.Infrastructure.Configuration {
	public class ExplorerConfiguration : ConfigurationSection {
		public	const string		SectionName = "explorerConfiguration";

		private const string	cEnableLibraryExplorerProperty				= "enableLibraryExplorer";
		private const string	cEnableBackgroundContentExplorerProperty	= "enableBackgroundContentExplorer";

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
	}
}
