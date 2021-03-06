﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Caliburn.Micro;
using Noise.Core.Database;
using Noise.Core.FileProcessor;
using Noise.Core.FileStore;
using Noise.Core.Logging;
using Noise.Core.Platform;
using Noise.Core.Sidecars;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.DataBuilders {
	internal class LibraryBuilder : ILibraryBuilder {
		private readonly IEventAggregator			mEventAggregator;
		private readonly ILogLibraryBuilding		mLog;
		private readonly ILogUserStatus				mUserStatus;
		private readonly FileSystemWatcherEx		mFolderWatcher;
		private readonly IStorageFolderSupport		mStorageFolderSupport;
		private readonly IFolderExplorer			mFolderExplorer;
		private readonly IMetaDataCleaner			mMetaDataCleaner;
		private readonly IStorageFileProcessor		mStorageFileProcessor;
		private readonly ISidecarBuilder			mSidecarBuilder;
		private	readonly ISummaryBuilder			mSummaryBuilder;
		private readonly DatabaseStatistics			mDatabaseStatistics;
		private bool								mContinueExploring;

		public	bool								LibraryUpdateInProgress { get; private set; }
		public	bool								LibraryUpdatePaused { get; private set; }
        public  IDatabaseStatistics                 LibraryStatistics => mDatabaseStatistics;

		public LibraryBuilder( IEventAggregator eventAggregator, ILogUserStatus userStatus, ILogLibraryBuilding log,
							   IStorageFolderSupport storageFolderSupport, IFolderExplorer folderExplorer, IMetaDataCleaner metaDataCleaner,
							   IStorageFileProcessor storageFileProcessor, ISidecarBuilder sidecarBuilder, ISummaryBuilder summaryBuilder,
							   DatabaseStatistics databaseStatistics ) {
			mEventAggregator = eventAggregator;
			mLog = log;
			mUserStatus = userStatus;
			mStorageFolderSupport = storageFolderSupport;
			mFolderExplorer = folderExplorer;
			mMetaDataCleaner = metaDataCleaner;
			mStorageFileProcessor = storageFileProcessor;
			mSidecarBuilder = sidecarBuilder;
			mSummaryBuilder = summaryBuilder;
			mDatabaseStatistics = databaseStatistics;
			mFolderWatcher = new FileSystemWatcherEx();
		}

		public bool EnableUpdateOnLibraryChange {
			get => ( mFolderWatcher.IsWatching );
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
				retValue.AddRange( mFolderExplorer.RootFolderList().Select( rootFolder => mStorageFolderSupport.GetPath( rootFolder )));
			}
			catch( Exception ex ) {
				mLog.LogException( "Building root folder paths", ex );
			}

			return( retValue );
		}

		public void StartLibraryUpdate() {
			mLog.BuildingStarted();
			mUserStatus.StartingLibraryUpdate();

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

		    mFolderExplorer?.Stop();
		    mMetaDataCleaner?.Stop();
		    mStorageFileProcessor?.Stop();
		    mSummaryBuilder?.Stop();
		    mSidecarBuilder?.Stop();

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
			var results = new DatabaseChangeSummary();

			mContinueExploring = true;
			LibraryUpdateInProgress = true;

			try {
				mEventAggregator.PublishOnUIThread( new Events.LibraryUpdateStarted( 0L ));

				using( new SleepPreventer()) {
					if( mContinueExploring ) {
						mFolderExplorer.SynchronizeDatabaseFolders();
					}

					if( mContinueExploring ) {
						mMetaDataCleaner.CleanDatabase( results );
					}

					if( mContinueExploring ) {
						mStorageFileProcessor.Process( results );
					}

					if( mContinueExploring ) {
						mSummaryBuilder.BuildSummaryData( results );
					}

					if( mContinueExploring ) {
						mSidecarBuilder.Process();
					}

					mLog.BuildingCompleted( results );
					mUserStatus.CompletedLibraryUpdate();

					if( results.HaveChanges ) {
						mUserStatus.LibraryChanged( results );
					}

					if( mContinueExploring ) {
						LogStatistics( true );
					}
				}
			}
			catch( Exception ex ) {
				mLog.LogException( "Building libraries", ex );
			}
			finally {
				LibraryUpdateInProgress = false;
			}

			mEventAggregator.PublishOnUIThread( new Events.LibraryUpdateCompleted( results ));
            mEventAggregator.PublishOnCurrentThread( new Events.LibraryBackupPressure( 1, "UpdateLibrary" ));
		}

		public void LogLibraryStatistics() {
			ThreadPool.QueueUserWorkItem( LogStatistics );
		}

		private void LogStatistics( object state ) {
			LogStatistics( false );
		}

		private void LogStatistics( bool allCounts ) {
			mDatabaseStatistics.GatherStatistics( allCounts );

			mLog.DatabaseStatistics( mDatabaseStatistics );
			mEventAggregator.PublishOnUIThread( new Events.DatabaseStatisticsUpdated( mDatabaseStatistics ));
			mUserStatus.LibraryStatistics( mDatabaseStatistics );
		}
	}
}
