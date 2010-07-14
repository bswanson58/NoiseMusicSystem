using System.IO;
using System.Linq;
using Eloquera.Linq;
using Microsoft.Practices.Unity;
using Noise.Core.Database;
using Noise.Core.Exceptions;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Recls;

namespace Noise.Core.FileStore {
	public class FolderExplorer : IFolderExplorer {
		private readonly IUnityContainer	mContainer;
		private readonly IDatabaseManager	mDatabase;
		private readonly ILog				mLog;

		public  FolderExplorer( IUnityContainer container ) {
			mContainer = container;
			mLog = mContainer.Resolve<ILog>();

			mDatabase = mContainer.Resolve<IDatabaseManager>( Constants.NewInstance );
			if( mDatabase.InitializeDatabase()) {
				mDatabase.OpenDatabase();
			}
		}

		public void SynchronizeDatabaseFolders() {
			var rootFolders = from RootFolder root in mDatabase.Database where true select root;

			if( rootFolders.Count() == 0 ) {
				LoadConfiguration();

				rootFolders = from RootFolder root in mDatabase.Database where true select root;
			}

			if( rootFolders.Count() > 0 ) {
				foreach( var rootFolder in rootFolders ) {
					if( Directory.Exists( StorageHelpers.GetPath( mDatabase.Database, rootFolder ))) {
						mLog.LogInfo( "Synchronizing folder: {0}", rootFolder.DisplayName );
						BuildFolder( rootFolder );
					}
					else {
						mLog.LogMessage( "Storage folder does not exists: {0}", rootFolder.DisplayName );
					}
				}
			}
			else {
				throw( new StorageConfigurationException());
			}
		}

		private void LoadConfiguration() {
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

					mDatabase.Database.Store( root );
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
		private void BuildFolder( StorageFolder parent ) {
			var directories = FileSearcher.Search( StorageHelpers.GetPath( mDatabase.Database, parent ), null, SearchOptions.Directories, 0 );

			foreach( var directory in directories ) {
				var folder = new StorageFolder( directory.File, mDatabase.Database.GetUid( parent ));
				var param = mDatabase.Database.CreateParameters();

				param["parent"] = folder.ParentFolder;
				param["name"] = folder.Name;

				var databaseFolder = mDatabase.Database.ExecuteScalar( "SELECT StorageFolder WHERE ParentFolder = @parent AND Name = @name", param ) as StorageFolder;
				if( databaseFolder == null ) {
					mDatabase.Database.Store( folder );
				}
				else {
					folder = databaseFolder;
				}

				BuildFolderFiles( folder );
				BuildFolder( folder );
			}
		}

		private void BuildFolderFiles( StorageFolder storageFolder ) {
			var	parentId =  mDatabase.Database.GetUid( storageFolder );
			var param = mDatabase.Database.CreateParameters();

			param["parent"] = parentId;

			var databaseFiles = mDatabase.Database.ExecuteQuery( "SELECT StorageFile WHERE ParentFolder = @parent", param );
			var dbList = databaseFiles.Cast<StorageFile>().ToList();
			var files = FileSearcher.BreadthFirst.Search( StorageHelpers.GetPath( mDatabase.Database, storageFolder ), null,
														  SearchOptions.Files | SearchOptions.IncludeSystem | SearchOptions.IncludeHidden, 0 );

			foreach( var file in files ) {
				var fileName = file.File;

				if(!dbList.Exists( dbFile => dbFile.Name == fileName )) {
					mDatabase.Database.Store( new StorageFile( file.File, parentId, file.Size, file.ModificationTime ));
				}
			}
		}
	}
}
