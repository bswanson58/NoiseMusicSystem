using System;
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

		public  FolderExplorer( IUnityContainer container ) {
			mContainer = container;
			mLog = mContainer.Resolve<ILog>();
		}

		public void SynchronizeDatabaseFolders() {
			mStopExploring = false;

			var databaseMgr = mContainer.Resolve<IDatabaseManager>();
			var database = databaseMgr.ReserveDatabase();

			if( database != null ) {
				try {
					var rootFolders = from RootFolder root in database.Database where true select root;

					if( rootFolders.Count() == 0 ) {
						LoadConfiguration( database );

						rootFolders = from RootFolder root in database.Database where true select root;
					}

					if( rootFolders.Count() > 0 ) {
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
					}
				}
				catch( Exception ex ) {
					mLog.LogException( "Exception - FolderExplorer:", ex );
				}
				finally {
					databaseMgr.FreeDatabase( database );
				}
			}
		}

		public void Stop() {
			mStopExploring = true;
		}

		private void LoadConfiguration( IDatabase database ) {
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

/*		private void BuildDatabaseFolders( RootFolder rootFolder ) {
			if( Directory.Exists( rootFolder.FullPath )) {
				var directories = FileSearcher.DepthFirst.Search( rootFolder.FullPath,
																  null,
																  SearchOptions.Directories, 
																  FileSearcher.UnrestrictedDepth,
																  delegate( string directory, int depth ) {
																		return( ProgressHandlerResult.Continue );
																  },
																  delegate( string path, Exception ex ) {
																  		return( ExceptionHandlerResult.ConsumeExceptionAndContinue );
																  });
			}
		}
*/
		private void BuildFolder( IDatabase database, StorageFolder parent ) {
			var directories = FileSearcher.Search( StorageHelpers.GetPath( database.Database, parent ), null, SearchOptions.Directories, 0 );

			foreach( var directory in directories ) {
				var folder = new StorageFolder( directory.File, parent.DbId );
				var parentFolder = folder.ParentFolder;
				var parentName = folder.Name;
				var	databaseFolder = ( from StorageFolder dbFolder in database.Database
									   where dbFolder.ParentFolder == parentFolder && dbFolder.Name == parentName select dbFolder ).FirstOrDefault();
				if( databaseFolder == null ) {
					database.Insert( folder );

					if( parent is RootFolder ) {
						mLog.LogInfo( string.Format( "Adding folder: {0}", StorageHelpers.GetPath( database.Database, folder )));
					}
				}
				else {
					folder = databaseFolder;
				}

				BuildFolderFiles( database, folder );
				BuildFolder( database, folder );

				if( mStopExploring ) {
					break;
				}
			}
		}

		private void BuildFolderFiles( IDatabase database, StorageFolder storageFolder ) {
			var dbList = ( from StorageFile file in database.Database where file.ParentFolder == storageFolder.DbId select file ).ToList();
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
