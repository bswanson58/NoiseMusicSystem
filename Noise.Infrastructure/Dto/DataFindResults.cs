namespace Noise.Infrastructure.Dto {
	public class DataFindResults {
		public	long			DatabaseId { get; private set; }
		public	DbArtist		Artist { get; private set; }
		public	DbAlbum			Album { get; private set; }
		public	DbTrack			Track { get; private set; }
		public	bool			WasSuccessful { get; private set; }

		public DataFindResults( long databaseId, DbArtist artist, bool success ) {
			DatabaseId = databaseId;
			Artist = artist;

			WasSuccessful = success;
		}

		public DataFindResults( long databaseId, DbArtist artist, DbAlbum album, bool success ) :
			this( databaseId, artist, success ) {
			Album = album;
		}

		public DataFindResults( long databaseId, DbArtist artist, DbAlbum album, DbTrack track, bool success ) :
			this( databaseId, artist, album, success ) {
			Track = track;
		}
	}
}
