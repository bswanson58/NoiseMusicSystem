namespace Noise.Infrastructure.Dto {
	public class DataFindResults {
		public	DbArtist		Artist { get; private set; }
		public	DbAlbum			Album { get; private set; }
		public	DbTrack			Track { get; private set; }
		public	bool			WasSuccessful { get; private set; }

		public DataFindResults( DbArtist artist, bool success ) {
			Artist = artist;

			WasSuccessful = success;
		}

		public DataFindResults( DbArtist artist, DbAlbum album, bool success ) :
			this( artist, success ) {
			Album = album;
		}

		public DataFindResults( DbArtist artist, DbAlbum album, DbTrack track, bool success ) :
			this( artist, album, success ) {
			Track = track;
		}
	}
}
