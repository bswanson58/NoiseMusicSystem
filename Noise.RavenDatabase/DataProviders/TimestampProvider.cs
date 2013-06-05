﻿using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.RavenDatabase.Interfaces;
using Noise.RavenDatabase.Support;

namespace Noise.RavenDatabase.DataProviders {
	public class TimestampProvider : BaseProvider<DbTimestamp>, ITimestampProvider {
		public TimestampProvider( IDbFactory databaseFactory ) :
			base( databaseFactory, entity => new object[] { entity.ComponentId }) {
		}

		public long GetTimestamp( string componentId ) {
			var retValue = 0L;
			var timestamp = Database.Get( componentId );

			if( timestamp != null ) {
				retValue = timestamp.Timestamp;
			}

			return( retValue );
		}

		public void SetTimestamp( string componentId, long ticks ) {
			var timeStamp = Database.Get( componentId );

			if( timeStamp != null ) {
				timeStamp.SetTimestamp( ticks );

				Database.Update( timeStamp );
			}
			else {
				timeStamp = new DbTimestamp( componentId );

				timeStamp.SetTimestamp( ticks );
				Database.Add( timeStamp );
			}
		}
	}
}