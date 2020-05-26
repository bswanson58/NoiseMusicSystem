using AutoMapper;
using Caliburn.Micro;
using Microsoft.Practices.Prism;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Adapters;
using Noise.UI.Behaviours;
using Noise.UI.Dto;
using Noise.UI.Interfaces;
using Noise.UI.Logging;
using Observal.Extensions;
using ReusableBits;
using ReusableBits.Mvvm.ViewModelSupport;
using System;

namespace Noise.UI.ViewModels {
	public class ArtistEditRequest : InteractionRequestData<UiArtist> {
		public ArtistEditRequest(UiArtist artist ) : base( artist ) { } 
	}

    internal class ArtistArtworkDisplayInfo : InteractionRequestData<ArtistArtworkViewModel> {
        public ArtistArtworkDisplayInfo( ArtistArtworkViewModel viewModel ) : base( viewModel ) { }
    }

	internal class ArtistViewModel : AutomaticCommandBase, IActiveAware,
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
		private readonly Observal.Observer		mChangeObserver;
		private UiArtist						mCurrentArtist;
		private Artwork							mArtistImage;
		private LinkNode						mArtistWebsite;
		private TaskHandler<DbArtist>			mArtistTaskHandler; 
		private TaskHandler<Artwork>			mArtworkTaskHandler; 
		private TaskHandler						mTopTracksTaskHandler;
		private IDisposable						mArtistSelectionSubscription;
		private bool							mIsActive;
        private bool                            mPortfolioAvailable;

        [DependsUpon( "Artist" )]
        public  bool                            ArtistValid => Artist != null;
		public	bool							ArtworkValid { get; private set; }
        public  UiArtist                        Artist => mCurrentArtist;
        public  LinkNode                        ArtistWebsite => mArtistWebsite;
		public	event EventHandler				IsActiveChanged  = delegate { };

		private readonly InteractionRequest<ArtistEditRequest>		    mArtistEditRequest;
        private readonly InteractionRequest<ArtistArtworkDisplayInfo>   mArtistArtworkDisplayRequest;

