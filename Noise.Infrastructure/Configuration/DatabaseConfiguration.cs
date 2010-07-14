using System.Configuration;

namespace Noise.Infrastructure.Configuration {
	public class DatabaseConfiguration : ConfigurationSection {
		public const string		SectionName = "databaseConfiguration";

		private const string	cDatabaseNameProperty = "databaseName";
		private	const string	cServerNameProperty = "serverName";

		[ConfigurationPropertyAttribute( cDatabaseNameProperty, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = "Noise" )]
		public string DatabaseName {
			get { return ( (string)( base[cDatabaseNameProperty] ) ); }
			set { base[cDatabaseNameProperty] = value; }
		}

		[ConfigurationPropertyAttribute( cServerNameProperty, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = "(local)" )]
		public string ServerName {
			get { return ( (string)( base[cServerNameProperty] ) ); }
			set { base[cServerNameProperty] = value; }
		}
	}
}
