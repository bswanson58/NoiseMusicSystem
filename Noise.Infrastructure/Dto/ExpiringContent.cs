using System;
using Eloquera.Client;

namespace Noise.Infrastructure.Dto {
	public abstract class ExpiringContent : DbBase {
		public	long		AssociatedItem { get; protected set; }
		public	long		Artist { get; set; }
		public	long		Album { get; set; }
		public	ContentType	ContentType { get; protected set; }
		public	DateTime	HarvestDate { get; protected set; }
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

		protected ExpiringContent( long associatedItem, ContentType contentType ) {
			AssociatedItem = associatedItem;
			ContentType = contentType;
			IsContentAvailable = false;

			Artist = Constants.cDatabaseNullOid;
			Album = Constants.cDatabaseNullOid;

			UpdateExpiration();
		}

		[Ignore]
		public int DbContentType {
			get{ return((int)ContentType ); }
			set{ ContentType = (ContentType)value; }
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
