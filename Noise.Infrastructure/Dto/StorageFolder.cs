using System;
using System.ComponentModel.Composition;

namespace Noise.Infrastructure.Dto {
	public class StorageFolder : DbBase {
		public string	Name { get; set; }
		public long		ParentFolder { get; set; }

		public StorageFolder( string name, long parentFolder ) {
			Name = name;
			ParentFolder = parentFolder;
		}

		[Export("PersistenceType")]
		public static Type PersistenceType {
			get{ return( typeof( StorageFolder )); }
		}
	}
}
