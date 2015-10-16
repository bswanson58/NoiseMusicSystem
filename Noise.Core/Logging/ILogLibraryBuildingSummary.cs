using System;
using System.Runtime.CompilerServices;
using Noise.Infrastructure.Dto;

namespace Noise.Core.Logging {
	public interface ILogLibraryBuildingSummary {
		void	LogSummaryBuildingStarted();
		void	LogSummaryBuildingCompleted();

		void	LogSummaryArtistStarted( DbArtist artist );
		void	LogSummaryArtistCompleted( DbArtist artist );
		void	LogSummaryAlbumStarted( DbAlbum album );
		void	LogSummaryAlbumCompleted( DbAlbum album );

		void	LogSummaryBuildingException( string message, Exception exception, [CallerMemberName] string callerName = "" );
	}
}
