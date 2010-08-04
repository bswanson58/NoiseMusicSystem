using System;
using Noise.Infrastructure.Dto;

namespace Noise.Core.DataBuilders {
	internal interface IContentProvider {
		ContentType		ContentType { get; }
		TimeSpan		ExpirationPeriod { get; }

		bool			CanUpdateArtist { get; }
		bool			CanUpdateAlbum { get; }
		bool			CanUpdateTrack { get; }

		void			UpdateContent( DbArtist forArtist );
		void			UpdateContent( DbAlbum forAlbum );
		void			UpdateContent( DbTrack forTrack );
	}

	public interface IContentManager {
		void			RequestContent( DbArtist forArtist );
		void			RequestContent( DbAlbum forAlbum );
		void			RequestContent( DbTrack forTrack );
	}
}
