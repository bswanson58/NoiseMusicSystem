using System.Configuration;

namespace Noise.Infrastructure.Configuration {
	public class ServerConfiguration : ConfigurationSection {
		public const string		SectionName = "serverConfiguration";

		private const string	cUseServerProperty	= "useServer";
		private	const string	cServerNameProperty = "serverName";

		[ConfigurationPropertyAttribute( cUseServerProperty, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = false )]
		public bool UseServer {
			get { return ((bool)( base[cUseServerProperty] )); }
			set { base[cUseServerProperty] = value; }
		}

		[ConfigurationPropertyAttribute( cServerNameProperty, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = "localhost" )]
		public string ServerName {
			get { return ((string)( base[cServerNameProperty] )); }
			set { base[cServerNameProperty] = value; }
		}
	}
}
