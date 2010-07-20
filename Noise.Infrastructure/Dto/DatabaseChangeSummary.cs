namespace Noise.Infrastructure.Dto {
	public class DatabaseChangeSummary {
		public	int		ArtistsAdded { get; set; }
		public	int		ArtistsRemoved { get; set; }
		public	int		AlbumsAdded { get; set; }
		public	int		AlbumsRemoved { get; set; }
		public	int		TracksAdded { get; set; }
		public	int		TracksRemoved { get; set; }

		public bool HaveChanges {
			get { return( ArtistChanges || AlbumChanges || TrackChanges ); }
		}

		public bool ArtistChanges {
			get { return(( ArtistsAdded != 0 ) || ( ArtistsRemoved != 0 )); }
		}

		public bool AlbumChanges {
			get { return(( AlbumsAdded != 0 ) || ( AlbumsRemoved != 0 )); }
		}

		public bool TrackChanges {
			get { return(( TracksAdded != 0 ) || ( TracksRemoved != 0 )); }
		}

		public override string ToString() {
			return( string.Format( "Artists - added: {0}, removed: {1} - Albums - added {2}, removed {3} - Tracks - added: {4}, removed {5}",
									ArtistsAdded, ArtistsRemoved, AlbumsAdded, AlbumsRemoved, TracksAdded, TracksRemoved ));
		}
	}
}
