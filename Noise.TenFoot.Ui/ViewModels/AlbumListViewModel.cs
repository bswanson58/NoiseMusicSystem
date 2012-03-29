using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.TenFoot.Ui.Interfaces;
using ReusableBits;
using ReusableBits.Mvvm.CaliburnSupport;

namespace Noise.TenFoot.Ui.ViewModels {
	public class AlbumListViewModel : Screen, IAlbumList {
		private readonly IAlbumTrackList				mAlbumTrackList;
		private readonly IAlbumProvider					mAlbumProvider;
		private readonly BindableCollection<DbAlbum>	mAlbumList; 
		private long									mCurrentArtist;
		private DbAlbum									mCurrentAlbum;
		private TaskHandler								mAlbumRetrievalTaskHandler;

		public AlbumListViewModel( IAlbumTrackList trackListViewModel, IAlbumProvider albumProvider ) {
			mAlbumTrackList = trackListViewModel;
			mAlbumProvider = albumProvider;

			mAlbumList = new BindableCollection<DbAlbum>();
		}

		public void SetContext( long artistId ) {
			if( mCurrentArtist != artistId ) {
				mAlbumList.Clear();

				mCurrentArtist = artistId;
				RetrieveAlbumsForArtist( mCurrentArtist );
			}
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

		private void RetrieveAlbumsForArtist( long artistId ) {
			AlbumRetrievalTaskHandler.StartTask( () => {
			                                     	using( var albumList = mAlbumProvider.GetAlbumList( artistId )) {
			                                     		mAlbumList.AddRange( albumList.List );
			                                     	}
			                                     },
												 () => { },
												 ( ex ) => NoiseLogger.Current.LogException( "AlbumListViewModel:RetrieveAlbumsForArtist", ex )
				);
		}

		public BindableCollection<DbAlbum> AlbumList {
			get{ return( mAlbumList ); }
		}

		public DbAlbum SelectedAlbumList {
			get{ return( mCurrentAlbum ); }
			set {
				mCurrentAlbum = value;
			}
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
