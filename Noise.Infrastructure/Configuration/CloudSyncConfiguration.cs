using System.Configuration;

namespace Noise.Infrastructure.Configuration {
	public class CloudSyncConfiguration : ConfigurationSection {
		public const string		SectionName = "cloudSyncConfiguration";

		private const string	cLoginNameProperty			= "loginName";
		private const string	cLoginPasswordProperty		= "password";
		private const string	cUseCloudProperty			= "useCloud";
		private const string	cLastSequenceProperty		= "lastSequence";

		[ConfigurationPropertyAttribute( cUseCloudProperty, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = false )]
		public bool UseCloud {
			get { return ((bool)( base[cUseCloudProperty] )); }
			set { base[cUseCloudProperty] = value; }
		}

		[ConfigurationPropertyAttribute( cLoginNameProperty, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = "" )]
		public string LoginName {
			get { return ((string)( base[cLoginNameProperty] )); }
			set { base[cLoginNameProperty] = value; }
		}

		[ConfigurationPropertyAttribute( cLoginPasswordProperty, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = "" )]
		public string LoginPassword {
			get { return ((string)( base[cLoginPasswordProperty] )); }
			set { base[cLoginPasswordProperty] = value; }
		}

		[ConfigurationPropertyAttribute( cLastSequenceProperty, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = "0" )]
		public long LastSequence {
			get { return ((long)( base[cLastSequenceProperty] )); }
			set { base[cLastSequenceProperty] = value; }
		}
	}
}
