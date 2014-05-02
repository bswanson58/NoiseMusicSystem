using System;
using System.Diagnostics;

namespace Noise.Infrastructure.Dto {
	[DebuggerDisplay("File = {Name}")]
	public class StorageFile : DbBase {
		public string		Name { get; protected set; }
		public long			ParentFolder { get; protected set; }
		public long			FileSize { get; protected set; }
		public long			FileModifiedTicks { get; protected set; }
		public eFileType	FileType { get; set; }
		public long			MetaDataPointer { get; set; }
		public bool			IsDeleted { get; set; }
		public bool			WasUpdated { get; set; }

		public StorageFile() :
			this( string.Empty, Constants.cDatabaseNullOid, 0L, DateTime.Now ) { }

		public StorageFile( string name, long parentFolder, long fileSize, DateTime modifiedDate ) {
			Name = name;
			ParentFolder = parentFolder;
			FileSize = fileSize;
			FileModifiedTicks = modifiedDate.Ticks;

			FileType = eFileType.Undetermined;
			MetaDataPointer = Constants.cDatabaseNullOid;
			IsDeleted = false;
			WasUpdated = false;
		}

		public void UpdateModifiedDate( DateTime fileModificationDate ) {
			FileModifiedTicks = fileModificationDate.Ticks;
			WasUpdated = true;
		}
	}
}
