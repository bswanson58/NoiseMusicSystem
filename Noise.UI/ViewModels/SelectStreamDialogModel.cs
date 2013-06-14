using System.Collections.Generic;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Support;

namespace Noise.UI.ViewModels {
	public class SelectStreamDialogModel : DialogModelBase {
		private readonly List<DbInternetStream>	mStreamList;
		private	DbInternetStream				mSelectedItem;

		public IPlayStrategyParameters			Parameters { get; private set; }

		public SelectStreamDialogModel( IInternetStreamProvider streamProvider ) {
			using( var streamList = streamProvider.GetStreamList()) {
				mStreamList = new List<DbInternetStream>( streamList.List );
			}
		}

		public IEnumerable<DbInternetStream> StationList {
			get{ return( mStreamList ); }
		}

		public DbInternetStream SelectedItem {
			get { return ( mSelectedItem ); }
			set {
				mSelectedItem = value;

				if( mSelectedItem != null ) {
					Parameters = new PlayStrategyParameterDbId( ePlayExhaustedStrategy.PlayStream ) { DbItemId = mSelectedItem.DbId };
				}
			}
		}
	}
}
