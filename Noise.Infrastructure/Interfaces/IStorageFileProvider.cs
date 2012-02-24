using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IStorageFileProvider {
		void							AddFile( StorageFile file );
		void							DeleteFile( StorageFile file );

		StorageFile						GetPhysicalFile( DbTrack forTrack );
		string							GetPhysicalFilePath( StorageFile forFile );
		string							GetAlbumPath( long albumId );

		IDataProviderList<StorageFile>	GetAllFiles();
		IDataProviderList<StorageFile>	GetDeletedFilesList();
		IDataProviderList<StorageFile>	GetFilesInFolder( long parentFolder );
		IDataProviderList<StorageFile>	GetFilesOfType( eFileType fileType );
		
		IDataUpdateShell<StorageFile>	GetFileForUpdate( long fileId );

		long							GetItemCount();
	}
}
