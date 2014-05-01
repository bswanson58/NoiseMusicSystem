using System;

namespace Noise.Infrastructure.Dto {
	public class DbDecadeTag : DbTag {
		public	Int32		StartYear { get; set; }
		public	Int32		EndYear { get; set; }
		public	string		Website { get; set; }

		protected DbDecadeTag() :
			this( string.Empty ) { }

		public DbDecadeTag( string name ) :
			base( eTagGroup.Decade, name ) {
			Website = string.Empty;
		}
	}
}
