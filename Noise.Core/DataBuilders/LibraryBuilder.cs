using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Noise.Core.Database;
using Noise.Core.FileStore;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.DataBuilders {
	internal class LibraryBuilder : ILibraryBuilder {
		private readonly IUnityContainer	mContainer;
		private readonly IEventAggregator	mEvents;
		private IFolderExplorer				mFolderExplorer;
		private IMetaDataExplorer			mMetaDataExplorer;
		private ISearchBuilder				mSearchBuilder;
		private	ISummaryBuilder				mSummaryBuilder;
		private bool						mContinueExploring;
		private readonly ILog				mLog;

		public	bool						LibraryUpdateInProgress { get; private set; }
		public	bool						LibraryUpdatePaused { get; private set; }

		public LibraryBuilder( IUnityContainer container ) {
			mContainer = container;
			mEvents = mContainer.Resolve<IEventAggregator>();
			mLog = mContainer.Resolve<ILog>();
		}

		public IEnumerable<string> RootFolderList() {
			var retValue = new List<string>();
			var folderExplorer = mContainer.Resolve<IFolderExplorer>();
			var databaseManager = mContainer.Resolve<IDatabaseManager>();
			var database = databaseManager.ReserveDatabase();

			if( database != null ) {
				try {
					foreach( var rootFolder in folderExplorer.RootFolderList()) {
						retValue.Add( StorageHelpers.GetPath( database.Database, rootFolder ));
					}
				}
				catch( Exception ex ) {
					mLog.LogException( "Exception - RootFolderList:", ex );
				}
				finally {
					databaseManager.FreeDatabase( database );
				}
			}

			return( retValue );
		}

		public void StartLibraryUpdate() {
			mLog.LogMessage( "LibraryBuilder: Starting Library Update." );

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

			if( mMetaDataExplorer != null ) {
				mMetaDataExplorer.Stop();
			}

			if( mSummaryBuilder != null ) {
				mSummaryBuilder.Stop();
			}

			if( mSearchBuilder != null ) {
				mSearchBuilder.Stop();
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

				if( mContinueExploring ) {
					mFolderExplorer = mContainer.Resolve<IFolderExplorer>();
					mFolderExplorer.SynchronizeDatabaseFolders();
				}

				var results = new DatabaseChangeSummary();
				if( mContinueExploring ) {
					mMetaDataExplorer = mContainer.Resolve<IMetaDataExplorer>();
					mMetaDataExplorer.BuildMetaData( results );
				}

				if(( mContinueExploring ) &&
				   ( results.HaveChanges )) {
					mSummaryBuilder = mContainer.Resolve<ISummaryBuilder>();
					mSummaryBuilder.BuildSummaryData( results.ChangedArtists );
				}

				if(( mContinueExploring ) &&
				   ( results.HaveChanges )) {
					mSearchBuilder = mContainer.Resolve<ISearchBuilder>();
					mSearchBuilder.BuildSearchIndex( results.ChangedArtists );
				}

				mLog.LogMessage( "LibraryBuilder: Update Finished." );

				if( results.HaveChanges ) {
					mLog.LogInfo( string.Format( "Database changes: {0}", results ) );
				}

				if( mContinueExploring ) {
					LogStatistics();
				}
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - LibraryBuilderUpdate: ", ex );
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
			LogStatistics();
		}

		private void LogStatistics() {
			var	statistics = mContainer.Resolve<DatabaseStatistics>();

			statistics.GatherStatistics();
			mLog.LogInfo( statistics.ToString());
		}
	}
}
