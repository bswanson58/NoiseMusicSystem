using System;
using System.ComponentModel.Composition;
using CuttingEdge.Conditions;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;

namespace Noise.Core.FileStore {
	public class RootFolder : StorageFolder {
		public string			DisplayName { get; set; }
		public FolderStrategy	FolderStrategy { get; set; }

		public RootFolder( string path, string displayName ) :
			base( path, Constants.cDatabaseNullOid ) {
			Condition.Requires( path ).IsNotNullOrEmpty();

			DisplayName = displayName;
			FolderStrategy = new FolderStrategy();
		}

		[Export("PersistenceType")]
		public static new Type PersistenceType {
			get{ return( typeof( RootFolder )); }
		}
	}
}
