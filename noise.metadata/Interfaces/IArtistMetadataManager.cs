namespace Noise.Metadata.Interfaces {
	public interface IArtistMetadataManager {
		void		Initialize();
		void		Shutdown();

		void		ArtistMentioned( string artistName );
		void		ArtistForgotten( string artistName );

		void		ArtistMetadataRequested( string artistName );
	}
}
