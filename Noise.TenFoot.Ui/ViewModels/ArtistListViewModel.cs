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
	public class ArtistListViewModel : BaseListViewModel, IArtistList {
		private readonly IEventAggregator				mEventAggregator;
		private readonly IAlbumList						mAlbumsList;
		private readonly IArtistProvider				mArtistProvider;
		private readonly IArtworkProvider				mArtworkProvider;
		private readonly BitmapImage					mUnknownArtistImage;
		private readonly BindableCollection<UiArtist>	mArtistList; 
		private UiArtist								mSelectedArtist;
		private TaskHandler								mArtistRetrievalTaskHandler;

		public	double									ArtistIndex { get; set; }

		public ArtistListViewModel( IAlbumList albumListViewModel, IArtistProvider artistProvider, IArtworkProvider artworkProvider,
									IEventAggregator eventAggregator, IResourceProvider resourceProvider ) {
			mEventAggregator = eventAggregator;
			mAlbumsList = albumListViewModel;
			mArtistProvider = artistProvider;
			mArtworkProvider = artworkProvider;

			mUnknownArtistImage = resourceProvider.RetrieveImage( "Unknown Artist.png" );

			mArtistList = new BindableCollection<UiArtist>();

			mEventAggregator.Subscribe( this );
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
						mArtistList.AddRange( from artist in artistList.List orderby artist.Name select TransformArtist( artist ));
					}

					foreach( var artist in mArtistList ) {
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
				() => { SelectedArtist = mArtistList.FirstOrDefault(); },
				ex => NoiseLogger.Current.LogException( "ArtistListViewModel:RetrieveArtistList", ex )
			); 
		}

		private void OnArtistSelect( UiArtist artist ) {
			if( artist != null ) {
				DisplayItem();
			}
		}

		public BindableCollection<UiArtist> ArtistList {
			get{ return( mArtistList ); }
		}
 
		public UiArtist SelectedArtist {
			get{ return( mSelectedArtist ); }
			set {
				mSelectedArtist = value;

				if( mSelectedArtist != null ) {
					ArtistIndex = mArtistList.IndexOf( mSelectedArtist );

					NotifyOfPropertyChange( () => ArtistIndex );
				}

				NotifyOfPropertyChange( () => SelectedArtist );
			}
		}

		private void SetSelectedArtist( int index ) {
			var artistCount = ArtistList.Count();

			if( artistCount > 0 ) {
				if( index < 0 ) {
					index = artistCount + index;
				}

				if( index >= artistCount ) {
					index = index % artistCount;
				}

				if( index < artistCount ) {
					SelectedArtist = ArtistList[index];
				}
			}
		}

		protected override void NextItem() {
			SetSelectedArtist((int)ArtistIndex + 1 );
		}

		protected override void PreviousItem() {
			SetSelectedArtist((int)ArtistIndex - 1 );
		}

		protected override void DisplayItem() {
			if(( Parent is INavigate ) &&
			   ( mSelectedArtist != null )) {
				var controller = Parent as INavigate;

				mAlbumsList.SetContext( mSelectedArtist.DbId );
				controller.NavigateTo( mAlbumsList );
			}
		}
	}
}
