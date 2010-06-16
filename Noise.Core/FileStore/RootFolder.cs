using CuttingEdge.Conditions;

namespace Noise.Core.FileStore {
	public class RootFolder : StorageFolder {
		public string	DisplayName { get; set; }

		public RootFolder( string path, string displayName ) :
			base( path, StorageHelpers.cNullOid ) {
			Condition.Requires( path ).IsNotNullOrEmpty();

			DisplayName = displayName;
		}
	}
}
