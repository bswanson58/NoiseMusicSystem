using System.Configuration;

namespace Noise.Infrastructure.Configuration {
	public class ExplorerConfiguration : ConfigurationSection {
		public	const string		SectionName = "explorerConfiguration";

		private const string	cEnableExplorerProperty	= "enableExplorer";

		[ConfigurationPropertyAttribute( cEnableExplorerProperty, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = "false" )]
		public bool EnableExplorer {
			get { return ((bool)( base[cEnableExplorerProperty] ) ); }
			set { base[cEnableExplorerProperty] = value; }
		}
	}
}
