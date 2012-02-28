﻿using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using Eloquera.Client;

namespace Noise.Infrastructure.Dto {
	public enum eTagGroup {
		Genre,
		Decade,
		User,
		Unknown
	}

	[DebuggerDisplay("Tag = {Name}")]
	public class DbTag : DbBase {
		public	eTagGroup		TagGroup { get; protected set; }
		public	string			Name { get; protected set; }
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

		[Ignore]
		public int DbTagGroup {
			get{ return((int)TagGroup ); }
			protected set{ TagGroup = (eTagGroup)value; }
		}

		[Export("PersistenceType")]
		public static Type PersistenceType {
			get{ return( typeof( DbTag )); }
		}
	}
}
