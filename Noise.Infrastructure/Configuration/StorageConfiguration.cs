using System;
using System.Configuration;
using Noise.Infrastructure.Dto;

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
		internal const string cDescriptionName = "description";
		internal const string cPreferStrategyProperty = "preferFolderStrategy";
		internal const string cFolderStrategyProperty = "folderStrategy";

		public RootFolderConfiguration() {
			Key = BitConverter.ToInt64( Guid.NewGuid().ToByteArray(), 0 );
		}

		public override bool IsReadOnly() {
			return( false );
		}

		[ConfigurationProperty( cKeyPropertyName, IsRequired = true, IsKey = true, IsDefaultCollection = false )]
		public long Key {
			get { return((long)( base[cKeyPropertyName])); }
			set { base[cKeyPropertyName] = value; }
		}

		[ConfigurationProperty( cPathPropertyName, IsRequired = true, IsKey = false, IsDefaultCollection = false )]
		public string Path {
			get { return((string)( base[cPathPropertyName])); }
			set { base[cPathPropertyName] = value; }
		}

		[ConfigurationProperty( cDescriptionName, IsRequired = true, IsKey = false, IsDefaultCollection = false )]
		public string Description {
			get { return((string)( base[cDescriptionName])); }
			set { base[cDescriptionName] = value; }
		}

		[ConfigurationProperty( cPreferStrategyProperty, IsRequired = true, IsKey = false, IsDefaultCollection = false )]
		public bool PreferFolderStrategy {
			get { return((bool) base[cPreferStrategyProperty]); }
			set { base[cPreferStrategyProperty] = value; }
		}

		[ConfigurationProperty( cFolderStrategyProperty, IsRequired = false, IsKey = false, IsDefaultCollection = false )]
		public ConfigurationElementCollection<FolderStrategyConfiguration> StorageStrategy {
			get { return((ConfigurationElementCollection<FolderStrategyConfiguration>)( base[cFolderStrategyProperty])); }
			set { base[cFolderStrategyProperty] = value; }
		}

		public override string ToString() {
			return( Key.ToString());
		}
	}

	public class FolderStrategyConfiguration : ConfigurationElement {
		internal const string cLevelProperty = "level";
		internal const string cStrategyProperty = "strategy";

		public FolderStrategyConfiguration() {
		}

		public FolderStrategyConfiguration( int level, eFolderStrategy strategy ) {
			Level = level;
			Strategy = (int)strategy;
		}

		[ConfigurationProperty( cLevelProperty, IsRequired = true, IsKey = true, IsDefaultCollection = false )]
		public int Level {
			get { return((int)( base[cLevelProperty])); }
			set { base[cLevelProperty] = value; }
		}

		[ConfigurationProperty( cStrategyProperty, IsRequired = true, IsKey = true, IsDefaultCollection = false )]
		public int Strategy {
			get { return((int)( base[cStrategyProperty])); }
			set { base[cStrategyProperty] = value; }
		}

		public override string ToString() {
			return( Level.ToString());
		}
	}
}
