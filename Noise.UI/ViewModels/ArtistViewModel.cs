﻿using AutoMapper;
using Caliburn.Micro;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Adapters;
using Noise.UI.Behaviours;
using Noise.UI.Dto;
using Observal.Extensions;
using ReusableBits;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
	public class ArtistEditRequest : InteractionRequestData<UiArtist> {
		public ArtistEditRequest(UiArtist artist ) : base( artist ) { } 
	}

	public class ArtistViewModel : AutomaticCommandBase,
								   IHandle<Events.ArtistFocusRequested>, IHandle<Events.AlbumFocusRequested>, 
								   IHandle<Events.ArtistContentUpdated>, IHandle<Events.ArtistUserUpdate> {
		private readonly IEventAggregator		mEventAggregator;
		private readonly IArtistProvider		mArtistProvider;
		private readonly IArtworkProvider		mArtworkProvider;
		private readonly ITagManager			mTagManager;
		private readonly Observal.Observer		mChangeObserver;
		private UiArtist						mCurrentArtist;
		private Artwork							mArtistImage;
		private LinkNode						mArtistWebsite;
		private TaskHandler<DbArtist>			mArtistTaskHandler; 
		private TaskHandler<Artwork>			mArtworkTaskHandler; 
		private readonly InteractionRequest<ArtistEditRequest>		mArtistEditRequest;

		public ArtistViewModel( IEventAggregator eventAggregator, IArtistProvider artistProvider, IArtworkProvider artworkProvider, ITagManager tagManager ) {
			mEventAggregator = eventAggregator;
			mArtistProvider = artistProvider;
			mArtworkProvider = artworkProvider;
			mTagManager = tagManager;

			mEventAggregator.Subscribe( this );

			mChangeObserver = new Observal.Observer();
			mChangeObserver.Extend( new PropertyChangedExtension()).WhenPropertyChanges( OnArtistChanged );

			mArtistEditRequest = new InteractionRequest<ArtistEditRequest>(); 
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
			}

			return( retValue );
		}

		private void ClearCurrentArtist() {
			if( mCurrentArtist != null ) {
				mChangeObserver.Release( mCurrentArtist );

				mCurrentArtist = null;
				mArtistWebsite = null;

				RaisePropertyChanged( () => Artist );
				RaisePropertyChanged( () => ArtistWebsite );
			}
		}

		public void ClearCurrentArtistInfo() {
			mArtistImage = null;

			RaisePropertyChanged( () => ArtistImage );
		}

		private void SetCurrentArtist( DbArtist artist ) {
			CurrentArtist = artist != null ? TransformArtist( artist ) : null;
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

		public void Handle( Events.ArtistContentUpdated eventArgs ) {
			if(( CurrentArtist != null ) &&
			   ( CurrentArtist.DbId == eventArgs.ArtistId )) {
				RequestArtist( CurrentArtist.DbId );
			}
		}

		public void Handle( Events.ArtistFocusRequested request ) {
			if( CurrentArtist != null ) {
				if( request.ArtistId != CurrentArtist.DbId ) {
					RequestArtistAndContent( request.ArtistId );
				}
			}
			else {
				RequestArtistAndContent( request.ArtistId );
			}
		}

		public void Handle( Events.AlbumFocusRequested request ) {
			if( CurrentArtist != null ) {
				if( request.ArtistId != CurrentArtist.DbId ) {
					RequestArtistAndContent( request.ArtistId );
				}
			}
			else {
				RequestArtistAndContent( request.ArtistId );
			}
		}

		public void Handle( Events.ArtistUserUpdate eventArgs ) {
			if(( CurrentArtist != null ) &&
			   ( eventArgs.ArtistId == CurrentArtist.DbId )) {
				RequestArtist( CurrentArtist.DbId );
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
			RetrieveArtwork( artistId );
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

		private void RetrieveArtwork( long artistId ) {
			ArtworkTaskHandler.StartTask( () => mArtworkProvider.GetArtistArtwork( artistId, ContentType.ArtistPrimaryImage ),
										   SetArtwork,
										   exception => NoiseLogger.Current.LogException( "ArtistViewModel:GetArtistArtwork", exception ));
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
