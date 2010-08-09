using System;
using System.ComponentModel.Composition;

namespace Noise.Infrastructure.Dto {
	public class DbDiscographyRelease : ExpiringContent {
		public string	Title { get; private set; }
		public string	Format { get; private set; }
		public string	Label { get; private set; }
		public uint		Year { get; private set; }

		public DbDiscographyRelease( long associatedItem, string title, string format, string label, uint year ) :
			base( associatedItem, ContentType.Discography ) {
			Title = title;
			Format = format;
			Label = label;
			Year = year;
		}

		[Export("PersistenceType")]
		public static Type PersistenceType {
			get{ return( typeof( DbDiscographyRelease )); }
		}
	}
}

