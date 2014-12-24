using System;
using AutoMapper;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Dto;
using Noise.UI.Interfaces;
using ReusableBits;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
	public class PlayingArtistViewModel : AutomaticPropertyBase {
		private readonly IEventAggregator		mEventAggregator;
		private readonly IArtistProvider		mArtistProvider;
		private readonly ITagManager			mTagManager;
		private readonly IMetadataManager		mMetadataManager;
		private readonly ISelectionState		mSelectionState;
		private TaskHandler<DbArtist>			mArtistTaskHandler; 
		private TaskHandler<Artwork>			mArtworkTaskHandler; 
		private Artwork							mArtistImage;

		public PlayingArtistViewModel( IEventAggregator eventAggregator, IArtistProvider artistProvider, IMetadataManager metadataManager,
									   ITagManager tagManager, ISelectionState selectionState ) {
			mEventAggregator = eventAggregator;
			mArtistProvider = artistProvider;
			mTagManager = tagManager;
			mMetadataManager = metadataManager;
			mSelectionState = selectionState;

			mSelectionState.CurrentArtistChanged.Subscribe( OnArtistRequested );
			OnArtistRequested( mSelectionState.CurrentArtist );
		}

		public UiArtist CurrentArtist {
			get {  return( Get( () => CurrentArtist )); }
			set {  Set( () => CurrentArtist, value ); }
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

		private void ClearCurrentArtist() {
			CurrentArtist = null;
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

			RequestArtist( artistId );

			mEventAggregator.Publish( new Events.ArtistContentRequest( artistId ));
		}

		private void RequestArtist( long artistId ) {
			RetrieveArtist( artistId );
		}

		private void RetrieveArtist( long artistId ) {
			ArtistTaskHandler.StartTask( () => mArtistProvider.GetArtist( artistId ), 
										SetCurrentArtist,
										exception => NoiseLogger.Current.LogException( "PlayingArtistViewModel:GetArtist", exception ));
		}

		private void SetCurrentArtist( DbArtist artist ) {
			CurrentArtist = artist != null ? TransformArtist( artist ) : null;

			if( CurrentArtist != null ) {
				RetrieveArtwork( CurrentArtist.Name );
			}
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
										   exception => NoiseLogger.Current.LogException( "PlayingArtistViewModel:GetArtistArtwork", exception ));
		}

		private void SetArtwork( Artwork artwork ) {
			mArtistImage = artwork;

			RaisePropertyChanged( () => ArtistImage );
		}
	}
}
