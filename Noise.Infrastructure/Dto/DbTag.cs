using System;
using System.Diagnostics;

namespace Noise.Infrastructure.Dto {
	public enum eTagGroup {
		Genre,
		Decade,
		User,
		Unknown
	}

	[DebuggerDisplay("Tag = {" + nameof( Name ) + "}")]
	public class DbTag : DbBase {
		public	eTagGroup		TagGroup { get; protected set; }
		public	string			Name { get; set; }
		public	string			Description { get; set; }
		public	Int16			Rating { get; set; }
		public	bool			IsFavorite { get; set; }

		protected DbTag() :
			this( eTagGroup.Unknown, string.Empty ) { }

		public DbTag( eTagGroup group, string name ) {
			TagGroup = group;
			Name = name;
			Description = string.Empty;
		}

        public void UpdateFrom( DbTag tag ) {
            Name = tag.Name;
            Description = tag.Description;
            Rating = tag.Rating;
            IsFavorite = tag.IsFavorite;
        }

		public override string ToString() {
			return( $"Tag \"{Name}\", Group:{TagGroup}" );
		}
	}
}
