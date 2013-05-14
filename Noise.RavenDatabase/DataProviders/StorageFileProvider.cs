using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.RavenDatabase.Interfaces;
using Noise.RavenDatabase.Support;

namespace Noise.RavenDatabase.DataProviders {
	public class StorageFileProvider : IStorageFileProvider {
		private readonly IDbFactory					mDbFactory;
		private readonly IRepository<StorageFile>	mDatabase;

		public StorageFileProvider( IDbFactory databaseFactory ) {
			mDbFactory = databaseFactory;

			mDatabase = new RavenRepositoryT<StorageFile>( mDbFactory.GetLibraryDatabase(), entity => new object[] { entity.DbId });
		}
		public void AddFile( StorageFile file ) {
			mDatabase.Add( file );
		}

		public void DeleteFile( StorageFile file ) {
			mDatabase.Delete( file );
		}

		public StorageFile GetPhysicalFile( DbTrack forTrack ) {
			return( mDatabase.Get( track => track.MetaDataPointer == forTrack.DbId ));
		}

		public IDataProviderList<StorageFile> GetAllFiles() {
			return( new RavenDataProviderList<StorageFile>( mDatabase.FindAll()));
		}

		public IDataProviderList<StorageFile> GetDeletedFilesList() {
			return( new RavenDataProviderList<StorageFile>( mDatabase.Find( entity => entity.IsDeleted )));
		}

		public IDataProviderList<StorageFile> GetFilesInFolder( long parentFolder ) {
			return( new RavenDataProviderList<StorageFile>( mDatabase.Find( entity => entity.ParentFolder == parentFolder )));
		}

		public IDataProviderList<StorageFile> GetFilesRequiringProcessing() {
			return( new RavenDataProviderList<StorageFile>( mDatabase.Find( entity => entity.FileType == eFileType.Undetermined || entity.WasUpdated )));
		}

		public IDataUpdateShell<StorageFile> GetFileForUpdate( long fileId ) {
			return( new RavenDataUpdateShell<StorageFile>( entity => mDatabase.Update( entity ), mDatabase.Get( fileId )));
		}

		public long GetItemCount() {
			return( mDatabase.Count());
		}
	}
}
