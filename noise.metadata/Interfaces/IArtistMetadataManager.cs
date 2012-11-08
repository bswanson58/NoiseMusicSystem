﻿using Noise.Infrastructure.Interfaces;
using Raven.Client;

namespace Noise.Metadata.Interfaces {
	public interface IArtistMetadataManager {
		void			Initialize( IDocumentStore documentStore );
		void			Shutdown();

		void			ArtistMentioned( string artistName );
		void			ArtistForgotten( string artistName );

		IArtistMetadata	GetArtistBiography( string artistName );
	}
}
