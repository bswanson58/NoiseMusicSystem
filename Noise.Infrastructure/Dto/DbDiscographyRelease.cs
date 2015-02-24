namespace Noise.Infrastructure.Dto {
	public enum DiscographyReleaseType {
		Unknown,
		Release,
		Appearance,
		TrackAppearance,
		Other
	}

	public class DbDiscographyRelease {
		public string					Title { get; protected set; }
		public string					Format { get; protected set; }
		public string					Label { get; protected set; }
		public int						Year { get; protected set; }
		public DiscographyReleaseType	ReleaseType { get; protected set; }

		protected DbDiscographyRelease() :
			this( string.Empty, string.Empty, string.Empty, Constants.cUnknownYear, DiscographyReleaseType.Unknown ) { }

		public DbDiscographyRelease( string title, string format, string label, int year, DiscographyReleaseType releaseType ) {
			Title = title;
			Format = format;
			Label = label;
			ReleaseType = releaseType;
			Year = year;
		}

		public override string ToString() {
			return( string.Format( "Discography Title\"{0}\", Year:{1}", Title, Year ));
		}
	}
}

