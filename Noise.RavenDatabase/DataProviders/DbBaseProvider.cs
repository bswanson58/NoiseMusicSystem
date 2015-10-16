using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.RavenDatabase.Interfaces;
using Noise.RavenDatabase.Logging;
using Noise.RavenDatabase.Support;

namespace Noise.RavenDatabase.DataProviders {
	internal class DbBaseProvider : IDbBaseProvider {
		private readonly IDbFactory				mDbFactory;
		private readonly ILogRaven				mLog;

		public DbBaseProvider( IDbFactory databaseFactory, ILogRaven log ) {
			mDbFactory = databaseFactory;
			mLog = log;
		}

		public DbBase GetItem( long itemId ) {
			DbBase	retValue;

			{
				var database = new RavenRepository<DbArtist>( mDbFactory.GetLibraryDatabase(), entity => new object[] { entity.DbId }, mLog );

				retValue = database.Get( itemId );
			}

			if( retValue == null ) {
				var database = new RavenRepository<DbAlbum>( mDbFactory.GetLibraryDatabase(), entity => new object[] { entity.DbId }, mLog );

				retValue = database.Get( itemId );
			}

			if( retValue == null ) {
				var database = new RavenRepository<DbTrack>( mDbFactory.GetLibraryDatabase(), entity => new object[] { entity.DbId }, mLog );

				retValue = database.Get( itemId );
			}

			if( retValue == null ) {
				var database = new RavenRepository<DbPlayList>( mDbFactory.GetLibraryDatabase(), entity => new object[] { entity.DbId }, mLog );

				retValue = database.Get( itemId );
			}

			if( retValue == null ) {
				var database = new RavenRepository<DbArtwork>( mDbFactory.GetLibraryDatabase(), entity => new object[] { entity.DbId }, mLog );

				retValue = database.Get( itemId );
			}

			if( retValue == null ) {
				var database = new RavenRepository<DbTextInfo>( mDbFactory.GetLibraryDatabase(), entity => new object[] { entity.DbId }, mLog );

				retValue = database.Get( itemId );
			}

			return ( retValue );
		}
	}
}
