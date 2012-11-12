using Eloquera.Client;

namespace Noise.Infrastructure.Dto {
	public abstract class AssociatedContent : DbBase {
		public	long		AssociatedItem { get; protected set; }
		public	long		Artist { get; set; }
		public	long		Album { get; set; }
		public	ContentType	ContentType { get; protected set; }
		public	bool		IsContentAvailable { get; set; }

		protected AssociatedContent( AssociatedContent clone ) :
			base( clone.DbId ) {
			AssociatedItem = clone.AssociatedItem;
			Artist = clone.Artist;
			Album = clone.Album;
			ContentType = clone.ContentType;
			IsContentAvailable = clone.IsContentAvailable;
		}

		protected AssociatedContent( long associatedItem, ContentType contentType ) {
			AssociatedItem = associatedItem;
			ContentType = contentType;
			IsContentAvailable = false;

			Artist = Constants.cDatabaseNullOid;
			Album = Constants.cDatabaseNullOid;
		}

		[Ignore]
		public int DbContentType {
			get{ return((int)ContentType ); }
			set{ ContentType = (ContentType)value; }
		}

		protected void Copy( AssociatedContent copy ) {
			Artist = copy.Artist;
			Album = copy.Album;
			IsContentAvailable = copy.IsContentAvailable;
		}
	}
}
