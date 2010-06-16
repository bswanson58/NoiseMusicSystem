namespace Noise.Core.FileStore {
	public class StorageFile {
		public string	Name { get; private set; }
		public long		ParentFolder { get; private set; }

		public StorageFile( string name, long parentFolder ) {
			Name = name;
			ParentFolder = parentFolder;
		}
	}
}
