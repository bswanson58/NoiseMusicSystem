using System;
using System.Configuration;

namespace Noise.Infrastructure.Configuration {
	public class StorageConfiguration : ConfigurationSection {
		public	const string	SectionName = "storageConfiguration";

		private const string	cRootFoldersProperty = "rootFolders";

		public override bool IsReadOnly() {
			return( false );
		}

		[ConfigurationProperty( cRootFoldersProperty, IsRequired = false, IsKey = false, IsDefaultCollection = false )]
		public ConfigurationElementCollection<RootFolderConfiguration> RootFolders {
			get { return((ConfigurationElementCollection<RootFolderConfiguration>)( base[cRootFoldersProperty])); }
			set { base[cRootFoldersProperty] = value; }
		}
	}

	public class RootFolderConfiguration : ConfigurationElement {
		internal const string cKeyPropertyName = "key";
		internal const string cPathPropertyName = "path";

		public RootFolderConfiguration() {
			Key = Guid.NewGuid().ToString();
		}

		public override bool IsReadOnly() {
			return( false );
		}

		[ConfigurationPropertyAttribute( cKeyPropertyName, IsRequired = true, IsKey = true, IsDefaultCollection = false )]
		public string Key {
			get { return((string)( base[cKeyPropertyName])); }
			set { base[cKeyPropertyName] = value; }
		}

		[ConfigurationPropertyAttribute( cPathPropertyName, IsRequired = true, IsKey = false, IsDefaultCollection = false )]
		public string Path {
			get { return((string)( base[cPathPropertyName])); }
			set { base[cPathPropertyName] = value; }
		}

		public override string ToString() {
			return( Key );
		}
	}

	public class FolderStrategyConfiguration : ConfigurationElement {
		
	}
}
