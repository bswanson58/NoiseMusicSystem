using System;
using System.Collections.Generic;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Database {
	internal class DbBaseProvider : BaseDataProvider<DbBase>, IDbBaseProvider {
		public DbBaseProvider( IDatabaseManager databaseManager ) :
			base( databaseManager ) { }

		public DbBase GetItem( long itemId ) {
			return( TryGetItem( "SELECT DbBase Where DbId = @itemId", new Dictionary<string, object> {{ "itemId", itemId }}, "GetItem" ));
		}

		public long DatabaseInstanceId() {
			var retValue = 0l;

			try {
				using( var dbShell = CreateDatabase()) {
					retValue = dbShell.Database.DatabaseVersion.DatabaseId;
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "DatabaseInstanceId", ex );
			}

			return( retValue );
		}
	}
}
