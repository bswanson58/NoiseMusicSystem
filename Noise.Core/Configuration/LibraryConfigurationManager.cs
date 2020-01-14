using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using Noise.Core.Logging;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Configuration {
	public class LibraryConfigurationManager : ILibraryConfiguration, IRequireInitialization {
		private const string						cBackupDateFormat = "yyyy-MM-dd HH-mm-ss";

		private readonly IEventAggregator			mEventAggregator;
		private readonly ILogLibraryConfiguration	mLog;
		private readonly ILogUserStatus				mUserStatus;
		private readonly INoiseEnvironment			mNoiseEnvironment;
		private readonly IPreferences				mPreferences;
		private	readonly List<LibraryConfiguration>	mLibraries;
		private LibraryConfiguration				mCurrentLibrary;

		public LibraryConfigurationManager( ILifecycleManager lifecycleManager, IEventAggregator eventAggregator,
											INoiseEnvironment noiseEnvironment, IPreferences preferences,
											ILogUserStatus userStatus, ILogLibraryConfiguration log ) {
			mEventAggregator = eventAggregator;
			mUserStatus = userStatus;
			mLog = log;
			mNoiseEnvironment = noiseEnvironment;
			mPreferences = preferences;
			mLibraries = new List<LibraryConfiguration>();

			lifecycleManager.RegisterForInitialize( this );
			lifecycleManager.RegisterForShutdown( this );
		}

		public IEnumerable<LibraryConfiguration> Libraries {
			get{ return( mLibraries ); }
		}

		public void Initialize() {
			LoadLibraries();

			mEventAggregator.PublishOnUIThread( new Events.LibraryConfigurationLoaded( this ));
		}

		public void Shutdown() {
			Current = null;
		}

		public LibraryConfiguration	Current {
			get { return( mCurrentLibrary ); }
			private set {
				if( mCurrentLibrary != value ) {
					if( mCurrentLibrary != null ) {
						mLog.LibraryClosed( mCurrentLibrary );
					}

					mCurrentLibrary = value;

					if( mCurrentLibrary != null ) {
						mLog.LibraryOpened( mCurrentLibrary );
						mUserStatus.OpeningLibrary( mCurrentLibrary );
					}
					mEventAggregator.PublishOnUIThread( new Events.LibraryChanged());
				}
			}
		}

		private void LoadLibraries() {
			try {
				mLibraries.Clear();

				foreach( var directory in Directory.EnumerateDirectories( mNoiseEnvironment.LibraryDirectory())) {
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
				mLog.LogException( "Loading library configuration", ex );
			}
		}

		public void OpenDefaultLibrary() {
			var defaultLibrary = ( from library in mLibraries where library.IsDefaultLibrary select library ).FirstOrDefault();

			if( defaultLibrary != null ) {
				Open( defaultLibrary );
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

				var preferences = mPreferences.Load<NoiseCorePreferences>();

				preferences.LastLibraryUsed = Current.LibraryId;
				mPreferences.Save( preferences );
			}
		}

		public async Task AsyncOpen( LibraryConfiguration configuration ) {
			var initTask = new Task( () => Open( configuration  ));

			initTask.Start();

			await initTask;
		} 

		public void Close( LibraryConfiguration configuration ) {
			Current = null;
		}

		public void AddLibrary( LibraryConfiguration configuration ) {
			try {
				if( configuration.IsDefaultLibrary ) {
					ClearDefaultLibrary( configuration );
				}

				var libraryPath = GetLibraryFolder( configuration );

				if( Directory.Exists( libraryPath )) {
					DeleteLibrary( configuration );
				}

				Directory.CreateDirectory( libraryPath );
				configuration.Persist( Path.Combine( libraryPath, Constants.LibraryConfigurationFile ));

				Directory.CreateDirectory( Path.Combine( libraryPath, Constants.LibraryDatabaseDirectory ));
				Directory.CreateDirectory( Path.Combine( libraryPath, Constants.BlobDatabaseDirectory ));
				Directory.CreateDirectory( Path.Combine( libraryPath, Constants.SearchDatabaseDirectory ));

				mLibraries.Add( configuration );
				mEventAggregator.PublishOnUIThread( new Events.LibraryListChanged());
			}
			catch( Exception ex ) {
				mLog.LogException( "Adding library", ex );
			}
		}

		public void UpdateLibrary( LibraryConfiguration configuration ) {
			if( configuration.IsDefaultLibrary ) {
				ClearDefaultLibrary( configuration );
			}

			StoreLibrary( configuration );
		}

		private void StoreLibrary( LibraryConfiguration configuration ) {
			UpdateConfiguration( configuration );

			if( configuration == Current ) {
				mEventAggregator.PublishOnUIThread( new Events.LibraryConfigurationChanged());
			}

			mEventAggregator.PublishOnUIThread( new Events.LibraryListChanged());
		}

		public void UpdateConfiguration( LibraryConfiguration configuration ) {
            try {
                configuration.Persist( Path.Combine( GetLibraryFolder( configuration ), Constants.LibraryConfigurationFile ));
            }
            catch( Exception ex ) {
                mLog.LogException( "Updating library configuration", ex );
            }
        }

		public void DeleteLibrary( LibraryConfiguration configuration ) {
			try {
				var libraryPath = GetLibraryFolder( configuration );

				if( Directory.Exists( libraryPath )) {
					Directory.Delete( libraryPath );
				}

				if( mLibraries.Contains( configuration )) {
					mLibraries.Remove( configuration );
				}

				mEventAggregator.PublishOnUIThread( new Events.LibraryListChanged());
			}
			catch( Exception ex ) {
				mLog.LogException( "Deleting library", ex );
			}
		}

		private void ClearDefaultLibrary( LibraryConfiguration configuration ) {
			foreach( var library in mLibraries ) {
				if(( library.IsDefaultLibrary ) &&
				   ( library != configuration )) {
					library.IsDefaultLibrary = false;

					StoreLibrary( library );
				}
			}
		}

		public string GetLibraryFolder( LibraryConfiguration libraryConfiguration ) {
			var retValue = Path.Combine( mNoiseEnvironment.LibraryDirectory(), libraryConfiguration.LibraryId.ToString( CultureInfo.InvariantCulture ));

			if(!Directory.Exists( retValue )) {
				Directory.CreateDirectory( retValue );
			}

			return( retValue );
		}

		public IEnumerable<LibraryBackup> GetLibraryBackups( LibraryConfiguration forLibrary ) {
			var retValue = new List<LibraryBackup>();
			var backupPath = Path.Combine( mNoiseEnvironment.BackupDirectory(), forLibrary.LibraryId.ToString( CultureInfo.InvariantCulture ));

			if( Directory.Exists( backupPath )) {
				var backupDirectories = Directory.EnumerateDirectories( backupPath );

				foreach( var directory in backupDirectories ) {
					DateTime	backupTime;
					var			directoryName = Path.GetFileName( directory );

					if( DateTime.TryParseExact( directoryName, cBackupDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out backupTime )) {
						retValue.Add( new LibraryBackup( backupTime, directory ));
					}
				}
			}

			return( retValue );
		}

		public LibraryBackup OpenLibraryBackup( LibraryConfiguration libraryConfiguration ) {
			var backupDate = DateTime.Now;
			var backupFolder = backupDate.ToString( cBackupDateFormat );
			var backupPath = Path.Combine( mNoiseEnvironment.BackupDirectory(),
											libraryConfiguration.LibraryId.ToString( CultureInfo.InvariantCulture ),
											backupFolder );
			var	retValue = new LibraryBackup( backupDate, backupPath );

			if(!Directory.Exists( retValue.BackupPath )) {
				Directory.CreateDirectory( retValue.BackupPath );
			}

			return( retValue );
		}

		public void CloseLibraryBackup( LibraryConfiguration libraryConfiguration, LibraryBackup backup ) {
			try {
				libraryConfiguration.Persist( Path.Combine( backup.BackupPath, Constants.LibraryConfigurationFile ));
			}
			catch( Exception exception ) {
				mLog.LogException( string.Format( "Persisting library configuration to \"{0}\"", backup.BackupPath ), exception );
			}

			mEventAggregator.PublishOnUIThread( new Events.LibraryBackupsChanged());
		}

		public void AbortLibraryBackup( LibraryConfiguration libraryConfiguration, LibraryBackup backup ) {
			if( Directory.Exists( backup.BackupPath )) {
				Directory.Delete( backup.BackupPath );
			}
		}

		public void OpenLibraryRestore( LibraryConfiguration libraryConfiguration, LibraryBackup backup ) { }
		public void CloseLibraryRestore( LibraryConfiguration libraryConfiguration, LibraryBackup backup ) { }
		public void AbortLibraryRestore( LibraryConfiguration libraryConfiguration, LibraryBackup backup ) { }

		public LibraryBackup OpenLibraryExport( LibraryConfiguration libraryConfiguration, string exportPath ) {
			var	retValue = new LibraryBackup( DateTime.Now, exportPath );

			return( retValue );
		}

		public void CloseLibraryExport( LibraryConfiguration libraryConfiguration, LibraryBackup backup ) {
			try {
				libraryConfiguration.Persist( Path.Combine( backup.BackupPath, Constants.LibraryConfigurationFile ));
			}
			catch( Exception exception ) {
				mLog.LogException( string.Format( "Persisting library configuration to \"{0}\"", backup.BackupPath ), exception );
			}
		}

		public void AbortLibraryExport( LibraryConfiguration libraryConfiguration, LibraryBackup backup ) {
		}

		public LibraryConfiguration OpenLibraryImport( LibraryConfiguration library, LibraryBackup fromBackup ) {
			var importLibrary = new LibraryConfiguration { LibraryName = library.LibraryName, DatabaseName = library.DatabaseName, 
														   DatabaseServer = library.DatabaseServer,  DatabasePassword = library.DatabasePassword, DatabaseUser = library.DatabaseUser,
														   MediaLocations = library.MediaLocations};

			AddLibrary( importLibrary );

			return( importLibrary );
		}

		public void CloseLibraryImport( LibraryConfiguration library ) {
			mEventAggregator.PublishOnUIThread( new Events.LibraryListChanged());
		}

		public void AbortLibraryImport( LibraryConfiguration library ) { }
	}
}
