﻿using System.Diagnostics;

namespace Noise.Infrastructure.Dto {
	[DebuggerDisplay("Folder = {Name}")]
	public class StorageFolder : DbBase {
		public string	Name { get; set; }
		public long		ParentFolder { get; set; }
		public bool		IsDeleted { get; set; }

		protected StorageFolder() :
			this( string.Empty, Constants.cDatabaseNullOid ) { }

		public StorageFolder( string name, long parentFolder ) {
			Name = name;
			ParentFolder = parentFolder;
			IsDeleted = false;
		}

		protected StorageFolder( long dbId, string path ) :
			base( dbId ) {
			Name = path;
			ParentFolder = Constants.cDatabaseNullOid;
		}

		public override string ToString() {
			var isDeleted = IsDeleted ? " (Marked for deletion)" : string.Empty;

			return( string.Format( "Folder \"{0}\", Id:{1}{2}", Name, DbId, isDeleted ));
		}
	}
}
