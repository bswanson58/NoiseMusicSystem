using System;
using System.ComponentModel.Composition;

namespace Noise.Infrastructure.Dto {
	public class DbTextInfo : ExpiringContent {
		public	long			AssociatedItem { get; private set; }
		public	long			FolderLocation { get; set; }
		public	TextInfoTypes	InfoType { get; set; }
		public	InfoSource		Source { get; set; }
		public	string			Text { get; set; }

		public DbTextInfo( long associatedItem ) {
			AssociatedItem = associatedItem;

			FolderLocation = Constants.cDatabaseNullOid;
			InfoType = TextInfoTypes.Unknown;
			Source = InfoSource.Unknown;
			Text = "";
		}

		[Export("PersistenceType")]
		public static Type PersistenceType {
			get{ return( typeof( DbTextInfo )); }
		}
	}
}
