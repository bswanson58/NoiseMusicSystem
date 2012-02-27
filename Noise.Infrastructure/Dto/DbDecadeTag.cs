using System;
using System.ComponentModel.Composition;

namespace Noise.Infrastructure.Dto {
	public class DbDecadeTag : DbTag {
		public	UInt32		StartYear { get; set; }
		public	UInt32		EndYear { get; set; }
		public	string		Website { get; set; }

		protected DbDecadeTag() :
			this( string.Empty ) { }

		public DbDecadeTag( string name ) :
			base( eTagGroup.Decade, name ) {
			Website = string.Empty;
		}

		[Export("PersistenceType")]
		public static new Type PersistenceType {
			get{ return( typeof( DbDecadeTag )); }
		}
	}
}
