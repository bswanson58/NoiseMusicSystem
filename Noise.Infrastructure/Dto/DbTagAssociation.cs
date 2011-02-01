using System;
using System.ComponentModel.Composition;

namespace Noise.Infrastructure.Dto {
	public class DbTagAssociation : DbBase {
		public	long		TagId { get; private set; }
		public	eTagGroup	TagGroup { get; private set; }
		public	long		ArtistId { get; private set; }
		public	long		AlbumId { get; private set; }

		public DbTagAssociation( eTagGroup group, long tagId, long artistId, long albumId ) {
			TagId = tagId;
			TagGroup = group;
			ArtistId = artistId;
			AlbumId = albumId;
		}

		[Export("PersistenceType")]
		public static Type PersistenceType {
			get{ return( typeof( DbTagAssociation )); }
		}
	}
}
