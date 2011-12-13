using System;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.DataBuilders {
	internal interface IContentProvider {
		bool			Initialize( INoiseManager noiseManager );

		ContentType		ContentType { get; }
		TimeSpan		ExpirationPeriod { get; }

		bool			CanUpdateArtist { get; }
		bool			CanUpdateAlbum { get; }
		bool			CanUpdateTrack { get; }

		void			UpdateContent( IDatabase database, DbArtist forArtist );
		void			UpdateContent( IDatabase database, DbAlbum forAlbum );
		void			UpdateContent( IDatabase database, DbTrack forTrack );
	}

	public interface IContentManager {
		bool			Initialize( INoiseManager noiseManager );
	}
}
