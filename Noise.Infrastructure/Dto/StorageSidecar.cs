using System.Diagnostics;

namespace Noise.Infrastructure.Dto {
	public enum SidecarStatus {
		Read,
		Unread
	}

	[DebuggerDisplay("Sidecar = {Name}")]
	public class StorageSidecar : DbBase {
		public	string			Name { get; private set; }
		public	long			ArtistId { get; private set; }
		public	long			AlbumId { get; private set; }
		public	long			Version { get; set; }
		public	SidecarStatus	Status { get; set; }

		public StorageSidecar() {
			Name = string.Empty;
			ArtistId = Constants.cDatabaseNullOid;
			AlbumId = Constants.cDatabaseNullOid;
			Version = 0;
			Status = SidecarStatus.Unread;
		}

		public StorageSidecar( string fileName, DbAlbum album ) :
		this() {
			Name = fileName;
			AlbumId = album.DbId;
			Version = album.Version;
		}

		public StorageSidecar( string fileName, DbArtist artist ) :
			this() {
			Name = fileName;
			ArtistId = artist.DbId;
		}

		public override string ToString() {
			return( string.Format( "Sidecar \"{0}\", Version:{1} Artist: {2}, Album: {3}", Name, Version, ArtistId, AlbumId ));
		}
	}
}
