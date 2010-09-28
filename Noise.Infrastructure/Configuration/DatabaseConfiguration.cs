using System.Configuration;

namespace Noise.Infrastructure.Configuration {
	public class DatabaseConfiguration : ConfigurationSection {
		public const string		SectionName = "databaseConfiguration";

		private const string	cDatabaseNameProperty = "databaseName";
		private	const string	cServerNameProperty = "serverName";
		private const string	cDatabaseUserNameProperty = "databaseUserName";
		private const string	cDatabasePasswordProperty = "databasePassword";
		private const string	cSearchIndexProperty = "searchIndexLocation";

		[ConfigurationPropertyAttribute( cDatabaseNameProperty, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = "Noise" )]
		public string DatabaseName {
			get { return ((string)( base[cDatabaseNameProperty] )); }
			set { base[cDatabaseNameProperty] = value; }
		}

		[ConfigurationPropertyAttribute( cServerNameProperty, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = "localhost" )]
		public string ServerName {
			get { return ((string)( base[cServerNameProperty] )); }
			set { base[cServerNameProperty] = value; }
		}

		[ConfigurationPropertyAttribute( cDatabaseUserNameProperty, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = "" )]
		public string UserName {
			get { return ((string)( base[cDatabaseUserNameProperty] )); }
			set { base[cDatabaseUserNameProperty] = value; }
		}

		[ConfigurationPropertyAttribute( cDatabasePasswordProperty, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = "" )]
		public string UserPassword {
			get { return ((string)( base[cDatabasePasswordProperty] )); }
			set { base[cDatabasePasswordProperty] = value; }
		}

		[ConfigurationPropertyAttribute( cSearchIndexProperty, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = "" )]
		public string SearchIndexLocation {
			get { return ((string)( base[cSearchIndexProperty] )); }
			set { base[cSearchIndexProperty] = value; }
		}
	}
}
