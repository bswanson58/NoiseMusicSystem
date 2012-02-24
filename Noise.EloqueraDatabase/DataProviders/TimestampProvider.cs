using System.Collections.Generic;
using Noise.EloqueraDatabase.Interfaces;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.EloqueraDatabase.DataProviders {
	internal class TimestampProvider : BaseDataProvider<DbTimestamp>, ITimestampProvider {
		public TimestampProvider( IEloqueraManager databaseManager ) :
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
			using( var updater = GetUpdateShell( "SELECT DbTimestamp WHERE ComponentId = @id", new Dictionary<string, object> {{ "id", componentId }} )) {
				if( updater.Item != null ) {
					updater.Item.SetTimestamp( ticks );

					updater.Update();
				}
				else {
					var timestamp = new DbTimestamp( componentId );

					timestamp.SetTimestamp( ticks );
					InsertItem( timestamp );
				}
			}
		}
	}
}
