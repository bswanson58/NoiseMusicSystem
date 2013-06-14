using System.Collections.Generic;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Support;

namespace Noise.UI.ViewModels {
	public class SelectPlayListDialogModel : DialogModelBase {
		private readonly List<DbPlayList>	mPlayLists;
		private	DbPlayList					mSelectedItem;

		public IPlayStrategyParameters		Parameters { get; private set; }

		public SelectPlayListDialogModel( IPlayListProvider playListProvider ) {
			using( var playLists = playListProvider.GetPlayLists()) {
				mPlayLists = new List<DbPlayList>( playLists.List );
			}
		}

		public IEnumerable<DbPlayList> PlayLists {
			get{ return( mPlayLists ); }
		}

		public DbPlayList SelectedItem {
			get { return ( mSelectedItem ); }
			set {
				mSelectedItem = value;

				if( mSelectedItem != null ) {
					Parameters = new PlayStrategyParameterDbId( ePlayExhaustedStrategy.PlayList ) { DbItemId = mSelectedItem.DbId };
				}
			}
		}
	}
}
