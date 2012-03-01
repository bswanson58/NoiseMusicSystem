using System;
using System.ComponentModel.Composition;

namespace Noise.Infrastructure.Dto {
	public enum DiscographyReleaseType {
		Unknown,
		Release,
		Appearance,
		TrackAppearance,
		Other
	}

	public class DbDiscographyRelease : ExpiringContent {
		public string					Title { get; protected set; }
		public string					Format { get; protected set; }
		public string					Label { get; protected set; }
		public int						Year { get; protected set; }
		public DiscographyReleaseType	ReleaseType { get; protected set; }

		protected DbDiscographyRelease() :
			this( Constants.cDatabaseNullOid, string.Empty, string.Empty, string.Empty, Constants.cUnknownYear, DiscographyReleaseType.Unknown ) { }

		public DbDiscographyRelease( long associatedItem, string title, string format, string label, int year, DiscographyReleaseType releaseType ) :
			base( associatedItem, ContentType.Discography ) {
			Artist = associatedItem;
			Title = title;
			Format = format;
			Label = label;
			ReleaseType = releaseType;
			Year = year;
		}

		public int DbDiscographyReleaseType {
			get{ return((int)ReleaseType ); }
			protected set{ ReleaseType = (DiscographyReleaseType)value; }
		}

		[Export("PersistenceType")]
		public static Type PersistenceType {
			get{ return( typeof( DbDiscographyRelease )); }
		}
	}
}

