using System;
using System.ComponentModel.Composition;
using System.Diagnostics;

namespace Noise.Infrastructure.Dto {
	[DebuggerDisplay("File = {Name}")]
	public class StorageFile : DbBase {
		public string		Name { get; private set; }
		public long			ParentFolder { get; private set; }
		public long			FileSize { get; private set; }
		public DateTime		FileModifiedDate { get; private set; }
		public eFileType	FileType { get; set; }
		public long			MetaDataPointer { get; set; }
		public bool			IsDeleted { get; set; }

		public StorageFile( string name, long parentFolder, long fileSize, DateTime modifiedDate ) {
			Name = name;
			ParentFolder = parentFolder;
			FileSize = fileSize;
			FileModifiedDate = modifiedDate;

			FileType = eFileType.Undetermined;
			MetaDataPointer = Constants.cDatabaseNullOid;
			IsDeleted = false;
		}

		[Export("PersistenceType")]
		public static Type PersistenceType {
			get{ return( typeof( StorageFile )); }
		}
	}
}
