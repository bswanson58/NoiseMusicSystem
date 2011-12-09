using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Practices.Unity;
using Noise.Core.Database;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;
using Recls;

namespace Noise.Core.FileStore {
	public class FolderExplorer : IFolderExplorer {
		private readonly IUnityContainer	mContainer;
		private bool						mStopExploring;
		private DatabaseCache<StorageFile>	mFileCache;
		private DatabaseCache<StorageFolder>	mFolderCache;

		public  FolderExplorer( IUnityContainer container ) {
			mContainer = container;
		}

		public IEnumerable<RootFolder> RootFolderList() {
			IEnumerable<RootFolder>		retValue = null;

			var databaseMgr = mContainer.Resolve<IDatabaseManager>();
			var database = databaseMgr.ReserveDatabase();

			if( database != null ) {
				try {
					retValue = from RootFolder root in database.Database select root;

					if( retValue.Count() == 0 ) {
						LoadConfiguration( database );

						retValue = from RootFolder root in database.Database select root;
					}
				}
				catch( Exception ex ) {
					NoiseLogger.Current.LogException( "Exception - RootFolderList: ", ex );
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
								NoiseLogger.Current.LogInfo( "Synchronizing folder: {0}", rootFolder.DisplayName );
								BuildFolder( database, rootFolder );
							}
							else {
								NoiseLogger.Current.LogMessage( "Storage folder does not exists: {0}", rootFolder.DisplayName );
							}

							if( mStopExploring ) {
								break;
							}

							var localRoot = database.ValidateOnThread( rootFolder ) as RootFolder;
							if( localRoot != null ) {
								localRoot.UpdateLibraryScan();
								database.Store( localRoot );
							}
						}

						mFileCache.Clear();
						mFolderCache.Clear();
					}
					catch( Exception ex ) {
						NoiseLogger.Current.LogException( "Exception - FolderExplorer:", ex );
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
			var storageConfig = NoiseSystemConfiguration.Current.RetrieveConfiguration<StorageConfiguration>( StorageConfiguration.SectionName  );
			var rootCount = ( from RootFolder root in database.Database select root ).Count();

			if(( storageConfig != null ) &&
			   ( rootCount == 0 ) &&
			   ( storageConfig.RootFolders != null )) {
				foreach( RootFolderConfiguration folderConfig in storageConfig.RootFolders ) {
					var root = new RootFolder( folderConfig.Key, folderConfig.Path, folderConfig.Description );

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
			var folderList = mFolderCache.FindList( dbFolder => dbFolder.ParentFolder == parent.DbId );

			foreach( var directory in directories ) {
				var folderName = directory.File;

				var folder = folderList.Find( dbFolder => dbFolder.Name == folderName );
				if( folder == null ) {
					folder = new StorageFolder( directory.File, parent.DbId );

					database.Insert( folder );

					if( parent is RootFolder ) {
						NoiseLogger.Current.LogInfo( string.Format( "Adding folder: {0}", StorageHelpers.GetPath( database.Database, folder )));
					}
				}
				else {
					folderList.Remove( folder );
				}

				BuildFolderFiles( database, folder );
				BuildFolder( database, folder );

				if( mStopExploring ) {
					break;
				}
			}

			// Any folders remaining in the list must not have located on disk.
			foreach( var dbFolder in folderList ) {
				dbFolder.IsDeleted = true;

				database.Store( dbFolder );
			}
		}

		private void BuildFolderFiles( IDatabase database, StorageFolder storageFolder ) {
			var dbList = mFileCache.FindList( file => file.ParentFolder == storageFolder.DbId );
			var files = FileSearcher.BreadthFirst.Search( StorageHelpers.GetPath( database.Database, storageFolder ), null,
														  SearchOptions.Files | SearchOptions.IncludeSystem | SearchOptions.IncludeHidden, 0 );
			foreach( var file in files ) {
				var fileName = file.File;

				var	dbFile = dbList.Find( f => f.Name == fileName );
				if( dbFile != null ) {
					dbList.Remove( dbFile );
				}
				else {
					database.Insert( new StorageFile( file.File, storageFolder.DbId, file.Size, file.ModificationTime ));
				}

				if( mStopExploring ) {
					break;
				}
			}

			// Any files that we have remaining must not exist on disk.
			foreach( var dbFile in dbList ) {
				dbFile.IsDeleted = true;

				database.Store( dbFile );
			}
		}
	}
}
