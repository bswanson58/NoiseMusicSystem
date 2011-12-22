using System.Collections.Generic;
using CuttingEdge.Conditions;
using Noise.Core.FileStore;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Database {
	internal class StorageFileProvider : BaseDataProvider<StorageFile>, IStorageFileProvider {
		public StorageFileProvider( IDatabaseManager databaseManager ) :
			base( databaseManager ) { }

		public StorageFile GetPhysicalFile( DbTrack forTrack ) {
			Condition.Requires( forTrack ).IsNotNull();

			return( TryGetItem( "SELECT StorageFile Where MetaDataPointer = @trackId", new Dictionary<string, object>{{ "trackId", forTrack.DbId }}, "GetPhysicalFile" ));
		}

		public string GetPhysicalFilePath( StorageFile forFile ) {
			string	retValue;

			using( var dbShell = GetDatabase ) {
				retValue = StorageHelpers.GetPath( dbShell.Database.Database, forFile );
			}

			return( retValue );
		}
	}
}
