using System.Collections.Generic;
using Noise.EloqueraDatabase.Interfaces;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.EloqueraDatabase.DataProviders {
	internal class DbBaseProvider : BaseDataProvider<DbBase>, IDbBaseProvider {
		public DbBaseProvider( IEloqueraManager databaseManager ) :
			base( databaseManager ) { }

		public DbBase GetItem( long itemId ) {
			return( TryGetItem( "SELECT DbBase Where DbId = @itemId", new Dictionary<string, object> {{ "itemId", itemId }}, "GetItem" ));
		}
	}
}
