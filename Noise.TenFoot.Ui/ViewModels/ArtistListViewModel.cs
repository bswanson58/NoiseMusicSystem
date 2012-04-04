using System.Linq;
using AutoMapper;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.TenFoot.Ui.Dto;
using Noise.TenFoot.Ui.Interfaces;
using ReusableBits;
using ReusableBits.Mvvm.CaliburnSupport;

namespace Noise.TenFoot.Ui.ViewModels {
	public class ArtistListViewModel : Screen, IArtistList {
		private readonly IAlbumList						mAlbumsList;
		private readonly IArtistProvider				mArtistProvider;
		private readonly IArtworkProvider				mArtworkProvider;
		private readonly BindableCollection<UiArtist>	mArtistList; 
		private DbArtist								mSelectedArtist;
		private TaskHandler								mArtistRetrievalTaskHandler;

		public	double									ArtistIndex { get; set; }

		public ArtistListViewModel( IAlbumList albumListViewModel, IArtistProvider artistProvider, IArtworkProvider artworkProvider ) {
			mAlbumsList = albumListViewModel;
			mArtistProvider = artistProvider;
			mArtworkProvider = artworkProvider;

			mArtistList = new BindableCollection<UiArtist>();
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

		private UiArtist TransformArtist( DbArtist fromArtist ) {
			var retValue = new UiArtist();

			if( fromArtist != null ) {
				Mapper.DynamicMap( fromArtist, retValue );

				retValue.ArtistImage = mArtworkProvider.GetArtistArtwork( retValue.DbId, ContentType.ArtistPrimaryImage );
			}

			return( retValue );
		}

		private void RetrieveArtistList() {
			ArtistRetrievalTaskHandler.StartTask( () => {
					using( var artistList = mArtistProvider.GetArtistList()) {
						mArtistList.AddRange( from artist in artistList.List orderby artist.Name select TransformArtist( artist ));
					}
				},
				() => { },
				ex => NoiseLogger.Current.LogException( "ArtistListViewModel:RetrieveArtistList", ex )
			); 
		}

		public BindableCollection<UiArtist> ArtistList {
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
