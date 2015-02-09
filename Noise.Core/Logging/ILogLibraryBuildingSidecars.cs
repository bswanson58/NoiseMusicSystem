using System;
using System.Runtime.CompilerServices;
using Noise.Infrastructure.Dto;

namespace Noise.Core.Logging {
	public interface ILogLibraryBuildingSidecars {
		void	LogSidecarBuildingStarted();
		void	LogSidecarBuildingCompleted();

		void	LogLoadedSidecar( StorageSidecar sidecar, DbAlbum album );
		void	LogUpdatedSidecar( StorageSidecar sidecar, DbAlbum album );
		void	LogUpdatedAlbum( StorageSidecar sidecar, DbAlbum album );
		void	LogUnknownAlbumSidecar( StorageSidecar sidecar );

		void	LogException( string message, Exception exception, [CallerMemberName] string callerName = "" );
	}
}
