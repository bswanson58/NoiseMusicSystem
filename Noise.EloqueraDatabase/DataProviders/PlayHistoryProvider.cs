using System.Collections.Generic;
using CuttingEdge.Conditions;
using Noise.EloqueraDatabase.Interfaces;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.EloqueraDatabase.DataProviders {
	internal class PlayHistoryProvider : BaseDataProvider<DbPlayHistory>, IPlayHistoryProvider {
		public PlayHistoryProvider( IEloqueraManager databaseManager ) :
			base( databaseManager ) { }

		public void AddPlayHistory( DbPlayHistory playHistory ) {
			Condition.Requires( playHistory ).IsNotNull();

			InsertItem( playHistory );
		}

		public void DeletePlayHistory( DbPlayHistory playHistory ) {
			Condition.Requires( playHistory ).IsNotNull();

			DeleteItem( playHistory );
		}

		public IDataProviderList<DbPlayHistory> GetPlayHistoryList() {
			return( TryGetList( "SELECT DbPlayHistory", "GetPlayHistory" ));
		}

		public IDataUpdateShell<DbPlayHistory> GetPlayHistoryForUpdate( long playListId ) {
			return( GetUpdateShell( "SELECT DbPlayHistory Where DbId = @playListId", new Dictionary<string, object> {{ "playListId", playListId }} ));
		}
	}
}
