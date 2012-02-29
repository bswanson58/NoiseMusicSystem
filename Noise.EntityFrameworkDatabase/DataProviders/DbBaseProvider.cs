using Noise.EntityFrameworkDatabase.Interfaces;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.DataProviders {
	public class DbBaseProvider : IDbBaseProvider {
		private readonly IContextProvider	mContextProvider;

		public DbBaseProvider( IContextProvider contextProvider ) {
			mContextProvider = contextProvider;	
		}

		public DbBase GetItem( long itemId ) {
			DbBase	retValue = null;

			using( var context = mContextProvider.CreateContext()) {
				retValue = context.Set<DbArtist>().Find( itemId );

				if( retValue == null ) {
					retValue = context.Set<DbAlbum>().Find( itemId );
				}

				if( retValue == null ) {
					retValue = context.Set<DbTrack>().Find( itemId );
				}

				if( retValue == null ) {
					retValue = context.Set<DbPlayList>().Find( itemId );
				}

				if( retValue == null ) {
					retValue = context.Set<DbArtwork>().Find( itemId );
				}

				if( retValue == null ) {
					retValue = context.Set<DbTextInfo>().Find( itemId );
				}
			}

			return( retValue );
		}
	}
}
