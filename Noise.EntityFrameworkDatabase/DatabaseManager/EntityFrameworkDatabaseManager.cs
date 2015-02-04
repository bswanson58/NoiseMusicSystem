using System;
using Caliburn.Micro;
using Noise.EntityFrameworkDatabase.Interfaces;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.DatabaseManager {
	public class EntityFrameworkDatabaseManager : IDatabaseManager,
												  IHandle<Events.LibraryChanged> {
		private const Int16		cDatabaseVersion = 4;

		private readonly IEventAggregator				mEventAggregator;
		private readonly IDatabaseInitializeStrategy	mInitializeStrategy;
		private readonly IDatabaseInfo					mDatabaseInfo;
		private readonly IContextProvider				mContextProvider;
		private readonly ILibraryConfiguration			mLibraryConfiguration;

		public EntityFrameworkDatabaseManager( IEventAggregator eventAggregator, ILibraryConfiguration libraryConfiguration,
											   IDatabaseInitializeStrategy initializeStrategy, IDatabaseInfo databaseInfo, IContextProvider contextProvider ) {
			mEventAggregator = eventAggregator;
			mInitializeStrategy = initializeStrategy;
			mDatabaseInfo = databaseInfo;
			mContextProvider = contextProvider;
			mLibraryConfiguration = libraryConfiguration;

			mEventAggregator.Subscribe( this );
		}

		public bool IsOpen {
			get{ return( mLibraryConfiguration.Current != null ); }
		}

		public void Handle( Events.LibraryChanged args ) {
			CloseDatabase();

			if( mLibraryConfiguration.Current != null ) {
				OpenDatabase();
			}
		}

		public bool Initialize() {
			return( true );
		}

		public void Shutdown() {
			CloseDatabase();
		}

		private void OpenDatabase() {
			try { 
				if( mInitializeStrategy != null ) {
					if( mInitializeStrategy.InitializeDatabase( mContextProvider.CreateContext())) {
						if( mInitializeStrategy.DidCreateDatabase ) {
							mDatabaseInfo.InitializeDatabaseVersion( cDatabaseVersion );

							NoiseLogger.Current.LogMessage( "Created Database: '{0}'", mLibraryConfiguration.Current.DatabaseName );
						}
						else {
							NoiseLogger.Current.LogMessage( "Opened Database: '{0}', database version: {1}", 
															mLibraryConfiguration.Current.DatabaseName,	mDatabaseInfo.DatabaseVersion.DatabaseVersion );
						}
					}
				}

				mContextProvider.BlobStorageManager.Initialize( mLibraryConfiguration.Current.BlobDatabasePath );
				if(!mContextProvider.BlobStorageManager.IsOpen ) {
					if(!mContextProvider.BlobStorageManager.OpenStorage()) {
						mContextProvider.BlobStorageManager.CreateStorage();

						if(!mContextProvider.BlobStorageManager.OpenStorage()) {
							var ex = new ApplicationException( "EntityFrameworkDatabaseManager:Blob storage could not be created." );

							NoiseLogger.Current.LogException( "OpenDatabase", ex );
							throw( ex );
						}
					}
				}

				mEventAggregator.Publish( new Events.DatabaseOpened());
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( string.Format( "The database '{0}' could not be opened.", mLibraryConfiguration.Current.DatabaseName ), ex );
			}
		}

		private void CloseDatabase() {
			mEventAggregator.Publish( new Events.DatabaseClosing());

			if( mContextProvider.BlobStorageManager.IsOpen ) {
				mContextProvider.BlobStorageManager.CloseStorage();
			}
		}
	}
}
