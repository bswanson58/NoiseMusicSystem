using System;

namespace Noise.Infrastructure.Dto {
	public class ExpiringContent : DbBase {
		public	long		AssociatedItem { get; private set; }
		public	long		Artist { get; set; }
		public	long		Album { get; set; }
		public	ContentType	ContentType { get; private set; }
		public	DateTime	HarvestDate { get; private set; }
		public	bool		IsContentAvailable { get; set; }

		protected ExpiringContent( ExpiringContent clone ) :
			base( clone.DbId ) {
			AssociatedItem = clone.AssociatedItem;
			Artist = clone.Artist;
			Album = clone.Album;
			ContentType = clone.ContentType;
			HarvestDate = clone.HarvestDate;
			IsContentAvailable = clone.IsContentAvailable;
		}

		public ExpiringContent( long associatedItem, ContentType contentType ) {
			AssociatedItem = associatedItem;
			ContentType = contentType;
			IsContentAvailable = false;

			Artist = Constants.cDatabaseNullOid;
			Album = Constants.cDatabaseNullOid;

			UpdateExpiration();
		}

		protected void Copy( ExpiringContent copy ) {
			Artist = copy.Artist;
			Album = copy.Album;
			IsContentAvailable = copy.IsContentAvailable;
			HarvestDate = copy.HarvestDate;
		}

		public void UpdateExpiration() {
			HarvestDate = DateTime.Now.Date;
		}
	}
}
