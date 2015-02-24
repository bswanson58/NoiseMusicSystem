using System.Diagnostics;

namespace Noise.Infrastructure.Dto {
	[DebuggerDisplay("Lyrics = {SongName}")]
	public class DbLyric : DbBase {
		public	long	ArtistId { get; protected set; }
		public	long	TrackId { get; protected set; }
		public	string	SongName { get; protected set; }
		public	string	SourceUrl { get; set; }
		public	string	Lyrics { get; set; }

		protected DbLyric() :
			this( Constants.cDatabaseNullOid, Constants.cDatabaseNullOid, string.Empty ) { }

		public DbLyric( long artistId, long trackId, string trackName ) {
			ArtistId = artistId;
			TrackId = trackId;
			SongName = trackName;

			SourceUrl = "";
			Lyrics = "";
		}

		public override string ToString() {
			return( string.Format( "Lyrics for \"{0}\"", SongName ));
		}
	}
}
