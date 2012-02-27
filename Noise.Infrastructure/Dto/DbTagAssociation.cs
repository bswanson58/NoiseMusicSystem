using System;
using System.ComponentModel.Composition;

namespace Noise.Infrastructure.Dto {
	public class DbTagAssociation : DbBase {
		public	long		TagId { get; protected set; }
		public	eTagGroup	TagGroup { get; protected set; }
		public	long		ArtistId { get; protected set; }
		public	long		AlbumId { get; protected set; }

		protected DbTagAssociation() :
			this( eTagGroup.Unknown, Constants.cDatabaseNullOid, Constants.cDatabaseNullOid, Constants.cDatabaseNullOid ) { }

		public DbTagAssociation( eTagGroup group, long tagId, long artistId, long albumId ) {
			TagId = tagId;
			TagGroup = group;
			ArtistId = artistId;
			AlbumId = albumId;
		}

		public int DbTagGroup {
			get{ return((int)TagGroup ); }
			protected set{ TagGroup = (eTagGroup)value; }
		}

		[Export("PersistenceType")]
		public static Type PersistenceType {
			get{ return( typeof( DbTagAssociation )); }
		}
	}
}
