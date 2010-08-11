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
		public string					Title { get; private set; }
		public string					Format { get; private set; }
		public string					Label { get; private set; }
		public uint						Year { get; private set; }
		public DiscographyReleaseType	ReleaseType { get; private set; }

		public DbDiscographyRelease( long associatedItem, string title, string format, string label, uint year, DiscographyReleaseType releaseType ) :
			base( associatedItem, ContentType.Discography ) {
			Title = title;
			Format = format;
			Label = label;
			ReleaseType = releaseType;
			Year = year;
		}

		[Export("PersistenceType")]
		public static Type PersistenceType {
			get{ return( typeof( DbDiscographyRelease )); }
		}
	}
}

