namespace Noise.Core.FileStore {
	public class StorageFolder {
		public string	Name { get; set; }
		public long		ParentFolder { get; set; }

		public StorageFolder( string name, long parentFolder ) {
			Name = name;
			ParentFolder = parentFolder;
		}
	}
}
