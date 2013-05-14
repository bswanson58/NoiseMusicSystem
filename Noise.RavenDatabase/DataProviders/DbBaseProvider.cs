using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.RavenDatabase.Interfaces;
using Noise.RavenDatabase.Support;

namespace Noise.RavenDatabase.DataProviders {
	public class DbBaseProvider : IDbBaseProvider {
		private readonly IDbFactory				mDbFactory;

		public DbBaseProvider( IDbFactory databaseFactory ) {
			mDbFactory = databaseFactory;
		}

		public DbBase GetItem( long itemId ) {
			DbBase	retValue;

			{
				var database = new RavenRepository<DbArtist>( mDbFactory.GetLibraryDatabase(), entity => new object[] { entity.DbId } );

				retValue = database.Get( itemId );
			}

			if( retValue == null ) {
				var database = new RavenRepository<DbAlbum>( mDbFactory.GetLibraryDatabase(), entity => new object[] { entity.DbId } );

				retValue = database.Get( itemId );
			}

			if( retValue == null ) {
				var database = new RavenRepository<DbTrack>( mDbFactory.GetLibraryDatabase(), entity => new object[] { entity.DbId } );

				retValue = database.Get( itemId );
			}

			if( retValue == null ) {
				var database = new RavenRepository<DbPlayList>( mDbFactory.GetLibraryDatabase(), entity => new object[] { entity.DbId } );

				retValue = database.Get( itemId );
			}

			if( retValue == null ) {
				var database = new RavenRepository<DbArtwork>( mDbFactory.GetLibraryDatabase(), entity => new object[] { entity.DbId } );

				retValue = database.Get( itemId );
			}

			if( retValue == null ) {
				var database = new RavenRepository<DbTextInfo>( mDbFactory.GetLibraryDatabase(), entity => new object[] { entity.DbId } );

				retValue = database.Get( itemId );
			}

			return ( retValue );
		}
	}
}
