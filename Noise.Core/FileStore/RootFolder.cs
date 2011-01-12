using System;
using System.ComponentModel.Composition;
using CuttingEdge.Conditions;
using Noise.Infrastructure.Dto;

namespace Noise.Core.FileStore {
	public class RootFolder : StorageFolder {
		public	string			DisplayName { get; set; }
		public	FolderStrategy	FolderStrategy { get; set; }
		public	long			LastLibraryScan { get; private set; }
		public	long			LastSummaryScan { get; private set; }
		public	long			LastCloudSequenceId { get; set; }

		public RootFolder( long dbId, string path, string displayName ) :
			base( dbId, path ) {
			Condition.Requires( path ).IsNotNullOrEmpty();

			DisplayName = displayName;
			FolderStrategy = new FolderStrategy();
		}

		public void UpdateLibraryScan() {
			LastLibraryScan = DateTime.Now.Ticks;
		}

		public void UpdateSummaryScan() {
			LastSummaryScan = DateTime.Now.Ticks;
		}

		[Export("PersistenceType")]
		public static new Type PersistenceType {
			get{ return( typeof( RootFolder )); }
		}
	}
}
