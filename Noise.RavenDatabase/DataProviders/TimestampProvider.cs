using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.RavenDatabase.Interfaces;
using Noise.RavenDatabase.Support;

namespace Noise.RavenDatabase.DataProviders {
	public class TimestampProvider : ITimestampProvider {
		private readonly IDbFactory					mDbFactory;
		private readonly IRepository<DbTimestamp>	mDatabase;

		public TimestampProvider( IDbFactory databaseFactory ) {
			mDbFactory = databaseFactory;

			mDatabase = new RavenRepository<DbTimestamp>( mDbFactory.GetLibraryDatabase(), entity => new object[] { entity.ComponentId });
		}

		public long GetTimestamp( string componentId ) {
			var retValue = 0L;
			var timestamp = mDatabase.Get( componentId );

			if( timestamp != null ) {
				retValue = timestamp.Timestamp;
			}

			return( retValue );
		}

		public void SetTimestamp( string componentId, long ticks ) {
			var timeStamp = mDatabase.Get( componentId );

			if( timeStamp != null ) {
				timeStamp.SetTimestamp( ticks );

				mDatabase.Update( timeStamp );
			}
			else {
				timeStamp = new DbTimestamp( componentId );

				timeStamp.SetTimestamp( ticks );
				mDatabase.Add( timeStamp );
			}
		}
	}
}
