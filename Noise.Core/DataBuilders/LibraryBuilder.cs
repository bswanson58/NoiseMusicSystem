using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Unity;
using Noise.Core.Database;
using Noise.Core.FileStore;
using Noise.Core.Platform;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.DataBuilders {
	public class LibraryBuilder : ILibraryBuilder {
		private readonly IUnityContainer		mContainer;
		private readonly IEventAggregator		mEvents;
		private readonly FileSystemWatcherEx	mFolderWatcher;
		private IFolderExplorer					mFolderExplorer;
		private IMetaDataCleaner				mMetaDataCleaner;
		private IMetaDataExplorer				mMetaDataExplorer;
		private	ISummaryBuilder					mSummaryBuilder;
		private bool							mContinueExploring;

		public	bool							LibraryUpdateInProgress { get; private set; }
		public	bool							LibraryUpdatePaused { get; private set; }

		public LibraryBuilder( IUnityContainer container ) {
			mContainer = container;
			mEvents = mContainer.Resolve<IEventAggregator>();
			mFolderWatcher = new FileSystemWatcherEx();
		}

		public bool EnableUpdateOnLibraryChange {
			get{ return( mFolderWatcher.IsWatching ); }
			set {
				if( value ) {
					var folder = RootFolderList().FirstOrDefault();

					if( folder != null ) {
						mFolderWatcher.Initialize( folder, OnLibraryChanged );
					}
				}
				else {
					mFolderWatcher.Shutdown();
				}
			}
		}

		private void OnLibraryChanged( string rootFolder ) {
			if(!LibraryUpdateInProgress ) {
				StartLibraryUpdate();
			}
		}

		public IEnumerable<string> RootFolderList() {
			var retValue = new List<string>();
			var folderExplorer = mContainer.Resolve<IFolderExplorer>();
			var databaseManager = mContainer.Resolve<IDatabaseManager>();
			var database = databaseManager.ReserveDatabase();

			if( database != null ) {
				try {
					retValue.AddRange( folderExplorer.RootFolderList().Select( rootFolder => StorageHelpers.GetPath( database.Database, rootFolder )));
				}
				catch( Exception ex ) {
					NoiseLogger.Current.LogException( "Exception - RootFolderList:", ex );
				}
				finally {
					databaseManager.FreeDatabase( database );
				}
			}

			return( retValue );
		}

		public void StartLibraryUpdate() {
			NoiseLogger.Current.LogMessage( "LibraryBuilder: Starting Library Update." );

			ThreadPool.QueueUserWorkItem( UpdateLibrary );
		}

		public void PauseLibraryUpdate() {
			if(( LibraryUpdateInProgress ) &&
			   (!LibraryUpdatePaused )) {
				LibraryUpdatePaused = true;
			}
		}

		public void ResumeLibraryUpdate() {
			if(( LibraryUpdateInProgress ) &&
			   ( LibraryUpdatePaused )) {
				LibraryUpdatePaused = false;
			}
		}

		public void StopLibraryUpdate() {
			mContinueExploring = false;
			ResumeLibraryUpdate();

			if( mFolderExplorer != null ) {
				mFolderExplorer.Stop();
			}

			if( mMetaDataCleaner != null ) {
				mMetaDataCleaner.Stop();
			}

			if( mMetaDataExplorer != null ) {
				mMetaDataExplorer.Stop();
			}

			if( mSummaryBuilder != null ) {
				mSummaryBuilder.Stop();
			}

			WaitForExplorer();
		}

		private void WaitForExplorer() {
			int    timeOutSeconds = 10 * 2;

			while(( LibraryUpdateInProgress ) &&
				  ( timeOutSeconds > 0 )) {
				Thread.Sleep( TimeSpan.FromMilliseconds( 500 ));
				timeOutSeconds--;
			}
		}

		private void UpdateLibrary( object state ) {
			mContinueExploring = true;

			LibraryUpdateInProgress = true;

			try {
				mEvents.GetEvent<Events.LibraryUpdateStarted>().Publish( 0L );

				using( new SleepPreventer()) {
					if( mContinueExploring ) {
						mFolderExplorer = mContainer.Resolve<IFolderExplorer>();
						mFolderExplorer.SynchronizeDatabaseFolders();
					}

					var results = new DatabaseChangeSummary();
					if( mContinueExploring ) {
						mMetaDataCleaner = mContainer.Resolve<IMetaDataCleaner>();
						mMetaDataCleaner.CleanDatabase( results );
					}

					if( mContinueExploring ) {
						mMetaDataExplorer = mContainer.Resolve<IMetaDataExplorer>();
						mMetaDataExplorer.BuildMetaData( results );
					}

					if( mContinueExploring ) {
						mSummaryBuilder = mContainer.Resolve<ISummaryBuilder>();
						mSummaryBuilder.BuildSummaryData( results );
					}

					NoiseLogger.Current.LogMessage( "LibraryBuilder: Update Finished." );

					if( results.HaveChanges ) {
						NoiseLogger.Current.LogInfo( string.Format( "Database changes: {0}", results ) );
					}

					if( mContinueExploring ) {
						LogStatistics( true );
					}
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Exception - LibraryBuilderUpdate: ", ex );
			}
			finally {
				mFolderExplorer = null;
				mMetaDataExplorer = null;
				mSummaryBuilder = null;

				LibraryUpdateInProgress = false;
			}

			mEvents.GetEvent<Events.LibraryUpdateCompleted>().Publish( 0L );
		}

		public void LogLibraryStatistics() {
			ThreadPool.QueueUserWorkItem( LogStatistics );
		}

		private void LogStatistics( object state ) {
			LogStatistics( false );
		}

		private void LogStatistics( bool allCounts ) {
			var	statistics = mContainer.Resolve<DatabaseStatistics>();

			statistics.GatherStatistics( allCounts );
			NoiseLogger.Current.LogInfo( statistics.ToString());
		}
	}
}
