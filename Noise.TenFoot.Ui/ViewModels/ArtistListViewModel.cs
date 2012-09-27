using System.Linq;
using System.Windows.Media.Imaging;
using AutoMapper;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.TenFoot.Ui.Dto;
using Noise.TenFoot.Ui.Interfaces;
using ReusableBits;
using ReusableBits.Interfaces;
using ReusableBits.Mvvm.CaliburnSupport;

namespace Noise.TenFoot.Ui.ViewModels {
	public class ArtistListViewModel : BaseListViewModel<UiArtist>, IArtistList {
		private readonly IEventAggregator				mEventAggregator;
		private readonly IAlbumList						mAlbumsList;
		private readonly IArtistProvider				mArtistProvider;
		private readonly IArtworkProvider				mArtworkProvider;
		private readonly BitmapImage					mUnknownArtistImage;
		private TaskHandler								mArtistRetrievalTaskHandler;

		public ArtistListViewModel( IAlbumList albumListViewModel, IArtistProvider artistProvider, IArtworkProvider artworkProvider,
									IEventAggregator eventAggregator, IResourceProvider resourceProvider ) {
			mEventAggregator = eventAggregator;
			mAlbumsList = albumListViewModel;
			mArtistProvider = artistProvider;
			mArtworkProvider = artworkProvider;

			mUnknownArtistImage = resourceProvider.RetrieveImage( "Unknown Artist.png" );
		}

		protected override void OnInitialize() {
			base.OnInitialize();

			RetrieveArtistList();
		}

		protected override void OnActivate() {
			base.OnActivate();

			mEventAggregator.Subscribe( this );
		}

		protected override void OnDeactivate( bool close ) {
			base.OnDeactivate( close );

			mEventAggregator.Unsubscribe( this );
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
			var retValue = new UiArtist( OnArtistSelect );

			if( fromArtist != null ) {
				Mapper.DynamicMap( fromArtist, retValue );
			}

			return( retValue );
		}

		private void RetrieveArtistList() {
			ArtistRetrievalTaskHandler.StartTask( () => {
					using( var artistList = mArtistProvider.GetArtistList()) {
						ItemList.AddRange( from artist in artistList.List orderby artist.Name select TransformArtist( artist ));
					}

					foreach( var artist in ItemList ) {
						var artwork = mArtworkProvider.GetArtistArtwork( artist.DbId, ContentType.ArtistPrimaryImage );

						if(( artwork != null ) &&
						   ( artwork.HaveValidImage )) {
							artist.SetArtistArtwork( artwork );
						}
						else {
							artist.ArtistImage = mUnknownArtistImage;
						}
					}
				},
				() => { SelectedItem = ItemList.FirstOrDefault(); },
				ex => NoiseLogger.Current.LogException( "ArtistListViewModel:RetrieveArtistList", ex )
			); 
		}

		private void OnArtistSelect( UiArtist artist ) {
			if( artist != null ) {
				DisplayItem();
			}
		}

		private void SetSelectedArtist( int index ) {
			var artistCount = ItemList.Count();

			if( artistCount > 0 ) {
				if( index < 0 ) {
					index = artistCount + index;
				}

				if( index >= artistCount ) {
					index = index % artistCount;
				}

				if( index < artistCount ) {
					SelectedItem = ItemList[index];
				}
			}
		}

		protected override void NextItem() {
			SetSelectedArtist((int)SelectedItemIndex + 1 );
		}

		protected override void PreviousItem() {
			SetSelectedArtist((int)SelectedItemIndex - 1 );
		}

		protected override void DisplayItem() {
			if(( Parent is INavigate ) &&
			   ( SelectedItem != null )) {
				var controller = Parent as INavigate;

				mAlbumsList.SetContext( SelectedItem.DbId );
				controller.NavigateTo( mAlbumsList );
			}
		}
	}
}
