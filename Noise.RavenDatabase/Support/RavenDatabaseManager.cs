using System;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Caliburn.Micro;
using Noise.BlobStorage.BlobStore;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;
using Noise.RavenDatabase.DataProviders;
using Noise.RavenDatabase.Interfaces;
using Noise.RavenDatabase.Logging;
using Raven.Client;
using Raven.Client.Embedded;
using Raven.Client.Indexes;

namespace Noise.RavenDatabase.Support {
	internal class RavenDatabaseManager : IDatabaseManager, IDbFactory,
										  IHandle<Events.LibraryChanged> {
		private const Int16						cDatabaseVersion = 1;

		private readonly IEventAggregator		mEventAggregator;
		private readonly ILogRaven				mLog;
		private readonly ILibraryConfiguration	mLibraryConfiguration;
		private readonly IBlobStorageManager	mBlobStorageManager;
		private IDocumentStore					mLibraryDatabase;
		private bool							mBlobStorageInitialized;

		private readonly Subject<bool>			mDatabaseClosedSubject;
		public	IObservable<bool>				DatabaseClosed { get { return( mDatabaseClosedSubject.AsObservable()); } }

		public bool IsOpen { get; private set; }

		public RavenDatabaseManager( IEventAggregator eventAggregator, ILibraryConfiguration libraryConfiguration,
									 IBlobStorageManager blobStorageManager, IBlobStorageResolver storageResolver, ILogRaven log ) {
			mEventAggregator = eventAggregator;
			mLog = log;
			mLibraryConfiguration = libraryConfiguration;
			mBlobStorageManager = blobStorageManager;
			mBlobStorageManager.SetResolver( storageResolver );

			mDatabaseClosedSubject = new Subject<bool>();

			mEventAggregator.Subscribe( this );
		}

		public bool Initialize() {
			return( true );
		}

		public void Handle( Events.LibraryChanged args ) {
			CloseDatabases();

			if( mLibraryConfiguration.Current != null ) {
				// Complete the initial database creation, set the IsOpen flag.
				GetLibraryDatabase();

				mEventAggregator.Publish( new Events.DatabaseOpened());
			}
		}

		public void Shutdown() {
			CloseDatabases();
		}

		public IDocumentStore GetLibraryDatabase() {
			if(( mLibraryDatabase == null ) &&
			   ( mLibraryConfiguration.Current != null )) {
				var	databaseCreated = !Directory.Exists( mLibraryConfiguration.Current.LibraryDatabasePath );

				mLibraryDatabase = InitializeDatabase( mLibraryConfiguration.Current.LibraryDatabasePath );

				IndexCreation.CreateIndexes( GetType().Assembly, mLibraryDatabase );

				if( databaseCreated ) {
					var versionProvider = new DatabaseInfoProvider( this, mLog );

					versionProvider.InitializeDatabaseVersion( cDatabaseVersion );

					mLog.CreatedDatabase( mLibraryConfiguration.Current );
				}
				else {
					mLog.OpenedDatabase( mLibraryConfiguration.Current );
				}

				IsOpen = mLibraryDatabase != null;
			}

			return( mLibraryDatabase );
		}

		public IBlobStorage GetBlobStorage() {
			if( !mBlobStorageInitialized ) {
				InitBlobStorage();

				mBlobStorageInitialized = true;
			}

			return( mBlobStorageManager.GetStorage());
		}

		private IDocumentStore InitializeDatabase( string libraryPath ) {
			IDocumentStore	retValue = new EmbeddableDocumentStore { DataDirectory = libraryPath };

			retValue.Initialize();

			return( retValue );
		}

		private void InitBlobStorage() {
			mBlobStorageManager.Initialize( mLibraryConfiguration.Current.BlobDatabasePath );

			if(!mBlobStorageManager.IsOpen ) {
				if(!mBlobStorageManager.OpenStorage()) {
					mBlobStorageManager.CreateStorage();

					if(!mBlobStorageManager.OpenStorage()) {
						var ex = new ApplicationException( "RavenDatabaseManager:Blob storage could not be created." );

						mLog.LogException( "Initializing BlobStorage", ex );
						throw ( ex );
					}
				}
			}
		}

		private void CloseDatabases() {
			IsOpen = false;
			mDatabaseClosedSubject.OnNext( true );

			if( mLibraryDatabase != null ) {
				mLibraryDatabase.Dispose();

				mLibraryDatabase = null;
			}

			mBlobStorageManager.CloseStorage();
			mBlobStorageInitialized = false;

			mLog.ClosedDatabase();
		}
	}
}
