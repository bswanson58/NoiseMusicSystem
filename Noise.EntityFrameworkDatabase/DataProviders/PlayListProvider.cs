using Caliburn.Micro;
using Noise.EntityFrameworkDatabase.Interfaces;
using Noise.EntityFrameworkDatabase.Logging;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.DataProviders {
	internal class PlayListProvider : BaseProvider<DbPlayList>, IPlayListProvider {
		private readonly IEventAggregator	mEventAggregator;

		public PlayListProvider( IEventAggregator eventAggregator, IContextProvider contextProvider, ILogDatabase log ) :
			base( contextProvider, log ) {
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
			mEventAggregator.PublishOnUIThread( new Events.PlayListChanged( playList ));
		}
	}
}
