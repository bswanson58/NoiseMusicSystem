﻿using System;
using System.Diagnostics;

namespace Noise.Infrastructure.Dto {
	[DebuggerDisplay("RootFolder = {DisplayName}")]
	public class RootFolder : StorageFolder {
		public	string			DisplayName { get; set; }
		public	FolderStrategy	FolderStrategy { get; set; }
		public	long			LastLibraryScan { get; protected set; }
		public	long			LastSummaryScan { get; protected set; }
		public	long			LastCloudSequenceId { get; set; }
		public	long			InitialScanCompleted { get; protected set; }

		protected RootFolder() :
			this( Constants.cDatabaseNullOid, string.Empty, string.Empty ) { }

		public RootFolder( long dbId, string path, string displayName ) :
			base( dbId, path ) {
			DisplayName = displayName;
			FolderStrategy = new FolderStrategy();
		}

		public void UpdateLibraryScan() {
			LastLibraryScan = DateTime.Now.Ticks;
		}

		public void UpdateSummaryScan() {
			LastSummaryScan = DateTime.Now.Ticks;

			if( InitialScanCompleted == 0 ) {
				InitialScanCompleted = LastSummaryScan;
			}
		}

		public override string ToString() {
			return( string.Format( "RootFolder \"{0}\", Id:{1}", DisplayName, DbId ));
		}
	}
}
