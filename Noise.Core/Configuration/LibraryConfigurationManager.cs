using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Caliburn.Micro;
using Noise.Core.Support;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Configuration {
	public class LibraryConfigurationManager : ILibraryConfiguration, IRequireInitialization {
		private readonly IEventAggregator			mEventAggregator;
		private	readonly List<LibraryConfiguration>	mLibraries;
		private string								mConfigurationDirectory;

		public	LibraryConfiguration				Current { get; private set; }

		public LibraryConfigurationManager( ILifecycleManager lifecycleManager, IEventAggregator eventAggregator ) {
			mEventAggregator = eventAggregator;
			mLibraries = new List<LibraryConfiguration>();

//			lifecycleManager.RegisterForInitialize( this );
		}

		public IEnumerable<LibraryConfiguration> Libraries {
			get{ return( mLibraries ); }
		}

		public void Initialize() {
			mConfigurationDirectory = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ),
													Constants.CompanyName, 
													Constants.LibraryConfigurationDirectory );
			LoadLibraries();

			if(!mLibraries.Any()) {
				var defaultLibrary = new LibraryConfiguration { LibraryName = "Noise", DatabaseName = "Noise" };

				AddLibrary( defaultLibrary );
			}
		}

		public void Shutdown() {
		}

		private void LoadLibraries() {
			try {
				mLibraries.Clear();

				if(!Directory.Exists( mConfigurationDirectory )) {
					Directory.CreateDirectory( mConfigurationDirectory );
				}

				foreach( var directory in Directory.EnumerateDirectories( mConfigurationDirectory )) {
					var configDirectory = new DirectoryInfo( directory );
					var configFile = configDirectory.GetFiles( Constants.LibraryConfigurationFile ).FirstOrDefault();

					if( configFile != null ) {
						var libraryConfig = LibraryConfiguration.LoadConfiguration( configFile.FullName );

						if( libraryConfig != null ) {
							mLibraries.Add( libraryConfig );
						}
					}
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Loading library configuration:", ex );
			}
		}

		public void Open( long libraryId ) {
			var configuration = mLibraries.FirstOrDefault( c => c.LibraryId == libraryId );

			if( configuration != null ) {
				Open( configuration );
			}
		}

		public void Open( LibraryConfiguration configuration ) {
			Close( Current );

			if(( configuration != null ) &&
			   ( mLibraries.Contains( configuration ))) {
				Current = configuration;

				var expConfig = NoiseSystemConfiguration.Current.RetrieveConfiguration<ExplorerConfiguration>( ExplorerConfiguration.SectionName );
				if( expConfig != null ) {
					expConfig.LastLibraryUsed = Current.LibraryId;

					NoiseSystemConfiguration.Current.Save( expConfig );
				}
				
			}
		}

		public void Close( LibraryConfiguration configuration ) {
			Current = null;
		}

		public void AddLibrary( LibraryConfiguration configuration ) {
			try {
				var libraryPath = Path.Combine( mConfigurationDirectory, configuration.LibraryId.ToString( CultureInfo.InvariantCulture ));

				if( Directory.Exists( libraryPath )) {
					DeleteLibrary( configuration );
				}

				Directory.CreateDirectory( libraryPath );
				configuration.Persist( Path.Combine( libraryPath, Constants.LibraryConfigurationFile ));

				Directory.CreateDirectory( Path.Combine( libraryPath, Constants.BlobDatabaseDirectory ));
				Directory.CreateDirectory( Path.Combine( libraryPath, Constants.SearchDatabaseDirectory ));

				mLibraries.Add( configuration );
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "ConfigurationManager:AddLibrary", ex );
			}
		}

		public void UpdateLibrary( LibraryConfiguration configuration ) {
			try {
				var libraryPath = Path.Combine( mConfigurationDirectory, configuration.LibraryId.ToString( CultureInfo.InvariantCulture ));
				
				configuration.Persist( Path.Combine( libraryPath, Constants.LibraryConfigurationFile ));

				if( configuration == Current ) {
					mEventAggregator.Publish( new Events.LibraryConfigurationChanged());
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "ConfigurationManager:UpdateLibrary", ex );
			}
		}

		public void DeleteLibrary( LibraryConfiguration configuration ) {
			try {
				var libraryPath = Path.Combine( mConfigurationDirectory, configuration.LibraryId.ToString( CultureInfo.InvariantCulture ));

				if( Directory.Exists( libraryPath )) {
					Directory.Delete( libraryPath );
				}

				if( mLibraries.Contains( configuration )) {
					mLibraries.Remove( configuration );
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "ConfigurationManager:DeleteLibrary", ex );
			}
		}
	}
}
