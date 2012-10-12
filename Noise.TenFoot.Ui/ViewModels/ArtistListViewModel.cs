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

namespace Noise.TenFoot.Ui.ViewModels {
	public class ArtistListViewModel : BaseListViewModel<UiArtist>, IHomeScreen {
		private readonly IDatabaseInfo		mDatabaseInfo;
		private readonly IAlbumList			mAlbumsList;
		private readonly IArtistProvider	mArtistProvider;
		private readonly IArtworkProvider	mArtworkProvider;
		private readonly BitmapImage		mUnknownArtistImage;
		private TaskHandler					mArtistRetrievalTaskHandler;

		public	string						ScreenTitle { get; private set; }
		public	string						MenuTitle { get; private set; }
		public	string						Description { get; private set; }
		public	string						Context { get; private set; }
		public	eMainMenuCommand			MenuCommand { get; private set; }
		public	int							ScreenOrder { get; private set; }
		public	int							WrapItemCount { get; set; }

		public ArtistListViewModel( IEventAggregator eventAggregator, IResourceProvider resourceProvider, IDatabaseInfo databaseInfo,
									IAlbumList albumListViewModel, IArtistProvider artistProvider, IArtworkProvider artworkProvider ) :
			base( eventAggregator ) {
			mDatabaseInfo = databaseInfo;
			mAlbumsList = albumListViewModel;
			mArtistProvider = artistProvider;
			mArtworkProvider = artworkProvider;

			mUnknownArtistImage = resourceProvider.RetrieveImage( "Unknown Artist.png" );

			ScreenTitle = "Artists";
			MenuTitle = "Library";
			Description = "explore the music library";
			Context = "";

			MenuCommand = eMainMenuCommand.Library;
			ScreenOrder = 1;
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
			var retValue = new UiArtist( OnArtistSelect );

			if( fromArtist != null ) {
				Mapper.DynamicMap( fromArtist, retValue );
			}

			return( retValue );
		}

		private void RetrieveArtistList() {
			if( mDatabaseInfo.IsOpen ) {
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
		}

		private void OnArtistSelect( UiArtist artist ) {
			if( artist != null ) {
				DisplayItem();
			}
		}

		protected override void Left() {
			SetSelectedItem((int)SelectedItemIndex - WrapItemCount );
		}

		protected override void Right() {
			SetSelectedItem((int)SelectedItemIndex + WrapItemCount );
		}

		protected override void DisplayItem() {
			mAlbumsList.SetContext( SelectedItem );
			EventAggregator.Publish( new Input.Events.NavigateToScreen( mAlbumsList ));
		}
	}
}
