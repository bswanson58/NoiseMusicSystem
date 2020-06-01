using AutoMapper;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Dto;
using Noise.UI.Interfaces;
using Noise.UI.Logging;
using Observal.Extensions;
using ReusableBits;
using ReusableBits.Mvvm.ViewModelSupport;
using System;
using Noise.UI.Views;
using Prism;
using Prism.Commands;
using Prism.Services.Dialogs;

namespace Noise.UI.ViewModels {
//	public class ArtistEditRequest : InteractionRequestData<UiArtist> {
//		public ArtistEditRequest(UiArtist artist ) : base( artist ) { } 
//	}

	internal class ArtistViewModel : AutomaticPropertyBase, IActiveAware,
									 IHandle<Events.DatabaseClosing>,
									 IHandle<Events.ArtistContentUpdated>, IHandle<Events.ArtistUserUpdate> {
		private readonly IEventAggregator		mEventAggregator;
		private readonly IUiLog					mLog;
		private readonly ISelectionState		mSelectionState;
		private readonly IArtistProvider		mArtistProvider;
		private readonly ITagManager			mTagManager;
		private readonly IMetadataManager		mMetadataManager;
		private readonly IPlayCommand			mPlayCommand;
		private readonly IPlayingItemHandler    mPlayingItemHandler;
		private readonly IRatings				mRatings;
		private readonly IDialogService			mDialogService;
		private readonly Observal.Observer		mChangeObserver;
		private UiArtist						mCurrentArtist;
		private Artwork							mArtistImage;
		private TaskHandler<DbArtist>			mArtistTaskHandler; 
		private TaskHandler<Artwork>			mArtworkTaskHandler; 
		private IDisposable						mArtistSelectionSubscription;
		private bool							mIsActive;
        private bool                            mPortfolioAvailable;

        public  bool                            ArtistValid => Artist != null;
		public	bool							ArtworkValid { get; private set; }
        public  UiArtist                        Artist => mCurrentArtist;

        public	event EventHandler				IsActiveChanged  = delegate { };

		public	DelegateCommand					PlayRandomTracks { get; }
		public	DelegateCommand					PlayTopTracks {  get; }
		public	DelegateCommand					GenreClicked { get; }
		public	DelegateCommand					WebsiteClicked { get; }
		public	DelegateCommand					EditArtist { get; }

		public	DelegateCommand					DisplayArtistInfoPanel { get; }
		public  DelegateCommand					DisplayAlbumInfoPanel { get; }
		public	DelegateCommand					DisplayArtistTracksPanel { get; }
		public	DelegateCommand					DisplayRatedTracksPanel { get; }
		public	DelegateCommand					DisplayPortfolio { get; }

		public ArtistViewModel( IEventAggregator eventAggregator, IArtistProvider artistProvider, IRatings ratings, ISelectionState selectionState, IPlayingItemHandler playingItemHandler,
								ITagManager tagManager, IMetadataManager metadataManager, IPlayCommand playCommand, IDialogService dialogService, IUiLog log ) {
			mEventAggregator = eventAggregator;
			mLog = log;
			mSelectionState = selectionState;
			mArtistProvider = artistProvider;
			mTagManager = tagManager;
			mMetadataManager = metadataManager;
			mPlayCommand = playCommand;
			mPlayingItemHandler = playingItemHandler;
			mRatings = ratings;
			mDialogService = dialogService;

			mPlayingItemHandler.StartHandler( () => mCurrentArtist );

			mEventAggregator.Subscribe( this );

			mChangeObserver = new Observal.Observer();
			mChangeObserver.Extend( new PropertyChangedExtension()).WhenPropertyChanges( OnArtistChanged );

			PlayRandomTracks = new DelegateCommand( OnPlayRandomTracks, CanPlayRandomTracks );
			PlayTopTracks = new DelegateCommand( OnPlayTopTracks, CanPlayTopTracks );
			GenreClicked = new DelegateCommand( OnGenreClicked, CanGenreClicked );
			WebsiteClicked = new DelegateCommand( OnWebsiteClicked );
			EditArtist = new DelegateCommand( OnEditArtist, CanEditArtist );
			DisplayAlbumInfoPanel = new DelegateCommand( OnDisplayAlbumInfoPanel );
			DisplayArtistInfoPanel = new DelegateCommand( OnDisplayArtistInfoPanel );
			DisplayArtistTracksPanel = new DelegateCommand( OnDisplayArtistTracksPanel );
			DisplayRatedTracksPanel = new DelegateCommand( OnDisplayRatedTracksPanel );
			DisplayPortfolio = new DelegateCommand( OnDisplayPortfolio, CanDisplayPortfolio );

            mArtistSelectionSubscription = mSelectionState.CurrentArtistChanged.Subscribe( OnArtistRequested );
 
			OnArtistRequested( mSelectionState.CurrentArtist );
		}

		public bool IsActive {
			get => ( mIsActive );
            set {
				if( mIsActive ) {
					if( mArtistSelectionSubscription != null ) {
						mArtistSelectionSubscription.Dispose();
						mArtistSelectionSubscription = null;
					}
				}
				else {
					if( mArtistSelectionSubscription == null ) {
						mArtistSelectionSubscription = mSelectionState.CurrentArtistChanged.Subscribe( OnArtistRequested );
					}
				}

				mIsActive = value;
				IsActiveChanged( this, new EventArgs());
			}
		}

        public bool ArtistPortfolioAvailable {
            get => mPortfolioAvailable;
            set {
                mPortfolioAvailable = value;

                RaisePropertyChanged( () => ArtistPortfolioAvailable );
				DisplayPortfolio.RaiseCanExecuteChanged();
            }
        }

        private UiArtist TransformArtist( DbArtist dbArtist ) {
			var retValue = new UiArtist();

			if( dbArtist != null ) {
				Mapper.Map( dbArtist, retValue );
				retValue.DisplayGenre = mTagManager.GetGenre( dbArtist.Genre );

				var artistMetadata = mMetadataManager.GetArtistMetadata( dbArtist.Name );
				if( artistMetadata != null ) {
					retValue.ActiveYears = artistMetadata.GetMetadata( eMetadataType.ActiveYears );
					retValue.Website = artistMetadata.GetMetadata( eMetadataType.WebSite );
				}
			}

			return( retValue );
		}

		private void ClearCurrentArtist() {
			if( mCurrentArtist != null ) {
				mChangeObserver.Release( mCurrentArtist );

				mCurrentArtist = null;

                ArtistPortfolioAvailable = false;

				RaisePropertyChanged( () => ArtistValid );
				RaisePropertyChanged( () => Artist );
			}
		}

		public void ClearCurrentArtistInfo() {
			mArtistImage = null;

			RaisePropertyChanged( () => ArtistImage );
			EditArtist.RaiseCanExecuteChanged();
            GenreClicked.RaiseCanExecuteChanged();
			WebsiteClicked.RaiseCanExecuteChanged();
            PlayRandomTracks.RaiseCanExecuteChanged();
			PlayTopTracks.RaiseCanExecuteChanged();
		}

		private void SetCurrentArtist( DbArtist artist ) {
			CurrentArtist = artist != null ? TransformArtist( artist ) : null;

			if( CurrentArtist != null ) {
				RetrieveArtwork( CurrentArtist.Name );
			}
		}

		private UiArtist CurrentArtist {
			get => ( mCurrentArtist );
            set {
				ClearCurrentArtist();

				if( value != null ) {
					mCurrentArtist = value;
					mChangeObserver.Add( mCurrentArtist );
					RaisePropertyChanged( () => Artist );

                    mPlayingItemHandler.UpdateItem();

                    RaisePropertyChanged( () => ArtistImage );
					RaisePropertyChanged( () => ArtistValid );
					EditArtist.RaiseCanExecuteChanged();
                    GenreClicked.RaiseCanExecuteChanged();
					WebsiteClicked.RaiseCanExecuteChanged();
                    PlayRandomTracks.RaiseCanExecuteChanged();
                    PlayTopTracks.RaiseCanExecuteChanged();
				}
			}
		}

		public void Handle( Events.DatabaseClosing args ) {
			ClearCurrentArtist();
			OnDisplayArtistInfoPanel();
		}

		public void Handle( Events.ArtistContentUpdated eventArgs ) {
			if(( CurrentArtist != null ) &&
			   ( CurrentArtist.DbId == eventArgs.ArtistId )) {
				RequestArtist( CurrentArtist.DbId );
			}
		}

		public void Handle( Events.ArtistUserUpdate eventArgs ) {
			if(( CurrentArtist != null ) &&
			   ( eventArgs.ArtistId == CurrentArtist.DbId )) {
				RequestArtist( CurrentArtist.DbId );
			}
		}

		private void OnArtistRequested( DbArtist artist ) {
			if( artist == null ) {
				ClearCurrentArtist();
			}
			else {
				RequestArtistAndContent( artist.DbId );
			}
		}

		internal TaskHandler<DbArtist> ArtistTaskHandler {
			get {
				if( mArtistTaskHandler == null ) {
					Execute.OnUIThread( () => mArtistTaskHandler = new TaskHandler<DbArtist>());
				}

				return( mArtistTaskHandler );
			}

			set => mArtistTaskHandler = value;
        }
 
		private void RequestArtistAndContent( long artistId ) {
			ClearCurrentArtist();
			ClearCurrentArtistInfo();

			RequestArtist( artistId );

			mEventAggregator.PublishOnUIThread( new Events.ArtistContentRequest( artistId ));
		}

		private void RequestArtist( long artistId ) {
			RetrieveArtist( artistId );
		}

		private void RetrieveArtist( long artistId ) {
			ArtistTaskHandler.StartTask( () => mArtistProvider.GetArtist( artistId ), 
										SetCurrentArtist,
										exception => mLog.LogException( $"GetArtist:{artistId}", exception ));
		}

		internal TaskHandler<Artwork> ArtworkTaskHandler {
			get {
				if( mArtworkTaskHandler == null ) {
					Execute.OnUIThread( () => mArtworkTaskHandler = new TaskHandler<Artwork>());
				}

				return( mArtworkTaskHandler );
			}

			set => mArtworkTaskHandler = value;
        }

		private void RetrieveArtwork( string artistName ) {
            ArtistPortfolioAvailable = mMetadataManager.ArtistPortfolioAvailable( artistName );

            if( ArtistPortfolioAvailable ) { 
                ArtworkTaskHandler.StartTask( () => mMetadataManager.GetArtistArtwork( artistName ),
                                              SetArtwork,
                                              exception => mLog.LogException( $"GetArtistArtwork for '{artistName}'", exception ));
            }
			else {
				SetArtwork( null );
            }
		}

		private void SetArtwork( Artwork artwork ) {
			mArtistImage = artwork;

			ArtworkValid = artwork != null && artwork.HaveValidImage;

			RaisePropertyChanged( () => ArtistImage );
			RaisePropertyChanged( () => ArtworkValid );
		}
 
		private void OnArtistChanged( PropertyChangeNotification changeNotification ) {
            if( changeNotification.Source is UiArtist notifier ) {
				var artist = mArtistProvider.GetArtist( notifier.DbId );

				if( changeNotification.PropertyName == "UiRating" ) {
					mRatings.SetRating( artist, notifier.UiRating );
				}
				if( changeNotification.PropertyName == "UiIsFavorite" ) {
					mRatings.SetFavorite( artist, notifier.UiIsFavorite );
				}
			}
		}

		private void OnWebsiteClicked() {
			if(( CurrentArtist != null ) &&
			   (!string.IsNullOrWhiteSpace( CurrentArtist.Website ))) {
				mEventAggregator.PublishOnUIThread( new Events.UrlLaunchRequest( CurrentArtist.Website ));
			}
		}

		public byte[] ArtistImage {
			get {
				byte[]	retValue = null;

				if( mArtistImage != null ) {
					retValue = mArtistImage.Image;
				}

				return( retValue );
			}
		}

        private void OnPlayRandomTracks() {
			if( CurrentArtist != null ) {
				mPlayCommand.PlayRandomArtistTracks( mArtistProvider.GetArtist( CurrentArtist.DbId ));
			}
		}

		private void OnPlayTopTracks() {
			if( CurrentArtist != null ) {
				mPlayCommand.PlayTopArtistTracks( mArtistProvider.GetArtist( CurrentArtist.DbId ));
			}
		}

		public bool CanPlayTopTracks() {
			return( CurrentArtist != null );
		}

		public bool CanPlayRandomTracks() {
			return( CurrentArtist != null );
		}

		private void OnGenreClicked() {
			if( CurrentArtist != null ) {
                mEventAggregator.PublishOnUIThread( new Events.GenreFocusRequested( Artist.Genre ));
            }
        }

        public bool CanGenreClicked() {
            return( CurrentArtist != null );
        }

        private void OnEditArtist() {
			if( mCurrentArtist != null ) {
				var parameters = new DialogParameters{{ ArtistEditDialogModel.cArtistParameter, mCurrentArtist }};

				mDialogService.ShowDialog( nameof( ArtistEditDialog ), parameters, result => {
					if( result.Result == ButtonResult.OK ) {
						var artist = result.Parameters.GetValue<UiArtist>( ArtistEditDialogModel.cArtistParameter );

						if( artist != null ) {
                            using( var updater = mArtistProvider.GetArtistForUpdate( artist.DbId )) {
                                if( updater.Item != null ) {
                                    Mapper.Map( artist, updater.Item );

                                    updater.Update();
                                }

								RaisePropertyChanged( () => Artist );
                            }
                        }
					}
                });
			}
		}

		private bool CanEditArtist() {
			return( CurrentArtist != null );
		}

        public bool ArtistInfoViewOpen {
			get{ return( Get( () => ArtistInfoViewOpen )); }
			set{ Set( () => ArtistInfoViewOpen, value ); }
		}

		private void OnDisplayArtistInfoPanel() {
			var request = new Events.ViewDisplayRequest( ViewNames.ArtistInfoView );

			mEventAggregator.PublishOnUIThread( request );

			ArtistInfoViewOpen = request.ViewWasOpened;
		}

		public bool AlbumInfoViewOpen {
			get{ return( Get( () => AlbumInfoViewOpen )); }
			set{ Set( () => AlbumInfoViewOpen, value ); }
		}

		public void OnDisplayAlbumInfoPanel() {
			var request = new Events.ViewDisplayRequest( ViewNames.AlbumInfoView );

			mEventAggregator.PublishOnUIThread( request );

			AlbumInfoViewOpen = request.ViewWasOpened;
		}

		public bool ArtistTracksViewOpen {
			get{ return( Get( () => ArtistTracksViewOpen )); }
			set{ Set( () => ArtistTracksViewOpen, value ); }
		}

		public void OnDisplayArtistTracksPanel() {
			var request = new Events.ViewDisplayRequest( ViewNames.ArtistTracksView );

			mEventAggregator.PublishOnUIThread( request );

			ArtistTracksViewOpen = request.ViewWasOpened;
		}

        public bool RatedTracksViewOpen {
            get{ return( Get( () => RatedTracksViewOpen )); }
            set{ Set( () => RatedTracksViewOpen, value ); }
        }

        public void OnDisplayRatedTracksPanel() {
            var request = new Events.ViewDisplayRequest( ViewNames.RatedTracksView );

            mEventAggregator.PublishOnUIThread( request );

            RatedTracksViewOpen = request.ViewWasOpened;
        }

        private void OnDisplayPortfolio() {
            if( CanDisplayPortfolio()) {
				var parameters = new DialogParameters{{ ArtistArtworkViewModel.cArtistParameter, CurrentArtist }, 
                                                      { ArtistArtworkViewModel.cArtworkParameter, mArtistImage?.Name }};

				mDialogService.ShowDialog( nameof( ArtistArtworkView ), parameters, result => { });
            }
        }

        private bool CanDisplayPortfolio() {
            return( CurrentArtist != null ) && ArtistPortfolioAvailable;
        }
	}
}