		public ArtistViewModel( IEventAggregator eventAggregator, IArtistProvider artistProvider, IRatings ratings, ISelectionState selectionState, IPlayingItemHandler playingItemHandler,
								ITagManager tagManager, IMetadataManager metadataManager, IPlayCommand playCommand, IUiLog log ) {
			mEventAggregator = eventAggregator;
			mLog = log;
			mSelectionState = selectionState;
			mArtistProvider = artistProvider;
			mTagManager = tagManager;
			mMetadataManager = metadataManager;
			mPlayCommand = playCommand;
			mPlayingItemHandler = playingItemHandler;
			mRatings = ratings;

			mPlayingItemHandler.StartHandler( () => mCurrentArtist );

			mEventAggregator.Subscribe( this );

			mChangeObserver = new Observal.Observer();
			mChangeObserver.Extend( new PropertyChangedExtension()).WhenPropertyChanges( OnArtistChanged );

			mArtistEditRequest = new InteractionRequest<ArtistEditRequest>();
            mArtistArtworkDisplayRequest = new InteractionRequest<ArtistArtworkDisplayInfo>();

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
				mArtistWebsite = null;

                ArtistPortfolioAvailable = false;

				RaisePropertyChanged( () => Artist );
				RaisePropertyChanged( () => ArtistWebsite );
			}
		}

		public void ClearCurrentArtistInfo() {
			mArtistImage = null;

			RaisePropertyChanged( () => ArtistImage );
			RaiseCanExecuteChangedEvent( "CanExecute_PlayTopTracks" );
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

					mArtistWebsite = new LinkNode( CurrentArtist.Website, 0, OnWebsiteRequested );
					RaisePropertyChanged( () => ArtistWebsite );
				}
			}
		}

		public void Handle( Events.DatabaseClosing args ) {
			ClearCurrentArtist();
			DisplayArtistInfoPanel();
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

		internal TaskHandler TopTracksTaskHandler {
			get {
				if( mTopTracksTaskHandler == null ) {
					Execute.OnUIThread( () => mTopTracksTaskHandler = new TaskHandler());
				}

				return( mTopTracksTaskHandler );
			}
			set => mTopTracksTaskHandler = value;
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

		private void OnWebsiteRequested( long id ) {
			if(( CurrentArtist != null ) &&
			   (!string.IsNullOrWhiteSpace( CurrentArtist.Website ))) {
				mEventAggregator.PublishOnUIThread( new Events.UrlLaunchRequest( CurrentArtist.Website ));
			}
		}

		[DependsUpon( "Artist" )]
		public byte[] ArtistImage {
			get {
				byte[]	retValue = null;

				if( mArtistImage != null ) {
					retValue = mArtistImage.Image;
				}

				return( retValue );
			}
		}

        public void Execute_PlayRandomTracks() {
			if( CurrentArtist != null ) {
				mPlayCommand.PlayRandomArtistTracks( mArtistProvider.GetArtist( CurrentArtist.DbId ));
			}
		}

		public void Execute_PlayTopTracks() {
			if( CurrentArtist != null ) {
				mPlayCommand.PlayTopArtistTracks( mArtistProvider.GetArtist( CurrentArtist.DbId ));
			}
		}

		[DependsUpon( "Artist" )]
		public bool CanExecute_PlayTopTracks() {
			return( CurrentArtist != null );
		}

		[DependsUpon( "Artist" )]
		public bool CanExecute_PlayRandomTracks() {
			return( CurrentArtist != null );
		}

		public void Execute_GenreClicked() {
			if( CurrentArtist != null ) {
                mEventAggregator.PublishOnUIThread( new Events.GenreFocusRequested( Artist.Genre ));
            }
        }

        [DependsUpon( "Artist" )]
        public bool CanExecute_GenreClicked() {
            return( CurrentArtist != null );
        }

		public IInteractionRequest ArtistEditRequest => ( mArtistEditRequest );

        public void Execute_EditArtist() {
			if( mCurrentArtist != null ) {
				mArtistEditRequest.Raise( new ArtistEditRequest( mCurrentArtist ), OnArtistEdited );
			}
		}

		[DependsUpon( "Artist" )]
		public bool CanExecute_EditArtist() {
			return( CurrentArtist != null );
		}

		private void OnArtistEdited( ArtistEditRequest confirmation ) {
			if( confirmation.Confirmed ) {
				using( var updater = mArtistProvider.GetArtistForUpdate( confirmation.ViewModel.DbId )) {
					if( updater.Item != null ) {
						Mapper.Map( confirmation.ViewModel, updater.Item );
						updater.Update();
					}
				}
			}
		}

		public bool ArtistInfoViewOpen {
			get{ return( Get( () => ArtistInfoViewOpen )); }
			set{ Set( () => ArtistInfoViewOpen, value ); }
		}

		public void Execute_DisplayArtistInfoPanel() {
			DisplayArtistInfoPanel();
		}

		private void DisplayArtistInfoPanel() {
			var request = new Events.ViewDisplayRequest( ViewNames.ArtistInfoView );

			mEventAggregator.PublishOnUIThread( request );

			ArtistInfoViewOpen = request.ViewWasOpened;
		}

		public bool AlbumInfoViewOpen {
			get{ return( Get( () => AlbumInfoViewOpen )); }
			set{ Set( () => AlbumInfoViewOpen, value ); }
		}

		public void Execute_DisplayAlbumInfoPanel() {
			var request = new Events.ViewDisplayRequest( ViewNames.AlbumInfoView );

			mEventAggregator.PublishOnUIThread( request );

			AlbumInfoViewOpen = request.ViewWasOpened;
		}

		public bool ArtistTracksViewOpen {
			get{ return( Get( () => ArtistTracksViewOpen )); }
			set{ Set( () => ArtistTracksViewOpen, value ); }
		}

		public void Execute_DisplayArtistTracksPanel() {
			var request = new Events.ViewDisplayRequest( ViewNames.ArtistTracksView );

			mEventAggregator.PublishOnUIThread( request );

			ArtistTracksViewOpen = request.ViewWasOpened;
		}

        public bool RatedTracksViewOpen {
            get{ return( Get( () => RatedTracksViewOpen )); }
            set{ Set( () => RatedTracksViewOpen, value ); }
        }

        public void Execute_DisplayRatedTracksPanel() {
            var request = new Events.ViewDisplayRequest( ViewNames.RatedTracksView );

            mEventAggregator.PublishOnUIThread( request );

            RatedTracksViewOpen = request.ViewWasOpened;
        }

        public IInteractionRequest ArtistArtworkDisplayRequest => ( mArtistArtworkDisplayRequest );

        [DependsUpon( "ArtistPortfolioAvailable" )]
        public bool CanExecute_DisplayPortfolio() {
            return( CurrentArtist != null ) && ArtistPortfolioAvailable;
        }

        public void Execute_DisplayPortfolio() {
            if( CanExecute_DisplayPortfolio()) {
                var vm = new ArtistArtworkViewModel( mMetadataManager, CurrentArtist, mLog, mArtistImage?.Name );

                mArtistArtworkDisplayRequest.Raise( new ArtistArtworkDisplayInfo( vm ), AfterArtworkDisplayed );
            }
        }

        private void AfterArtworkDisplayed( ArtistArtworkDisplayInfo confirmation ) { }
	}
}
