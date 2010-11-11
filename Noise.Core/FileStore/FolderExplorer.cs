using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Practices.Unity;
using Noise.Core.Database;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Recls;

namespace Noise.Core.FileStore {
	public class FolderExplorer : IFolderExplorer {
		private readonly IUnityContainer	mContainer;
		private readonly ILog				mLog;
		private bool						mStopExploring;
		private DatabaseCache<StorageFile>	mFileCache;
		private DatabaseCache<StorageFolder>	mFolderCache;

		public  FolderExplorer( IUnityContainer container ) {
			mContainer = container;
			mLog = mContainer.Resolve<ILog>();
		}

		public IEnumerable<RootFolder> RootFolderList() {
			IEnumerable<RootFolder>		retValue = null;

			var databaseMgr = mContainer.Resolve<IDatabaseManager>();
			var database = databaseMgr.ReserveDatabase();

			if( database != null ) {
				try {
					retValue = from RootFolder root in database.Database where true select root;

					if( retValue.Count() == 0 ) {
						LoadConfiguration( database );

						retValue = from RootFolder root in database.Database where true select root;
					}
				}
				catch( Exception ex ) {
					mLog.LogException( "Exception - RootFolderList: ", ex );
				}
				finally {
					databaseMgr.FreeDatabase( database );
				}
			}

			return( retValue );
		}

		public void SynchronizeDatabaseFolders() {
			mStopExploring = false;

			var rootFolders = RootFolderList();

			if( rootFolders.Count() > 0 ) {
				var databaseMgr = mContainer.Resolve<IDatabaseManager>();
				var database = databaseMgr.ReserveDatabase();

				if( database != null ) {
					try {
						mFileCache = new DatabaseCache<StorageFile>( from StorageFile file in database.Database select file );
						mFolderCache = new DatabaseCache<StorageFolder>( from StorageFolder folder in database.Database select folder );

						foreach( var rootFolder in rootFolders ) {
							if( Directory.Exists( StorageHelpers.GetPath( database.Database, rootFolder ))) {
								mLog.LogInfo( "Synchronizing folder: {0}", rootFolder.DisplayName );
								BuildFolder( database, rootFolder );
							}
							else {
								mLog.LogMessage( "Storage folder does not exists: {0}", rootFolder.DisplayName );
							}

							if( mStopExploring ) {
								break;
							}
						}

						mFileCache.Clear();
						mFolderCache.Clear();
					}
					catch( Exception ex ) {
						mLog.LogException( "Exception - FolderExplorer:", ex );
					}
					finally {
						databaseMgr.FreeDatabase( database );
					}
				}
			}
		}

		public void Stop() {
			mStopExploring = true;
		}

		public void LoadConfiguration( IDatabase database ) {
			var configMgr = mContainer.Resolve<ISystemConfiguration>();
			var storageConfig = configMgr.RetrieveConfiguration<StorageConfiguration>( StorageConfiguration.SectionName  );

			if(( storageConfig != null ) &&
			   ( storageConfig.RootFolders != null )) {
				foreach( RootFolderConfiguration folderConfig in storageConfig.RootFolders ) {
					var root = new RootFolder( folderConfig.Path, folderConfig.Description );

					foreach( FolderStrategyConfiguration strategy in folderConfig.StorageStrategy ) {
						root.FolderStrategy.SetStrategyForLevel( strategy.Level, (eFolderStrategy)strategy.Strategy );
					}

					root.FolderStrategy.PreferFolderStrategy = folderConfig.PreferFolderStrategy;

					database.Insert( root );
				}
			}
		}

		private void BuildFolder( IDatabase database, StorageFolder parent ) {
			var directories = FileSearcher.Search( StorageHelpers.GetPath( database.Database, parent ), null, SearchOptions.Directories, 0 );

			foreach( var directory in directories ) {
				var folderName = directory.File;
				var folder = mFolderCache.Find( dbFolder => dbFolder.ParentFolder == parent.DbId && dbFolder.Name == folderName );
				if( folder == null ) {
					folder = new StorageFolder( directory.File, parent.DbId );

					database.Insert( folder );

					if( parent is RootFolder ) {
						mLog.LogInfo( string.Format( "Adding folder: {0}", StorageHelpers.GetPath( database.Database, folder )));
					}
				}

				BuildFolderFiles( database, folder );
				BuildFolder( database, folder );

				if( mStopExploring ) {
					break;
				}
			}
		}

		private void BuildFolderFiles( IDatabase database, StorageFolder storageFolder ) {
//			var dbList = ( from StorageFile file in database.Database where file.ParentFolder == storageFolder.DbId select file ).ToList();
			var dbList = mFileCache.FindList( file => file.ParentFolder == storageFolder.DbId );
			var files = FileSearcher.BreadthFirst.Search( StorageHelpers.GetPath( database.Database, storageFolder ), null,
														  SearchOptions.Files | SearchOptions.IncludeSystem | SearchOptions.IncludeHidden, 0 );
			foreach( var file in files ) {
				var fileName = file.File;

				if(!dbList.Exists( dbFile => dbFile.Name == fileName )) {
					database.Insert( new StorageFile( file.File, storageFolder.DbId, file.Size, file.ModificationTime ));
				}

				if( mStopExploring ) {
					break;
				}
			}
		}
	}
}
