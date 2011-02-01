using System;
using System.ComponentModel.Composition;
using System.Diagnostics;

namespace Noise.Infrastructure.Dto {
	public enum eTagGroup {
		Genre,
		Decade,
		User,
		Unknown
	}

	[DebuggerDisplay("Tag = {Name}")]
	public class DbTag : DbBase {
		public	eTagGroup		TagGroup { get; private set; }
		public	string			Name { get; private set; }
		public	string			Description { get; set; }
		public Int16			Rating { get; set; }
		public bool				IsFavorite { get; set; }

		public DbTag( eTagGroup group, string name ) {
			TagGroup = group;
			Name = name;
		}

		[Export("PersistenceType")]
		public static Type PersistenceType {
			get{ return( typeof( DbTag )); }
		}
	}
}
