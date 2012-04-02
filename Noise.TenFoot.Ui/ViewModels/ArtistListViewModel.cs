using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.TenFoot.Ui.Interfaces;
using ReusableBits;
using ReusableBits.Mvvm.CaliburnSupport;

namespace Noise.TenFoot.Ui.ViewModels {
	public class ArtistListViewModel : Screen, IArtistList {
		private readonly IAlbumList						mAlbumsList;
		private readonly IArtistProvider				mArtistProvider;
		private readonly BindableCollection<DbArtist>	mArtistList; 
		private DbArtist								mSelectedArtist;
		private TaskHandler								mArtistRetrievalTaskHandler;

		public	double									ArtistIndex { get; set; }

		public ArtistListViewModel( IAlbumList albumListViewModel, IArtistProvider artistProvider ) {
			mAlbumsList = albumListViewModel;
			mArtistProvider = artistProvider;

			mArtistList = new BindableCollection<DbArtist>();
		}

		protected override void OnInitialize() {
			base.OnInitialize();

			RetrieveArtistList();
		}

		internal TaskHandler ArtistRetrievalTaskHandler {
			get {
				if( mArtistRetrievalTaskHandler == null ) {
					mArtistRetrievalTaskHandler = new TaskHandler();
				}

				return( mArtistRetrievalTaskHandler );
			}

			set{ mArtistRetrievalTaskHandler = value; }
		}

		private void RetrieveArtistList() {
			ArtistRetrievalTaskHandler.StartTask( () => {
					using( var artistList = mArtistProvider.GetArtistList()) {
						mArtistList.AddRange( from artist in artistList.List orderby artist.Name select artist );
					}
				},
				() => { },
				ex => NoiseLogger.Current.LogException( "ArtistListViewModel:RetrieveArtistList", ex )
			); 
		}

		public BindableCollection<DbArtist> ArtistList {
			get{ return( mArtistList ); }
		}
 
		public DbArtist SelectedArtistList {
			get{ return( mSelectedArtist ); }
			set {
				mSelectedArtist = value;

				if( mSelectedArtist != null ) {
					mAlbumsList.SetContext( mSelectedArtist.DbId );
					Albums();
				}
			}
		}

		public void NextArtist() {
			ArtistIndex += 1.0;

			NotifyOfPropertyChange( () => ArtistIndex );
		}

		public void PreviousArtist() {
			ArtistIndex -= 1.0;

			NotifyOfPropertyChange( () => ArtistIndex );
		}

		public void Albums() {
			if( Parent is INavigate ) {
				var controller = Parent as INavigate;

				controller.NavigateTo( mAlbumsList );
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
