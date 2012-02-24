using System.Collections.Generic;
using Caliburn.Micro;
using CuttingEdge.Conditions;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.EloqueraDatabase.DataProviders {
	internal class PlayListProvider : BaseDataProvider<DbPlayList>, IPlayListProvider {
		private readonly IEventAggregator	mEventAggregator;

		public PlayListProvider( IEventAggregator eventAggregator, IDatabaseManager databaseManager ) :
			base( databaseManager ) {
			mEventAggregator = eventAggregator;
		}

		public void AddPlayList( DbPlayList playList ) {
			Condition.Requires( playList ).IsNotNull();

			InsertItem( playList );

			FireListChanged( playList );
		}

		public void DeletePlayList( DbPlayList playList ) {
			Condition.Requires( playList ).IsNotNull();

			DeleteItem( playList );

			FireListChanged( playList );
		}

		public DbPlayList GetPlayList( long playListId ) {
			return( TryGetItem( "SELECT DbPlayList Where DbId = @playListId", new Dictionary<string, object> {{ "playListId", playListId }}, "GetPlayList" ));
		}

		public IDataProviderList<DbPlayList> GetPlayLists() {
			return( TryGetList( "SELECT DbPlayList", "GetPlayLists" ));
		}

		public IDataUpdateShell<DbPlayList> GetPlayListForUpdate( long playListId ) {
			return( GetUpdateShell( "SELECT DbPlayList Where DbId = @playListId", new Dictionary<string, object> {{ "playListId", playListId }} ));
		}

		private void FireListChanged( DbPlayList playList ) {
			mEventAggregator.Publish( new Events.PlayListChanged( playList ));
		}
	}
}
