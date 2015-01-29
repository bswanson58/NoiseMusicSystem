namespace Noise.Infrastructure.Dto {
	public class DbTextInfo : AssociatedContent {
		public	long			FolderLocation { get; set; }
		public	InfoSource		Source { get; set; }
		public	string			Name { get; set; }

		protected DbTextInfo() :
			this( Constants.cDatabaseNullOid, ContentType.Unknown) { }

		protected DbTextInfo( DbTextInfo clone ) :
			base( clone ) {
			FolderLocation = clone.FolderLocation;
			Source = clone.Source;
			Name = clone.Name;
		}

		public DbTextInfo( long associatedItem, ContentType contentType ) :
		base( associatedItem, contentType ) {
			FolderLocation = Constants.cDatabaseNullOid;
			Source = InfoSource.Unknown;
			Name = string.Empty;
		}

		public void Copy( DbTextInfo copy ) {
			base.Copy( copy );

			FolderLocation = copy.FolderLocation;
			Source = copy.Source;
			Name = copy.Name;
		}

		public override string ToString() {
			return( string.Format( "TextInfo \"{0}\", Id:{1}, Artist:{2}, Album:{3}, Associated Item:{4}, Type:{5}", Name, DbId, Artist, Album, AssociatedItem, ContentType ));
		}
	}
}
