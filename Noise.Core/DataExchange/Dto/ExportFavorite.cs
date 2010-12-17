namespace Noise.Core.DataExchange.Dto {
	public class ExportFavorite : ExportBase {
		public	bool		IsFavorite { get; private set; }

		public ExportFavorite( long originDb, string artist, bool isFavorite ) :
			base( originDb, artist ) {
			IsFavorite = isFavorite;
		}

		public ExportFavorite( long originDb, string artist, string album, bool isFavorite ) :
			base( originDb, artist, album ) {
			IsFavorite = isFavorite;
		}

		public ExportFavorite( long originDb, string artist, string album, string track, bool isFavorite ) :
			base( originDb, artist, album, track ) {
			IsFavorite = isFavorite;
		}

		public ExportFavorite( string stream, long originDb, bool isFavorite ) :
			base( stream, originDb ) {
			IsFavorite = isFavorite;
		}
	}
}
