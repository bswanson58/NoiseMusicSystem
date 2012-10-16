using Noise.BlobStorage.BlobStore;

namespace Noise.Metadata.Interfaces {
	public interface IArtistMetadataManager {
		void		Initialize( IBlobStorageManager blobStorageManager );
		void		Shutdown();

		void		ArtistMentioned( string artistName );
		void		ArtistForgotten( string artistName );

		void		ArtistMetadataRequested( string artistName );
	}
}
