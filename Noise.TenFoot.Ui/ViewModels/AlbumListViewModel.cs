using Caliburn.Micro;
using Noise.TenFoot.Ui.Interfaces;
using ReusableBits.Mvvm.CaliburnSupport;

namespace Noise.TenFoot.Ui.ViewModels {
	public class AlbumListViewModel : Screen, IAlbumList {
		private readonly IAlbumTrackList	mAlbumTrackList;

		public AlbumListViewModel( IAlbumTrackList trackListViewModel ) {
			mAlbumTrackList = trackListViewModel;
		}

		public void Tracks() {
			if( Parent is INavigate ) {
				var controller = Parent as INavigate;

				controller.NavigateTo( mAlbumTrackList );
			}
		}

		public void Home() {
			if( Parent is INavigate ) {
				var controller = Parent as INavigate;

				controller.NavigateHome();
			}
		}

		public void Done() {
			if( Parent is INavigate ) {
				var controller = Parent as INavigate;

				controller.NavigateReturn( this, true );
			}
		}
	}
}
