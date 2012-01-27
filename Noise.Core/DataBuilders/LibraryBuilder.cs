using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Caliburn.Micro;
using Noise.Core.Database;
using Noise.Core.FileStore;
using Noise.Core.Platform;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.DataBuilders {
	public class LibraryBuilder : ILibraryBuilder {
		private readonly ICaliburnEventAggregator	mEventAggregator;
		private readonly FileSystemWatcherEx	mFolderWatcher;
		private readonly IStorageFolderProvider	mStorageFolderProvider;
		private readonly IFolderExplorer		mFolderExplorer;
		private readonly IMetaDataCleaner		mMetaDataCleaner;
		private readonly IMetaDataExplorer		mMetaDataExplorer;
		private	readonly ISummaryBuilder		mSummaryBuilder;
		private readonly DatabaseStatistics		mDatabaseStatistics;
		private bool							mContinueExploring;

		public	bool							LibraryUpdateInProgress { get; private set; }
		public	bool							LibraryUpdatePaused { get; private set; }

		public LibraryBuilder( ICaliburnEventAggregator eventAggregator, IStorageFolderProvider storageFolderProvider,
							   IFolderExplorer folderExplorer, IMetaDataCleaner metaDataCleaner, IMetaDataExplorer metaDataExplorer,
							   ISummaryBuilder summaryBuilder, DatabaseStatistics databaseStatistics ) {
			mEventAggregator = eventAggregator;
			mStorageFolderProvider = storageFolderProvider;
			mFolderExplorer = folderExplorer;
			mMetaDataCleaner = metaDataCleaner;
			mMetaDataExplorer = metaDataExplorer;
			mSummaryBuilder = summaryBuilder;
			mDatabaseStatistics = databaseStatistics;
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

			try {
				retValue.AddRange( mFolderExplorer.RootFolderList().Select( rootFolder => mStorageFolderProvider.GetPhysicalFolderPath( rootFolder )));
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Exception - RootFolderList:", ex );
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
				mEventAggregator.Publish( new Events.LibraryUpdateStarted( 0L ));

				using( new SleepPreventer()) {
					if( mContinueExploring ) {
						mFolderExplorer.SynchronizeDatabaseFolders();
					}

					var results = new DatabaseChangeSummary();
					if( mContinueExploring ) {
						mMetaDataCleaner.CleanDatabase( results );
					}

					if( mContinueExploring ) {
						mMetaDataExplorer.BuildMetaData( results );
					}

					if( mContinueExploring ) {
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
				LibraryUpdateInProgress = false;
			}

			mEventAggregator.Publish( new Events.LibraryUpdateCompleted( 0L ));
		}

		public void LogLibraryStatistics() {
			ThreadPool.QueueUserWorkItem( LogStatistics );
		}

		private void LogStatistics( object state ) {
			LogStatistics( false );
		}

		private void LogStatistics( bool allCounts ) {
			mDatabaseStatistics.GatherStatistics( allCounts );
			NoiseLogger.Current.LogInfo( mDatabaseStatistics.ToString());
		}
	}
}
