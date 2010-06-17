using CuttingEdge.Conditions;
using Noise.Core.Database;

namespace Noise.Core.FileStore {
	public class RootFolder : StorageFolder {
		public string	DisplayName { get; set; }

		public RootFolder( string path, string displayName ) :
			base( path, DatabaseManager.cNullOid ) {
			Condition.Requires( path ).IsNotNullOrEmpty();

			DisplayName = displayName;
		}
	}
}
