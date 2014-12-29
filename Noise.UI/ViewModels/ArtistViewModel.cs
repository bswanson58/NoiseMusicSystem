using System.Collections.Generic;
using System.Linq;
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
using Observal.Extensions;
using ReusableBits;
using ReusableBits.Mvvm.ViewModelSupport;
using System;
namespace Noise.UI.ViewModels {
	public class ArtistEditRequest : InteractionRequestData<UiArtist> {
		public ArtistEditRequest(UiArtist artist ) : base( artist ) { } 
	}

	public class ArtistViewModel : AutomaticCommandBase, IActiveAware,
								   IHandle<Events.DatabaseClosing>,
								   IHandle<Events.ArtistContentUpdated>, IHandle<Events.ArtistUserUpdate> {
		private readonly IEventAggregator		mEventAggregator;
		private readonly ISelectionState		mSelectionState;
		private readonly IArtistProvider		mArtistProvider;
		private readonly ITrackProvider			mTrackProvider;
		private readonly ITagManager			mTagManager;
		private readonly IMetadataManager		mMetadataManager;
		private readonly IPlayQueue				mPlayQueue;
		private readonly Observal.Observer		mChangeObserver;
		private readonly List<DbTrack>			mTopPlayTracks; 
		private readonly Random					mRandom;
		private DbArtist						mDbArtist;
		private UiArtist						mCurrentArtist;
		private Artwork							mArtistImage;
		private LinkNode						mArtistWebsite;
		private TaskHandler<DbArtist>			mArtistTaskHandler; 
		private TaskHandler<Artwork>			mArtworkTaskHandler; 
		private TaskHandler						mTopTracksTaskHandler;
		private IDisposable						mSelectionStateSubscription;
		private bool							mIsActive;
		public	event EventHandler				IsActiveChanged  = delegate { };

		private readonly InteractionRequest<ArtistEditRequest>		mArtistEditRequest;

		public ArtistViewModel( IEventAggregator eventAggregator, IArtistProvider artistProvider, ITrackProvider trackProvider,
								ISelectionState selectionState, ITagManager tagManager, IMetadataManager metadataManager, IPlayQueue playQueue ) {
			mEventAggregator = eventAggregator;
			mSelectionState = selectionState;
			mArtistProvider = artistProvider;
			mTrackProvider = trackProvider;
			mTagManager = tagManager;
			mMetadataManager = metadataManager;
			mPlayQueue = playQueue;

			mTopPlayTracks = new List<DbTrack>();
			mRandom = new Random( DateTime.Now.Millisecond );

			mEventAggregator.Subscribe( this );

			mChangeObserver = new Observal.Observer();
			mChangeObserver.Extend( new PropertyChangedExtension()).WhenPropertyChanges( OnArtistChanged );

			mArtistEditRequest = new InteractionRequest<ArtistEditRequest>();
 
			OnArtistRequested( mSelectionState.CurrentArtist );
		}

		public bool IsActive {
			get{ return( mIsActive ); }
			set {
				if( mIsActive ) {
					if( mSelectionStateSubscription != null ) {
						mSelectionStateSubscription.Dispose();
						mSelectionStateSubscription = null;
					}
				}
				else {
					if( mSelectionStateSubscription == null ) {
						mSelectionStateSubscription = mSelectionState.CurrentArtistChanged.Subscribe( OnArtistRequested );
					}
				}

				mIsActive = value;
				IsActiveChanged( this, new EventArgs());
			}
		}

		public UiArtist Artist {
			get{ return( mCurrentArtist ); }
		}

		[DependsUpon( "Artist" )]
		public bool ArtistValid {
			get{ return( Artist != null ); }
		}

		private UiArtist TransformArtist( DbArtist dbArtist ) {
			var retValue = new UiArtist();

			if( dbArtist != null ) {
				Mapper.DynamicMap( dbArtist, retValue );
				retValue.DisplayGenre = mTagManager.GetGenre( dbArtist.Genre );

				var artistMetadata = mMetadataManager.GetArtistMetadata( dbArtist.Name );
				if( artistMetadata != null ) {
					retValue.Website = artistMetadata.GetMetadata( eMetadataType.WebSite );
				}
			}

			return( retValue );
		}

		private void ClearCurrentArtist() {
			if( mCurrentArtist != null ) {
				mChangeObserver.Release( mCurrentArtist );

				mCurrentArtist = null;
				mDbArtist = null;
				mArtistWebsite = null;

				RaisePropertyChanged( () => Artist );
				RaisePropertyChanged( () => ArtistWebsite );
			}
		}

		public void ClearCurrentArtistInfo() {
			mArtistImage = null;
			mTopPlayTracks.Clear();

			RaisePropertyChanged( () => ArtistImage );
			RaiseCanExecuteChangedEvent( "CanExecute_PlayTopTracks" );
		}

		private void SetCurrentArtist( DbArtist artist ) {
			CurrentArtist = artist != null ? TransformArtist( artist ) : null;

			if( CurrentArtist != null ) {
				mDbArtist = artist;

				RetrieveArtwork( CurrentArtist.Name );
				RetrieveTopTracks();
			}
		}

		private UiArtist CurrentArtist {
			get{ return( mCurrentArtist ); }
			set {
				if( value != null ) {
					mCurrentArtist = value;
					mChangeObserver.Add( mCurrentArtist );
					RaisePropertyChanged( () => Artist );

					mArtistWebsite = new LinkNode( CurrentArtist.Website, 0, OnWebsiteRequested );
					RaisePropertyChanged( () => ArtistWebsite );
				}
				else {
					ClearCurrentArtist();
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

			set { mArtistTaskHandler = value; }
		}
 
		private void RequestArtistAndContent( long artistId ) {
			ClearCurrentArtist();
			ClearCurrentArtistInfo();

			RequestArtist( artistId );

			mEventAggregator.Publish( new Events.ArtistContentRequest( artistId ));
		}

		private void RequestArtist( long artistId ) {
			RetrieveArtist( artistId );
		}

		private void RetrieveArtist( long artistId ) {
			ArtistTaskHandler.StartTask( () => mArtistProvider.GetArtist( artistId ), 
										SetCurrentArtist,
										exception => NoiseLogger.Current.LogException( "ArtistViewModel:GetArtist", exception ));
		}

		internal TaskHandler<Artwork> ArtworkTaskHandler {
			get {
				if( mArtworkTaskHandler == null ) {
					Execute.OnUIThread( () => mArtworkTaskHandler = new TaskHandler<Artwork>());
				}

				return( mArtworkTaskHandler );
			}

			set { mArtworkTaskHandler = value; }
		}

		private void RetrieveArtwork( string artistName ) {
			ArtworkTaskHandler.StartTask( () => mMetadataManager.GetArtistArtwork( artistName ),
										   SetArtwork,
										   exception => NoiseLogger.Current.LogException( "ArtistViewModel:GetArtistArtwork", exception ));
		}

		internal TaskHandler TopTracksTaskHandler {
			get {
				if( mTopTracksTaskHandler == null ) {
					Execute.OnUIThread( () => mTopTracksTaskHandler = new TaskHandler());
				}

				return( mTopTracksTaskHandler );
			}
			set {
				mTopTracksTaskHandler = value;
			}
		}

		private void RetrieveTopTracks() {
			TopTracksTaskHandler.StartTask( () => {
												if( mDbArtist != null ) {
													var info = mMetadataManager.GetArtistMetadata( mDbArtist.Name );
													var topTracks = info.GetMetadataArray( eMetadataType.TopTracks ).ToArray();

													if( topTracks.Any()) {
														var allTracks = mTrackProvider.GetTrackList( mDbArtist );

														foreach( var trackName in topTracks ) {
															string	name = trackName;
															var		trackList = allTracks.List.Where( t => t.Name.Equals( name, StringComparison.CurrentCultureIgnoreCase )).ToList();

															if( trackList.Any()) {
																var selectedTrack = trackList.Skip( NextRandom( trackList.Count - 1 )).Take( 1 ).FirstOrDefault();

																if( selectedTrack != null ) {
																	mTopPlayTracks.Add( selectedTrack );
																}
															}
														}
													}
												}
											},
											() => RaiseCanExecuteChangedEvent( "CanExecute_PlayTopTracks" ),
											exception => NoiseLogger.Current.LogException( "ArtistViewModel:RetrieveTopTracks", exception ));
		}

		private int NextRandom( int maxValue ) {
			return( mRandom.Next( maxValue ));
		}

		private void SetArtwork( Artwork artwork ) {
			mArtistImage = artwork;

			RaisePropertyChanged( () => ArtistImage );
		}
 
		private static void OnArtistChanged( PropertyChangeNotification changeNotification ) {
			var notifier = changeNotification.Source as UiArtist;

			if( notifier != null ) {
				if( changeNotification.PropertyName == "UiRating" ) {
					GlobalCommands.SetRating.Execute( new SetRatingCommandArgs( notifier.DbId, notifier.UiRating ));
				}
				if( changeNotification.PropertyName == "UiIsFavorite" ) {
					GlobalCommands.SetFavorite.Execute( new SetFavoriteCommandArgs( notifier.DbId, notifier.UiIsFavorite ));
				}
			}
		}

		private void OnWebsiteRequested( long id ) {
			if(( CurrentArtist != null ) &&
			   (!string.IsNullOrWhiteSpace( CurrentArtist.Website ))) {
				mEventAggregator.Publish( new Events.UrlLaunchRequest( CurrentArtist.Website ));
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

		public LinkNode ArtistWebsite {
			get{ return( mArtistWebsite ); }
		}

		public void Execute_PlayRandomTracks() {
			if( CurrentArtist != null ) {
				mEventAggregator.Publish( new Events.PlayArtistTracksRandom( CurrentArtist.DbId ));
			}
		}

		public void Execute_PlayTopTracks() {
			if( mTopPlayTracks.Any()) {
				mPlayQueue.Add( mTopPlayTracks );
			}
		}

		public bool CanExecute_PlayTopTracks() {
			return( mTopPlayTracks.Any());
		}

		[DependsUpon( "Artist" )]
		public bool CanExecute_PlayRandomTracks() {
			return( CurrentArtist != null );
		}

		public IInteractionRequest ArtistEditRequest {
			get{ return( mArtistEditRequest ); }
		}

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
						Mapper.DynamicMap( confirmation.ViewModel, updater.Item );
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

			mEventAggregator.Publish( request );

			ArtistInfoViewOpen = request.ViewWasOpened;
		}

		public bool AlbumInfoViewOpen {
			get{ return( Get( () => AlbumInfoViewOpen )); }
			set{ Set( () => AlbumInfoViewOpen, value ); }
		}

		public void Execute_DisplayAlbumInfoPanel() {
			var request = new Events.ViewDisplayRequest( ViewNames.AlbumInfoView );

			mEventAggregator.Publish( request );

			AlbumInfoViewOpen = request.ViewWasOpened;
		}

		public bool ArtistTracksViewOpen {
			get{ return( Get( () => ArtistTracksViewOpen )); }
			set{ Set( () => ArtistTracksViewOpen, value ); }
		}

		public void Execute_DisplayArtistTracksPanel() {
			var request = new Events.ViewDisplayRequest( ViewNames.ArtistTracksView );

			mEventAggregator.Publish( request );

			ArtistTracksViewOpen = request.ViewWasOpened;
		}
	}
}
