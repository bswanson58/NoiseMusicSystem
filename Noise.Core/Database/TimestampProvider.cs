using System.Collections.Generic;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Database {
	internal class TimestampProvider : BaseDataProvider<DbTimestamp>, ITimestampProvider {
		public TimestampProvider( IDatabaseManager databaseManager ) :
			base( databaseManager ) { }

		public long GetTimestamp( string componentId ) {
			var	retValue = 0L;
			var	timestamp = TryGetItem( "SELECT DbTimestamp WHERE ComponentId = @id", new Dictionary<string, object> {{ "id", componentId }}, "GetTimestamp" );

			if( timestamp != null ) {
				retValue = timestamp.Timestamp;
			}

			return( retValue );
		}

		public void SetTimestamp( string componentId, long ticks ) {
			var	timestamp = TryGetItem( "SELECT DbTimestamp WHERE ComponentId = @id", new Dictionary<string, object> {{ "id", componentId }}, "GetTimestamp" );

			if( timestamp != null ) {
				timestamp.SetTimestamp( ticks );

				UpdateItem( timestamp );
			}
			else {
				timestamp = new DbTimestamp( componentId );

				timestamp.SetTimestamp( ticks );
				InsertItem( timestamp );
			}
		}
	}
}
