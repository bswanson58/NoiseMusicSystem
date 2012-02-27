using System;
using System.ComponentModel.Composition;
using System.Diagnostics;

namespace Noise.Infrastructure.Dto {
	[DebuggerDisplay("File = {Name}")]
	public class StorageFile : DbBase {
		public string		Name { get; protected set; }
		public long			ParentFolder { get; protected set; }
		public long			FileSize { get; protected set; }
		public DateTime		FileModifiedDate { get; protected set; }
		public eFileType	FileType { get; set; }
		public long			MetaDataPointer { get; set; }
		public bool			IsDeleted { get; set; }

		public StorageFile() :
			this( string.Empty, Constants.cDatabaseNullOid, 0L, DateTime.Now ) { }

		public StorageFile( string name, long parentFolder, long fileSize, DateTime modifiedDate ) {
			Name = name;
			ParentFolder = parentFolder;
			FileSize = fileSize;
			FileModifiedDate = modifiedDate;

			FileType = eFileType.Undetermined;
			MetaDataPointer = Constants.cDatabaseNullOid;
			IsDeleted = false;
		}

		public int DbFileType {
			get{ return((int)FileType ); }
			protected set{ FileType = (eFileType)value; }
		}

		[Export("PersistenceType")]
		public static Type PersistenceType {
			get{ return( typeof( StorageFile )); }
		}
	}
}
