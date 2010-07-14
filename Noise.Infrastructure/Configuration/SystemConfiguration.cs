using System.Configuration;

namespace Noise.Infrastructure.Configuration {
	public class SystemConfiguration : ISystemConfiguration {
		private readonly System.Configuration.Configuration	mConfiguration;

		public SystemConfiguration() {
			mConfiguration = ConfigurationManager.OpenExeConfiguration( ConfigurationUserLevel.PerUserRoamingAndLocal );

		}
		public T RetrieveConfiguration<T>( string sectionName ) {
			object section = mConfiguration.GetSection( sectionName );

			return((T)section);
		}

		public void Save( ConfigurationSection section ) {
			mConfiguration.Save( ConfigurationSaveMode.Modified );
		}
	}
}
