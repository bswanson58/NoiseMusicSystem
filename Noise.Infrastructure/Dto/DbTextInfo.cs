using System;
using System.ComponentModel.Composition;

namespace Noise.Infrastructure.Dto {
	public class DbTextInfo : ExpiringContent {
		public	long			FolderLocation { get; set; }
		public	InfoSource		Source { get; set; }
		public	string			Text { get; set; }

		public DbTextInfo( long associatedItem, ContentType contentType ) :
		base( associatedItem, contentType ) {
			FolderLocation = Constants.cDatabaseNullOid;
			Source = InfoSource.Unknown;
			Text = "";
		}

		[Export("PersistenceType")]
		public static Type PersistenceType {
			get{ return( typeof( DbTextInfo )); }
		}
	}
}
