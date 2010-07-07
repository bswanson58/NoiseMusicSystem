using System;
using System.ComponentModel.Composition;

namespace Noise.Infrastructure.Dto {
	public class DbArtwork {
		public	long			AssociatedItem { get; private set; }
		public	long			FolderLocation { get; set; }
		public	ArtworkTypes	ArtworkType { get; set; }
		public	InfoSource		Source { get; set; }
		public	byte[]			Image { get; set; }

		public DbArtwork( long associatedItem ) {
			AssociatedItem = associatedItem;
			ArtworkType = ArtworkTypes.Unknown;
			Source = InfoSource.Unknown;
			FolderLocation = Constants.cDatabaseNullOid;
		}

		[Export("PersistenceType")]
		public static Type PersistenceType {
			get{ return( typeof( DbArtwork )); }
		}
	}
}
