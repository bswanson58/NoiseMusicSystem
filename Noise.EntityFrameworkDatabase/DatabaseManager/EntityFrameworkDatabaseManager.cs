using System;
using Caliburn.Micro;
using Noise.EntityFrameworkDatabase.Interfaces;
using Noise.EntityFrameworkDatabase.Logging;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.DatabaseManager {
	internal class EntityFrameworkDatabaseManager : IDatabaseManager,
													IHandle<Events.LibraryChanged> {
		private const Int16								cDatabaseVersion = 5;

		private readonly IEventAggregator				mEventAggregator;
		private readonly ILogDatabase					mLog;
		private readonly IDatabaseInitializeStrategy	mInitializeStrategy;
		private readonly IDatabaseInfo					mDatabaseInfo;
		private readonly IContextProvider				mContextProvider;
		private readonly ILibraryConfiguration			mLibraryConfiguration;

        public bool                                     IsOpen => mLibraryConfiguration.Current != null;

		public EntityFrameworkDatabaseManager( IEventAggregator eventAggregator, ILogDatabase log, ILibraryConfiguration libraryConfiguration,
											   IDatabaseInitializeStrategy initializeStrategy, IDatabaseInfo databaseInfo, IContextProvider contextProvider ) {
			mEventAggregator = eventAggregator;
			mLog = log;
			mInitializeStrategy = initializeStrategy;
			mDatabaseInfo = databaseInfo;
			mContextProvider = contextProvider;
			mLibraryConfiguration = libraryConfiguration;

			mEventAggregator.Subscribe( this );
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

							mLog.CreatedDatabase( mLibraryConfiguration.Current );
						}
						else {
							mLog.OpenedDatabase( mLibraryConfiguration.Current, mDatabaseInfo.DatabaseVersion );
						}
					}
				}

				mEventAggregator.PublishOnUIThread( new Events.DatabaseOpened());
			}
			catch( Exception ex ) {
				mLog.LogException( $"Database could not be opened { mLibraryConfiguration.Current }", ex );
			}
		}

		private void CloseDatabase() {
			mDatabaseInfo.SetDatabaseClosed();
			mEventAggregator.PublishOnUIThread( new Events.DatabaseClosing());

			mLog.ClosedDatabase();
		}
	}
}
