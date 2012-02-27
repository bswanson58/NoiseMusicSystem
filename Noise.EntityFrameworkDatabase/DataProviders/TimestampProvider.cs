using System.Linq;
using Noise.EntityFrameworkDatabase.Interfaces;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.DataProviders {
	public class TimestampProvider : ITimestampProvider {
		private readonly IContextProvider	mContextProvider;

		public TimestampProvider( IContextProvider contextProvider ) {
			mContextProvider = contextProvider;
		}

		public long GetTimestamp( string componentId ) {
			var retValue = 0L;

			using( var context = mContextProvider.CreateContext()) {
				var timestamp = context.Set<DbTimestamp>().FirstOrDefault( entry => entry.ComponentId.Equals(  componentId ));

				if( timestamp != null ) {
					retValue = timestamp.Timestamp;
				}
			}

			return( retValue );
		}

		public void SetTimestamp( string componentId, long ticks ) {
			using( var context = mContextProvider.CreateContext()) {
				var timestamp = context.Set<DbTimestamp>().FirstOrDefault( entry => entry.ComponentId.Equals(  componentId ));

				if( timestamp == null ) {
					timestamp = new DbTimestamp( componentId );

					context.Set<DbTimestamp>().Add( timestamp );
				}

				timestamp.SetTimestamp( ticks );

				context.SaveChanges();
			}
		}
	}
}
