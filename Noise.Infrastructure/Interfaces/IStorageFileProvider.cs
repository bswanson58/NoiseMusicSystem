using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IStorageFileProvider {
		void							AddFile( StorageFile file );
		void							DeleteFile( StorageFile file );

		StorageFile						GetPhysicalFile( DbTrack forTrack );
		string							GetPhysicalFilePath( StorageFile forFile );
		string							GetAlbumPath( long albumId );

		DataProviderList<StorageFile>	GetAllFiles();
		DataProviderList<StorageFile>	GetDeletedFilesList();
		DataProviderList<StorageFile>	GetFilesInFolder( long parentFolder );
		DataProviderList<StorageFile>	GetFilesOfType( eFileType fileType );

		DataUpdateShell<StorageFile>	GetFileForUpdate( long fileId );
	}
}
