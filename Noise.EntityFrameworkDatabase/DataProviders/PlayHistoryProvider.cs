﻿using Noise.EntityFrameworkDatabase.Interfaces;
using Noise.EntityFrameworkDatabase.Logging;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.DataProviders {
	internal class PlayHistoryProvider : BaseProvider<DbPlayHistory>, IPlayHistoryProvider {
		public PlayHistoryProvider( IContextProvider contextProvider, ILogDatabase log ) :
			base( contextProvider, log ) { }

		public void AddPlayHistory( DbPlayHistory playHistory ) {
			AddItem( playHistory );
		}

		public void DeletePlayHistory( DbPlayHistory playHistory ) {
			RemoveItem( playHistory );
		}

		public IDataProviderList<DbPlayHistory> GetPlayHistoryList() {
			return( GetListShell());
		}

		public IDataUpdateShell<DbPlayHistory> GetPlayHistoryForUpdate( long playListId ) {
			return( GetUpdateShell( playListId ));
		}
	}
}
