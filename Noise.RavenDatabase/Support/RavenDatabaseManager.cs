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
using Raven.Client;
using Raven.Client.Embedded;

namespace Noise.RavenDatabase.Support {
	public class RavenDatabaseManager : IDatabaseManager, IDbFactory,
										IHandle<Events.LibraryChanged> {
		private const Int16						cDatabaseVersionMajor = 0;
		private const Int16						cDatabaseVersionMinor = 9;

		private readonly IEventAggregator		mEventAggregator;
		private readonly ILibraryConfiguration	mLibraryConfiguration;
		private readonly IBlobStorageManager	mBlobStorageManager;
		private IDocumentStore					mLibraryDatabase;
		private bool							mBlobStorageInitialized;

		private readonly Subject<bool>			mDatabaseClosedSubject;
		public	IObservable<bool>				DatabaseClosed { get { return( mDatabaseClosedSubject.AsObservable()); } }

		public bool IsOpen { get; private set; }

		public RavenDatabaseManager( IEventAggregator eventAggregator, ILibraryConfiguration libraryConfiguration,
									 IBlobStorageManager blobStorageManager, IBlobStorageResolver storageResolver ) {
			mEventAggregator = eventAggregator;
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

				if( databaseCreated ) {
					var versionProvider = new DatabaseInfoProvider( this );

					versionProvider.InitializeDatabaseVersion( cDatabaseVersionMajor, cDatabaseVersionMinor );

					NoiseLogger.Current.LogMessage( "Created Library Database: {0}", mLibraryConfiguration.Current.LibraryName );
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

						NoiseLogger.Current.LogException( ex );
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
		}
	}
}
