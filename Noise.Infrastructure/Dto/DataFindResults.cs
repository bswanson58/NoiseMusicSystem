namespace Noise.Infrastructure.Dto {
	public class DataFindResults {
		public	long			DatabaseId { get; }
		public	DbArtist		Artist { get; }
		public	DbAlbum			Album { get; }
		public	DbTrack			Track { get; }
		public	bool			WasSuccessful { get; }

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
