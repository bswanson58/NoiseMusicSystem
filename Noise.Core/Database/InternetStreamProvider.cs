using System;
using System.Collections.Generic;
using CuttingEdge.Conditions;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Database {
	internal class InternetStreamProvider : BaseDataProvider<DbInternetStream>, IInternetStreamProvider {
		public InternetStreamProvider( IDatabaseManager databaseManager ) :
			base( databaseManager ) { }

		public void AddStream( DbInternetStream stream ) {
			Condition.Requires( stream ).IsNotNull();

			try {
				InsertItem( stream );
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "AddStream", ex );
			}
		}

		public void DeleteStream( DbInternetStream stream ) {
			Condition.Requires( stream ).IsNotNull();

			try {
				DeleteItem( stream );
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "DeleteStream", ex );
			}
		}

		public DbInternetStream GetStream( long streamId ) {
			return( TryGetItem( "SELECT DbInternetStream Where DbId = @itemId", new Dictionary<string, object> {{ "itemId", streamId }}, "GetStream" ));
		}

		public DataProviderList<DbInternetStream> GetStreamList() {
			return( TryGetList( "SELECT DbInternetStream", "GetStreamList" ));
		}

		public DataUpdateShell<DbInternetStream> GetStreamForUpdate( long streamId ) {
			return( GetUpdateShell( "SELECT DbInternetStream Where DbId = @streamId", new Dictionary<string, object> {{ "streamId", streamId }} ));
		}
	}
}
