using Caliburn.Micro;
using Noise.EntityFrameworkDatabase.Interfaces;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.DataProviders {
	public class PlayListProvider : BaseProvider<DbPlayList>, IPlayListProvider {
		private readonly IEventAggregator	mEventAggregator;

		public PlayListProvider( IEventAggregator eventAggregator, IContextProvider contextProvider ) :
			base( contextProvider ) {
			mEventAggregator = eventAggregator;
		}

		public void AddPlayList( DbPlayList playList ) {
			AddItem( playList );

			FireListChanged( playList );
		}

		public void DeletePlayList( DbPlayList playList ) {
			RemoveItem( playList );

			FireListChanged( playList );
		}

		public DbPlayList GetPlayList( long playListId ) {
			return( GetItemByKey( playListId ));
		}

		public IDataProviderList<DbPlayList> GetPlayLists() {
			return( GetListShell());
		}

		public IDataUpdateShell<DbPlayList> GetPlayListForUpdate( long playListId ) {
			return( GetUpdateShell( playListId ));
		}

		private void FireListChanged( DbPlayList playList ) {
			mEventAggregator.Publish( new Events.PlayListChanged( playList ));
		}
	}
}
