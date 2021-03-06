﻿using System;
using System.Runtime.CompilerServices;
using Noise.Infrastructure.Dto;

namespace Noise.Core.Logging {
	public interface ILogLibraryBuildingSidecars {
		void	LogSidecarBuildingStarted();
		void	LogSidecarBuildingCompleted();

		void	LogLoadedSidecar( StorageSidecar sidecar, DbAlbum album );
		void	LogWriteSidecar( ScAlbum scAlbum );
		void	LogWriteSidecar( ScArtist scArtist );
		void	LogUpdatedSidecar( StorageSidecar sidecar, DbAlbum album );
		void	LogUpdatedSidecar( StorageSidecar sidecar, DbArtist artist );
		void	LogUpdated( DbAlbum dbAlbum, ScAlbum scAlbum );
		void	LogUpdated( DbArtist dbArtist, ScArtist scArtist );

		void	LogUnknownArtistSidecar( StorageSidecar sidecar );
		void	LogUnknownAlbumSidecar( StorageSidecar sidecar );
		void	LogUnknownTrack( DbAlbum album, ScTrack track );

		void	LogException( string message, Exception exception, [CallerMemberName] string callerName = "" );
	}
}
