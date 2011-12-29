using System.Collections.Generic;
using CuttingEdge.Conditions;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Database {
	internal class PlayHistoryProvider : BaseDataProvider<DbPlayHistory>, IPlayHistoryProvider {
		public PlayHistoryProvider( IDatabaseManager databaseManager ) :
			base( databaseManager ) { }

		public void AddPlayHistory( DbPlayHistory playHistory ) {
			Condition.Requires( playHistory ).IsNotNull();

			InsertItem( playHistory );
		}

		public void DeletePlayHistory( DbPlayHistory playHistory ) {
			Condition.Requires( playHistory ).IsNotNull();

			DeleteItem( playHistory );
		}

		public DataProviderList<DbPlayHistory> GetPlayHistoryList() {
			return( TryGetList( "SELECT DbPlayHistory", "GetPlayHistory" ));
		}

		public DataUpdateShell<DbPlayHistory> GetPlayHistoryForUpdate( long playListId ) {
			return( GetUpdateShell( "SELECT DbPlayHistory Where DbId = @playListId", new Dictionary<string, object> {{ "playListId", playListId }} ));
		}
	}
}
