using System;
using System.Configuration;

namespace Noise.Infrastructure.Configuration {
	public class NoiseSystemConfiguration : ISystemConfiguration {
		private static readonly ISystemConfiguration		mDefaultConfiguration = new NoiseSystemConfiguration();
		private static ISystemConfiguration					mCurrent;

		private readonly System.Configuration.Configuration	mConfiguration;

		public NoiseSystemConfiguration() {
			mConfiguration = ConfigurationManager.OpenExeConfiguration( ConfigurationUserLevel.PerUserRoamingAndLocal );
		}

		public static ISystemConfiguration Current {
			get{ return( mCurrent ?? ( mCurrent = mDefaultConfiguration )); }
			set{ mCurrent = value; }
		}

		public T RetrieveConfiguration<T>( string sectionName ) {
			object section = mConfiguration.GetSection( sectionName );

			return((T)section );
		}

		public void Save( ConfigurationSection section ) {
			try {
				mConfiguration.Save( ConfigurationSaveMode.Modified );
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "NoiseSystemConfiguration:Save", ex );
			}
		}
	}
}
