using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IStorageFileProvider {
		StorageFile		GetPhysicalFile( DbTrack forTrack );
		string			GetPhysicalFilePath( StorageFile forFile );
		string			GetAlbumPath( long albumId );
	}
}
