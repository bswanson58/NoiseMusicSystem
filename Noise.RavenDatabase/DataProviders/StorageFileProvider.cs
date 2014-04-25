using System.Collections.Generic;
using System.Linq;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.RavenDatabase.Interfaces;
using Noise.RavenDatabase.Support;
using Raven.Client.Indexes;

namespace Noise.RavenDatabase.DataProviders {
	public class StorageFileByDbId : AbstractIndexCreationTask<StorageFile> {
		public StorageFileByDbId() {
			Map = files => from file in files select new { file.DbId };
		}
	}

	public class StorageFileByParentFolder : AbstractIndexCreationTask<StorageFile> {
		public StorageFileByParentFolder() {
			Map = storageFiles => from file in storageFiles select new { file.ParentFolder };
		}
	}

	public class StorageFileByFileType : AbstractIndexCreationTask<StorageFile> {
		public StorageFileByFileType() {
			Map = storageFiles => from file in storageFiles select new { file.FileType, file.WasUpdated };
		}
	}

	public class StorageFileByIsDeleted : AbstractIndexCreationTask<StorageFile> {
		public StorageFileByIsDeleted() {
			Map = storageFiles => from file in storageFiles select new { file.IsDeleted };
		}
	}

	public class StorageFileProvider : BaseProvider<StorageFile>, IStorageFileProvider {
		public StorageFileProvider( IDbFactory databaseFactory ) :
			base( databaseFactory, entity => new object[] { entity.DbId }) {
		}

		public void AddFile( StorageFile file ) {
			Database.Add( file );
		}

		public void Add( IEnumerable<StorageFile> list ) {
			foreach( var item in list ) {
				AddFile( item );
			}
		}

		public void DeleteFile( StorageFile file ) {
			Database.Delete( file );
		}

		public StorageFile GetPhysicalFile( DbTrack forTrack ) {
			return( Database.Get( track => track.MetaDataPointer == forTrack.DbId ));
		}

		public IDataProviderList<StorageFile> GetAllFiles() {
			return( Database.FindAll());
		}

		public IDataProviderList<StorageFile> GetDeletedFilesList() {
			return( Database.Find( entity => entity.IsDeleted, typeof( StorageFileByIsDeleted ).Name ));
		}

		public IDataProviderList<StorageFile> GetFilesInFolder( long parentFolder ) {
			return ( Database.Find( entity => entity.ParentFolder == parentFolder, typeof( StorageFileByParentFolder ).Name));
		}

		public IDataProviderList<StorageFile> GetFilesRequiringProcessing() {
			return ( Database.Find( entity => ( entity.FileType == eFileType.Undetermined ) || entity.WasUpdated, typeof( StorageFileByFileType ).Name ));
		}

		public IDataUpdateShell<StorageFile> GetFileForUpdate( long fileId ) {
			return( new RavenDataUpdateShell<StorageFile>( entity => Database.Update( entity ), Database.Get( fileId )));
		}

		public long GetItemCount() {
			return( Database.Count());
		}
	}
}
