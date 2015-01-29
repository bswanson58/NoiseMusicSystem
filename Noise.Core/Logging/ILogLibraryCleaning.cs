using System;
using System.Runtime.CompilerServices;
using Noise.Infrastructure.Dto;

namespace Noise.Core.Logging {
	public interface ILogLibraryCleaning {
		void	LogCleaningStarted();
		void	LogCleaningCompleted();

		void	LogRemovingFolder( StorageFolder folder );
		void	LogRemovingFile( StorageFile file );
		void	LogRemovingTrack( DbTrack track );
		void	LogRemovingAlbum( DbAlbum album );
		void	LogRemovingArtist( DbArtist artist );
		void	LogRemovingArtwork( DbArtwork artwork );
		void	LogRemovingTextInfo( DbTextInfo textInfo );

		void	LogCleaningException( string message, Exception exception, [CallerMemberName] string callerName = "" );
	}
}
