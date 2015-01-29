using System;

namespace Noise.Infrastructure.Dto {
	public class DbArtwork : AssociatedContent {
		public	long			FolderLocation { get; set; }
		public	InfoSource		Source { get; set; }
		public	bool			IsUserSelection { get; set; }
		public	string			Name { get; set; }
		public	Int16			Rotation { get; set; }

		protected DbArtwork() :
			this( Constants.cDatabaseNullOid, ContentType.Unknown) { }

		protected DbArtwork( DbArtwork clone ) :
			base( clone ) {
			FolderLocation = clone.FolderLocation;
			Source = clone.Source;
			IsUserSelection = clone.IsUserSelection;
			Name = clone.Name;
			Rotation = clone.Rotation;
		}

		public DbArtwork( long associatedItem, ContentType contentType ) :
			base( associatedItem, contentType ) {
			Source = InfoSource.Unknown;
			FolderLocation = Constants.cDatabaseNullOid;
			IsUserSelection = false;
			Name = string.Empty;
			Rotation = 0;
		}

		public void Copy( DbArtwork copy ) {
			base.Copy( copy );

			FolderLocation = copy.FolderLocation;
			Source = copy.Source;
			IsUserSelection = copy.IsUserSelection;
			Name = copy.Name;
			Rotation = copy.Rotation;
		}

		public override string ToString() {
			return( string.Format( "Artwork \"{0}\", Id:{1}, Artist:{2}, Album:{3}, Associated Item:{4}, Type:{5}", Name, DbId, Artist, Album, AssociatedItem, ContentType ));
		}
	}
}
