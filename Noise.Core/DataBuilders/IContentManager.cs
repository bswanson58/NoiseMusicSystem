using System;
using Microsoft.Practices.Unity;
using Noise.Core.Database;
using Noise.Infrastructure.Dto;

namespace Noise.Core.DataBuilders {
	internal interface IContentProvider {
		bool			Initialize( IUnityContainer container );

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
		void			RequestContent( DbArtist forArtist );
		void			RequestContent( DbAlbum forAlbum );
		void			RequestContent( DbTrack forTrack );
	}
}
