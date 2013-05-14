using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;
using Noise.RavenDatabase.Interfaces;
using Raven.Client;
using Raven.Client.Embedded;

namespace Noise.RavenDatabase.Support {
	public class RavenDatabaseManager : IDatabaseManager, IDbFactory,
										IHandle<Events.LibraryChanged> {
		private readonly IEventAggregator		mEventAggregator;
		private readonly ILibraryConfiguration	mLibraryConfiguration;
		private IDocumentStore					mLibraryDatabase;

		public bool IsOpen { get; private set; }

		public RavenDatabaseManager(IEventAggregator eventAggregator, ILibraryConfiguration libraryConfiguration ) {
			mEventAggregator = eventAggregator;
			mLibraryConfiguration = libraryConfiguration;
		}

		public bool Initialize() {
			mEventAggregator.Subscribe( this );

			return( true );
		}

		public void Handle( Events.LibraryChanged args ) {
			CloseDatabase();
		}

		public void Shutdown() {
			CloseDatabase();
		}

		public IDocumentStore GetLibraryDatabase() {
			if(( mLibraryDatabase == null ) &&
			   ( mLibraryConfiguration.Current != null )) {
				mLibraryDatabase = InitializeDatabase( mLibraryConfiguration.Current.LibraryDatabasePath );

				IsOpen = mLibraryDatabase != null;
			}
			return( mLibraryDatabase );
		}

		private IDocumentStore InitializeDatabase( string libraryPath ) {
			IDocumentStore	retValue = new EmbeddableDocumentStore { DataDirectory = libraryPath };

			retValue.Initialize();

			return( retValue );
		}

		private void CloseDatabase() {
			if( mLibraryDatabase != null ) {
				mLibraryDatabase.Dispose();

				mLibraryDatabase = null;
			}

			IsOpen = false;
		}
	}
}
