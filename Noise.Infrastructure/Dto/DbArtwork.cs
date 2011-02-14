using System;
using System.ComponentModel.Composition;

namespace Noise.Infrastructure.Dto {
	public class DbArtwork : ExpiringContent {
		public	long			FolderLocation { get; set; }
		public	InfoSource		Source { get; set; }
		public	byte[]			Image { get; set; }
		public	bool			IsUserSelection { get; set; }
		public	string			Name { get; set; }
		public	Int16			Rotation { get; set; }

		public DbArtwork( long associatedItem, ContentType contentType ) :
			base( associatedItem, contentType ) {
			Source = InfoSource.Unknown;
			FolderLocation = Constants.cDatabaseNullOid;
			Rotation = 0;
		}

		[Export("PersistenceType")]
		public static Type PersistenceType {
			get{ return( typeof( DbArtwork )); }
		}
	}
}
