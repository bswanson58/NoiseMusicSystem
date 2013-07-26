using System.Collections.Generic;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IReplayGainScanner {
		void						AddFile( string filePath );
		void						AddFile( long fileId, string filePath );
		void						AddFile( ReplayGainFile file );

		void						AddAlbum( string directory );
		void						AddAlbum( IEnumerable<ReplayGainFile> fileList );

		bool						CalculateReplayGain();

		IEnumerable<ReplayGainFile>	FileList { get; }
		double						AlbumGain { get; }

		void						ResetScanner();
	}
}
