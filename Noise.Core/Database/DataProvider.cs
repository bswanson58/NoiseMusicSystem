using System;
using System.Collections.Generic;
using Noise.Core.Support;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Database {
	public class DataProvider : IDataProvider, IRequireConstruction {
		private readonly IDatabaseManager	mDatabaseManager;
		private long						mDatabaseId;

		public DataProvider( IDatabaseManager databaseManager ) {
			mDatabaseManager = databaseManager;
			mDatabaseId = Constants.cDatabaseNullOid;

			NoiseLogger.Current.LogInfo( "DataProvider created" );
		}

		public long DatabaseId {
			get {
				if( mDatabaseId == Constants.cDatabaseNullOid ) {
					IDatabase database = null;

					try {
						database = mDatabaseManager.ReserveDatabase();

						if( database != null ) {
							mDatabaseId = database.DatabaseVersion.DatabaseId;
						}
					}
					catch( Exception ex ) {
						NoiseLogger.Current.LogException( "Exception - Could not access database id.", ex );
					}
					finally {
						if( database != null ) {
							mDatabaseManager.FreeDatabase( database );
						}
					}
				}
				return( mDatabaseId );
			}
		}

		public DbBase GetItem( long itemId ) {
			DbBase	retValue;

			using( var database = mDatabaseManager.CreateDatabase()) {
				retValue = database.Database.QueryForItem( "SELECT DbBase Where DbId = @itemId", new Dictionary<string, object> {{ "itemId", itemId }} ) as DbBase;
			}

			return( retValue );
		}
	}
}
