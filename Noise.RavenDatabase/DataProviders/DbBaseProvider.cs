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
				var database = new RavenRepositoryT<DbArtist>( mDbFactory.GetLibraryDatabase(), entity => new object[] { entity.DbId } );

				retValue = database.Get( itemId );
			}

			if( retValue == null ) {
				var database = new RavenRepositoryT<DbAlbum>( mDbFactory.GetLibraryDatabase(), entity => new object[] { entity.DbId } );

				retValue = database.Get( itemId );
			}

			if( retValue == null ) {
				var database = new RavenRepositoryT<DbTrack>( mDbFactory.GetLibraryDatabase(), entity => new object[] { entity.DbId } );

				retValue = database.Get( itemId );
			}

			if( retValue == null ) {
				var database = new RavenRepositoryT<DbPlayList>( mDbFactory.GetLibraryDatabase(), entity => new object[] { entity.DbId } );

				retValue = database.Get( itemId );
			}

			if( retValue == null ) {
				var database = new RavenRepositoryT<DbArtwork>( mDbFactory.GetLibraryDatabase(), entity => new object[] { entity.DbId } );

				retValue = database.Get( itemId );
			}

			if( retValue == null ) {
				var database = new RavenRepositoryT<DbTextInfo>( mDbFactory.GetLibraryDatabase(), entity => new object[] { entity.DbId } );

				retValue = database.Get( itemId );
			}

			return ( retValue );
		}
	}
}
