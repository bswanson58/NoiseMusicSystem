using Caliburn.Micro;
using Noise.TenFoot.Ui.Interfaces;
using ReusableBits;
using ReusableBits.Mvvm.CaliburnSupport;

namespace Noise.TenFoot.Ui.ViewModels {
	public class AlbumListViewModel : Screen, IAlbumList {
		private readonly IAlbumTrackList	mAlbumTrackList;
		private TaskHandler					mAlbumRetrievalTaskHandler;

		public AlbumListViewModel( IAlbumTrackList trackListViewModel ) {
			mAlbumTrackList = trackListViewModel;
		}

		public void SetContext( long artistId ) {
			
		}

		internal TaskHandler AlbumRetrievalTaskHandler {
			get {
				if( mAlbumRetrievalTaskHandler == null ) {
					mAlbumRetrievalTaskHandler = new TaskHandler();
				}

				return( mAlbumRetrievalTaskHandler );
			}

			set{ mAlbumRetrievalTaskHandler = value; }
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
