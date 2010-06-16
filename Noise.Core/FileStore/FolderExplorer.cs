using System.IO;
using System.Linq;
using Eloquera.Linq;
using Microsoft.Practices.Unity;
using Recls;

namespace Noise.Core.FileStore {
	public class FolderExplorer : IFolderExplorer {
		private readonly IUnityContainer	mContainer;
		private readonly IDatabaseManager	mDatabase;

		public  FolderExplorer( IUnityContainer container ) {
			mContainer = container;
			mDatabase = mContainer.Resolve<IDatabaseManager>();
		}

		public void SynchronizeDatabaseFolders() {
			var rootFolders = from RootFolder root in mDatabase.Database where true select root;

			foreach( var rootFolder in rootFolders ) {
				if( Directory.Exists( StorageHelpers.GetPath( mDatabase.Database, rootFolder ))) {
					BuildFolder( rootFolder );
				}
			}

			var folders = from StorageFolder folder in mDatabase.Database where true select folder;
			var	count = folders.Select( folder => folder.Name ).Count();
			var files = from StorageFile file in mDatabase.Database where true select file;
			var fileCount = files.Select( file => file.Name ).Count();
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
			var files = FileSearcher.Search( StorageHelpers.GetPath( mDatabase.Database, storageFolder ), null, SearchOptions.Files, 0 );

			foreach( var file in files ) {
				var fileName = file.File;

				if(!dbList.Exists( dbFile => dbFile.Name == fileName )) {
					mDatabase.Database.Store( new StorageFile( file.File, parentId ));
				}
			}
		}
	}
}
