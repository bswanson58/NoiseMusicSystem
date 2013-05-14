using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.RavenDatabase.Interfaces;
using Noise.RavenDatabase.Support;

namespace Noise.RavenDatabase.DataProviders {
	public class StorageFileProvider : BaseProvider<StorageFile>, IStorageFileProvider {
		public StorageFileProvider( IDbFactory databaseFactory ) :
			base( databaseFactory, entity => new object[] { entity.DbId }) {
		}

		public void AddFile( StorageFile file ) {
			Database.Add( file );
		}

		public void DeleteFile( StorageFile file ) {
			Database.Delete( file );
		}

		public StorageFile GetPhysicalFile( DbTrack forTrack ) {
			return( Database.Get( track => track.MetaDataPointer == forTrack.DbId ));
		}

		public IDataProviderList<StorageFile> GetAllFiles() {
			return( new RavenDataProviderList<StorageFile>( Database.FindAll()));
		}

		public IDataProviderList<StorageFile> GetDeletedFilesList() {
			return( new RavenDataProviderList<StorageFile>( Database.Find( entity => entity.IsDeleted )));
		}

		public IDataProviderList<StorageFile> GetFilesInFolder( long parentFolder ) {
			return( new RavenDataProviderList<StorageFile>( Database.Find( entity => entity.ParentFolder == parentFolder )));
		}

		public IDataProviderList<StorageFile> GetFilesRequiringProcessing() {
			return( new RavenDataProviderList<StorageFile>( Database.Find( entity => entity.FileType == eFileType.Undetermined || entity.WasUpdated )));
		}

		public IDataUpdateShell<StorageFile> GetFileForUpdate( long fileId ) {
			return( new RavenDataUpdateShell<StorageFile>( entity => Database.Update( entity ), Database.Get( fileId )));
		}

		public long GetItemCount() {
			return( Database.Count());
		}
	}
}
