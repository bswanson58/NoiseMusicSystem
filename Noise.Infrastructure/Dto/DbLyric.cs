using System;
using System.ComponentModel.Composition;
using System.Diagnostics;

namespace Noise.Infrastructure.Dto {
	[DebuggerDisplay("Lyrics = {SongName}")]
	public class DbLyric : DbBase {
		public	long	ArtistId { get; private set; }
		public	long	TrackId { get; private set; }
		public	string	SongName { get; private set; }
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

		[Export("PersistenceType")]
		public static Type PersistenceType {
			get{ return( typeof( DbLyric )); }
		}
	}
}
