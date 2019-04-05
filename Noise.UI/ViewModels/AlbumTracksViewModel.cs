using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AutoMapper;
using Caliburn.Micro;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Behaviours;
using Noise.UI.Dto;
using Noise.UI.Interfaces;
using Noise.UI.Logging;
using Observal.Extensions;
using ReusableBits;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
    internal class TagEditInfo : InteractionRequestData<TagAssociationDialogModel> {
        public TagEditInfo( TagAssociationDialogModel viewModel ) : base( viewModel ) { }
    }

	internal class AlbumTracksViewModel : AutomaticPropertyBase,
										  IHandle<Events.DatabaseClosing>, IHandle<Events.TrackUserUpdate>, IHandle<Events.UserTagsChanged> {
		private readonly IEventAggregator				mEventAggregator;
		private readonly IUiLog							mLog;
		private readonly ISelectionState				mSelectionState;
		private readonly ITrackProvider					mTrackProvider;
        private readonly IUserTagManager                mTagManager;
		private readonly IPlayCommand					mPlayCommand;
		private readonly IRatings						mRatings;
        private readonly IPlayingItemHandler            mPlayingItemHandler;
		private readonly Observal.Observer				mChangeObserver;
		private readonly BindableCollection<UiTrack>	mTracks;
        private TaskHandler<IEnumerable<UiTrack>>		mTrackRetrievalTaskHandler;
		private CancellationTokenSource					mCancellationTokenSource;
		private long									mCurrentAlbumId;

        public BindableCollection<UiTrack>              TrackList => mTracks;
        public InteractionRequest<TagEditInfo>          TagEditRequest { get; }

		public AlbumTracksViewModel( IEventAggregator eventAggregator, IRatings ratings, ISelectionState selectionState, IPlayingItemHandler playingItemHandler,
									 ITrackProvider trackProvider, IPlayCommand playCommand, IUserTagManager tagManager, IUiLog log ) {
			mEventAggregator = eventAggregator;
			mLog = log;
			mSelectionState = selectionState;
			mRatings = ratings;
			mTrackProvider = trackProvider;
			mPlayCommand = playCommand;
            mTagManager = tagManager;
            mPlayingItemHandler = playingItemHandler;

			mEventAggregator.Subscribe( this );

			mTracks = new BindableCollection<UiTrack>();
			mCurrentAlbumId = Constants.cDatabaseNullOid;

			mChangeObserver = new Observal.Observer();
			mChangeObserver.Extend( new PropertyChangedExtension()).WhenPropertyChanges( OnNodeChanged );

            TagEditRequest = new InteractionRequest<TagEditInfo>();

			mSelectionState.CurrentAlbumChanged.Subscribe( OnAlbumChanged );
            mSelectionState.CurrentAlbumVolumeChanged.Subscribe( OnVolumeChanged );

            mPlayingItemHandler.StartHandler( mTracks );
		}

		public void Handle( Events.DatabaseClosing args ) {
			ClearTrackList();
		}

	    public void Handle( Events.UserTagsChanged message ) {
            mTracks.ForEach( SetTrackTags );
        }

		private void OnAlbumChanged( DbAlbum album ) {
			if( album != null ) {
				UpdateTrackList( album.DbId );
			}
			else {
				ClearTrackList();
			}
		}

        private void OnVolumeChanged( string volumeName ) {
            if(!string.IsNullOrWhiteSpace( volumeName )) {
                foreach( var track in mTracks ) {
                    track.IsHighlighted = volumeName.Equals( track.VolumeName );
                }
            }
            else {
                foreach( var track in mTracks ) {
                    track.IsHighlighted = false;
                }
            }
        }

		private void UpdateTrackList( long albumId ) {
			if( albumId == Constants.cDatabaseNullOid ) {
				ClearTrackList();
			}
			else {
				if( mCurrentAlbumId != albumId ) {
					ClearTrackList();

					RetrieveTracks( albumId );
				}
			}
		}

		internal TaskHandler<IEnumerable<UiTrack>>  TracksRetrievalTaskHandler {
			get {
				if( mTrackRetrievalTaskHandler == null ) {
					Execute.OnUIThread( () => mTrackRetrievalTaskHandler = new TaskHandler<IEnumerable<UiTrack>> ());
				}

				return( mTrackRetrievalTaskHandler );
			}
			set => mTrackRetrievalTaskHandler = value;
        }

		private CancellationToken GenerateCanellationToken() {
			mCancellationTokenSource = new CancellationTokenSource();

			return( mCancellationTokenSource.Token );
		}

		private void CancelRetrievalTask() {
			if( mCancellationTokenSource != null ) {
				mCancellationTokenSource.Cancel();
				mCancellationTokenSource = null;
			}
		}

		private void ClearCurrentTask() {
			mCancellationTokenSource = null;
		}

		private void RetrieveTracks( long forAlbumId ) {
			CancelRetrievalTask();
	
			var cancellationToken = GenerateCanellationToken();

			TracksRetrievalTaskHandler.StartTask( () => LoadTracks( forAlbumId, cancellationToken ),
												albumList => UpdateUi( albumList, forAlbumId ),
												ex => mLog.LogException( $"Retrieve tracks for {forAlbumId}", ex ),
												cancellationToken );
		}

		private IEnumerable<UiTrack> LoadTracks( long albumId, CancellationToken cancellationToken ) {
			var retValue = new List<UiTrack>();

			using( var tracks = mTrackProvider.GetTrackList( albumId )) {
				if(!cancellationToken.IsCancellationRequested ) {
					var sortedList = new List<DbTrack>( from DbTrack track in tracks.List orderby track.VolumeName, track.TrackNumber select track );

					retValue.AddRange( sortedList.Select( TransformTrack ));
                    retValue.ForEach( SetTrackTags );
				}
			}

			return( retValue );
		}

		private void UpdateUi( IEnumerable<UiTrack> trackList, long albumId ) {
			ClearTrackList();

			mTracks.AddRange( trackList );
			mCurrentAlbumId = albumId;
			AlbumPlayTime = TimeSpan.FromSeconds( mTracks.Sum( track => track.Duration.TotalSeconds ));

			foreach( var track in mTracks ) {
				mChangeObserver.Add( track );
			}

			ClearCurrentTask();
            mPlayingItemHandler.UpdateList();
		}

		private UiTrack TransformTrack( DbTrack dbTrack ) {
			var retValue = new UiTrack( OnTrackPlay, OnTagEdit  );

			if( dbTrack != null ) {
				Mapper.Map( dbTrack, retValue );
			}

			return( retValue );
		}

        private void SetTrackTags( UiTrack track ) {
            track?.SetTags( from tag in mTagManager.GetAssociatedTags( track.DbId ) orderby tag.Name select tag.Name );
        }

		private void ClearTrackList() {
			foreach( var track in mTracks ) {
				mChangeObserver.Release( track );
			}

			mTracks.Clear();
			mCurrentAlbumId = Constants.cDatabaseNullOid;

			AlbumPlayTime = new TimeSpan();
		}

		private void OnTrackPlay( long trackId ) {
			mPlayCommand.Play( mTrackProvider.GetTrack( trackId ));
		}

		private void OnNodeChanged( PropertyChangeNotification propertyNotification ) {
			if( propertyNotification.Source is UiBase item ) {
                var	track= mTrackProvider.GetTrack( item.DbId );

				if( track != null ) {
					if( propertyNotification.PropertyName == "UiRating" ) {
						mRatings.SetRating( track, item.UiRating );
					}
					if( propertyNotification.PropertyName == "UiIsFavorite" ) {
						mRatings.SetFavorite( track, item.UiIsFavorite );
					}
				}
			}
		}

		public void Handle( Events.TrackUserUpdate eventArgs ) {
			var track = ( from UiTrack node in mTracks where node.DbId == eventArgs.Track.DbId select node ).FirstOrDefault();

			if( track != null ) {
				var newTrack = TransformTrack( eventArgs.Track );

				mChangeObserver.Release( track );
				mTracks[mTracks.IndexOf( track )] = newTrack;
				mChangeObserver.Add( newTrack );
			}
		}

        public TimeSpan AlbumPlayTime {
			get{ return( Get( () => AlbumPlayTime )); }
			set{ Set( () => AlbumPlayTime, value ); }
		}

        private void OnTagEdit( long trackId ) {
            var track = mTrackProvider.GetTrack( trackId );

            if( track != null ) {
                var dialogModel = new TagAssociationDialogModel( track, mTagManager.GetUserTagList(), mTagManager.GetAssociatedTags( track.DbId ));
				
                TagEditRequest.Raise( new TagEditInfo( dialogModel ), OnTagEdited );
            }
        }

        private void OnTagEdited( TagEditInfo confirmation ) {
            if( confirmation.Confirmed ) {
                mTagManager.UpdateAssociations( confirmation.ViewModel.Track.DbId, confirmation.ViewModel.GetSelectedTags());

                SetTrackTags( TrackList.FirstOrDefault( t => t.DbId.Equals( confirmation.ViewModel.Track.DbId )));
            }
        }
	}
}
