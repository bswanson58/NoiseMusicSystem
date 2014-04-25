using System;
using Caliburn.Micro;
using Noise.EntityFrameworkDatabase.Interfaces;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.DatabaseManager {
	public class EntityFrameworkDatabaseManager : IDatabaseManager,
												  IHandle<Events.LibraryChanged> {
		private const Int16		cDatabaseVersion = 1;

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
			if( mInitializeStrategy != null ) {
				if(( mInitializeStrategy.InitializeDatabase( mContextProvider.CreateContext()) &&
					( mInitializeStrategy.DidCreateDatabase ))) {
					mDatabaseInfo.InitializeDatabaseVersion( cDatabaseVersion );
				}
			}

			mContextProvider.BlobStorageManager.Initialize( mLibraryConfiguration.Current.BlobDatabasePath );
			if(!mContextProvider.BlobStorageManager.IsOpen ) {
				if(!mContextProvider.BlobStorageManager.OpenStorage()) {
					mContextProvider.BlobStorageManager.CreateStorage();

					if(!mContextProvider.BlobStorageManager.OpenStorage()) {
						var ex = new ApplicationException( "EntityFrameworkDatabaseManager:Blob storage could not be created." );

						NoiseLogger.Current.LogException( ex );
						throw( ex );
					}
				}
			}

			mEventAggregator.Publish( new Events.DatabaseOpened());
		}

		private void CloseDatabase() {
			mEventAggregator.Publish( new Events.DatabaseClosing());

			if( mContextProvider.BlobStorageManager.IsOpen ) {
				mContextProvider.BlobStorageManager.CloseStorage();
			}
		}
	}
}
