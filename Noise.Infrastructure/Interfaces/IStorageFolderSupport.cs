using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Support;

namespace Noise.Infrastructure.Interfaces {
	public interface IStorageFolderSupport {
		string						GetPath( StorageFolder forFolder );
		string						GetPath( StorageFile forFile );
		string						GetArtistPath( long artistId);
		string						GetAlbumPath( long albumId );

		FolderStrategyInformation	GetFolderStrategy( StorageFile forFile );

		eFileType					DetermineFileType( StorageFile file );
		eAudioEncoding				DetermineAudioEncoding( StorageFile file );

		bool						IsCoverFile( string fileName );

		short						ConvertFromId3Rating( short rating );
		byte						ConvertToId3Rating( short rating );
	}
}
