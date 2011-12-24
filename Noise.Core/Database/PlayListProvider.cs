using System.Collections.Generic;
using CuttingEdge.Conditions;
using Microsoft.Practices.Prism.Events;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Database {
	internal class PlayListProvider : BaseDataProvider<DbPlayList>, IPlayListProvider {
		private readonly IEventAggregator	mEvents;

		public PlayListProvider( IEventAggregator eventAggregator, IDatabaseManager databaseManager ) :
			base( databaseManager ) {
			mEvents = eventAggregator;
		}

		public void AddPlayList( DbPlayList playList ) {
			Condition.Requires( playList ).IsNotNull();

			using( var dbShell = CreateDatabase()) {
				dbShell.InsertItem( playList );
			}

			FireListChanged( playList );
		}

		public void DeletePlayList( DbPlayList playList ) {
			Condition.Requires( playList ).IsNotNull();

			using( var dbShell = CreateDatabase()) {
				dbShell.DeleteItem( playList );
			}

			FireListChanged( playList );
		}

		public DbPlayList GetPlayList( long playListId ) {
			return( TryGetItem( "SELECT DbPlayList Where DbId = @playListId", new Dictionary<string, object> {{ "playListId", playListId }}, "GetPlayList" ));
		}

		public DataProviderList<DbPlayList> GetPlayLists() {
			return( TryGetList( "SELECT DbPlayList", "GetPlayLists" ));
		}

		private void FireListChanged( DbPlayList playList ) {
			mEvents.GetEvent<Events.PlayListChanged>().Publish( playList );
		}
	}
}
