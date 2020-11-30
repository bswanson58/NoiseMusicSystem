namespace Noise.Metadata.Interfaces {
	public interface IMetadataUpdater {
		void	Initialize();
		void	Shutdown();

		void	QueueArtistUpdate( string forArtist );
	}
}
