using System.Collections.Generic;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Database {
	internal class DbBaseProvider : BaseDataProvider<DbBase>, IDbBaseProvider {
		public DbBaseProvider( IDatabaseManager databaseManager ) :
			base( databaseManager ) { }

		public DbBase GetItem( long itemId ) {
			return( TryGetItem( "SELECT DbBase Where DbId = @itemId", new Dictionary<string, object> {{ "itemID", itemId }}, "GetItem" ));
		}
	}
}
