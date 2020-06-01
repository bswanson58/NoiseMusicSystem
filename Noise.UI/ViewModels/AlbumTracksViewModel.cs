using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AutoMapper;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Dto;
using Noise.UI.Interfaces;
using Noise.UI.Logging;
using Noise.UI.Views;
using Observal.Extensions;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReusableBits;
using ReusableBits.ExtensionClasses.MoreLinq;
using ReusableBits.Mvvm.ViewModelSupport;
using IDialogService = Prism.Services.Dialogs.IDialogService;

namespace Noise.UI.ViewModels {
	internal class AlbumTracksViewModel : AutomaticPropertyBase,
										  IHandle<Events.DatabaseClosing>, IHandle<Events.TrackUserUpdate>, IHandle<Events.UserTagsChanged>, IHandle<Events.AlbumStructureChanged> {
		private readonly IEventAggregator				mEventAggregator;
		private readonly IUiLog							mLog;
		private readonly ISelectionState				mSelectionState;
		private readonly ITrackProvider					mTrackProvider;
        private readonly IUserTagManager                mTagManager;
		private readonly IPlayCommand					mPlayCommand;
		private readonly IRatings						mRatings;
        private readonly IPlayingItemHandler            mPlayingItemHandler;
		private readonly IDialogService					mDialogService;
		private readonly Observal.Observer				mChangeObserver;
		private readonly BindableCollection<UiTrack>	mTracks;
        private TaskHandler<IEnumerable<UiTrack>>		mTrackRetrievalTaskHandler;
		private CancellationTokenSource					mCancellationTokenSource;
		private long									mCurrentAlbumId;

        public BindableCollection<UiTrack>              TrackList => mTracks;
        public DelegateCommand                          ClearTrackRatings { get; }
		public UiTrack									PlayingTrack { get; private set; }

		public AlbumTracksViewModel( IEventAggregator eventAggregator, IRatings ratings, ISelectionState selectionState, IPlayingItemHandler playingItemHandler,
									 ITrackProvider trackProvider, IPlayCommand playCommand, IUserTagManager tagManager, IUiLog log, IDialogService dialogService ) {
			mEventAggregator = eventAggregator;
			mLog = log;
			mSelectionState = selectionState;
			mRatings = ratings;
			mTrackProvider = trackProvider;
			mPlayCommand = playCommand;
            mTagManager = tagManager;
            mPlayingItemHandler = playingItemHandler;
			mDialogService = dialogService;

			mEventAggregator.Subscribe( this );

			mTracks = new BindableCollection<UiTrack>();
			mCurrentAlbumId = Constants.cDatabaseNullOid;

			mChangeObserver = new Observal.Observer();
			mChangeObserver.Extend( new PropertyChangedExtension()).WhenPropertyChanges( OnNodeChanged );

            ClearTrackRatings = new DelegateCommand( OnClearRatings );

			mSelectionState.CurrentAlbumChanged.Subscribe( OnAlbumChanged );
            mSelectionState.CurrentAlbumVolumeChanged.Subscribe( OnVolumeChanged );

            mPlayingItemHandler.StartHandler( mTracks, OnPlayingItemChanged );
		}

		private void OnPlayingItemChanged( IPlayingItem item ) {
			if( item is UiTrack track ) {
				if( track.IsPlaying ) {
					PlayingTrack = track;

					RaisePropertyChanged( () => PlayingTrack );
                }
            }
        }

		public void Handle( Events.DatabaseClosing args ) {
			ClearTrackList();
		}

	    public void Handle( Events.UserTagsChanged message ) {
            mTracks.ForEach( SetTrackTags );
        }

		public void Handle( Events.AlbumStructureChanged message ) {
			if( mCurrentAlbumId == message.AlbumId ) {
                ClearTrackList();

				mCurrentAlbumId = message.AlbumId;
                RetrieveTracks( mCurrentAlbumId );
            }
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

		private CancellationToken GenerateCancellationToken() {
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
	
			var cancellationToken = GenerateCancellationToken();

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
			var retValue = new UiTrack( OnTrackPlay, OnTagEdit, OnStrategyOptions, OnFocusRequest );

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
			var targetTrack = mTracks.FirstOrDefault( t => t.DbId.Equals( trackId ));
			var previousTrack = targetTrack;
            var previousTracks = new List<UiTrack>();
			var playList = new List<DbTrack>();

            while(( previousTrack != null ) && 
                 (( previousTrack.PlayAdjacentStrategy == ePlayAdjacentStrategy.PlayPrevious ) ||
				  ( previousTrack.PlayAdjacentStrategy == ePlayAdjacentStrategy.PlayNextPrevious ))) {
				previousTrack = mTracks.TakeWhile( t => !t.DbId.Equals( previousTrack.DbId )).LastOrDefault();

				if( previousTrack != null ) {
					previousTracks.Insert( 0, previousTrack );
                }
            }
			previousTracks.ForEach( t => playList.Add( mTrackProvider.GetTrack( t.DbId )));
            
            playList.Add( mTrackProvider.GetTrack( trackId ));

			while(( targetTrack != null ) &&
                 (( targetTrack.PlayAdjacentStrategy == ePlayAdjacentStrategy.PlayNextPrevious ) ||
				  ( targetTrack.PlayAdjacentStrategy == ePlayAdjacentStrategy.PlayNext ))) {
				targetTrack = mTracks.SkipWhile( t => !t.DbId.Equals( targetTrack.DbId )).Skip( 1 ).FirstOrDefault();

				if( targetTrack != null ) {
					playList.Add( mTrackProvider.GetTrack( targetTrack.DbId ));
                }
            }

			mPlayCommand.Play( playList );
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

        private void OnClearRatings() {
            mTracks.ForEach( track => track.UiRating = 0 );
        }

		public void Handle( Events.TrackUserUpdate eventArgs ) {
			var track = ( from UiTrack node in mTracks where node.DbId == eventArgs.Track.DbId select node ).FirstOrDefault();

			if( track != null ) {
				var newTrack = TransformTrack( eventArgs.Track );

				mChangeObserver.Release( track );
				mTracks[mTracks.IndexOf( track )] = newTrack;
				mChangeObserver.Add( newTrack );

                SetTrackTags( newTrack );
                mPlayingItemHandler.UpdateList();
			}
		}

        public TimeSpan AlbumPlayTime {
			get{ return( Get( () => AlbumPlayTime )); }
			set{ Set( () => AlbumPlayTime, value ); }
		}

        private void OnTagEdit( long trackId ) {
            var track = mTrackProvider.GetTrack( trackId );

            if( track != null ) {
				var parameters = new DialogParameters{{ TagAssociationDialogModel.cTrackParameter, track }};

				mDialogService.ShowDialog( nameof( TagAssociationDialog ), parameters, result => {
					if( result.Result == ButtonResult.OK ) {
                        SetTrackTags( TrackList.FirstOrDefault( t => t.DbId.Equals( track.DbId )));
                    }
                });
            }
        }

        private void OnStrategyOptions( long trackId ) {
            var track = mTrackProvider.GetTrack( trackId );

            if( track != null ) {
				var parameters = new DialogParameters{{ TrackStrategyOptionsDialogModel.cTrackParameter, track }};

				mDialogService.ShowDialog( nameof( TrackStrategyOptionsDialog ), parameters, result => {
					if( result.Result == ButtonResult.OK ) {
						var editedTrack = result.Parameters.GetValue<DbTrack>( TrackStrategyOptionsDialogModel.cTrackParameter );

						if( editedTrack != null ) {
                            using( var updateTrack = mTrackProvider.GetTrackForUpdate( editedTrack.DbId ) ) {
                                if( updateTrack.Item != null ) {
									updateTrack.Item.PlayAdjacentStrategy = editedTrack.PlayAdjacentStrategy;
									updateTrack.Item.DoNotStrategyPlay = editedTrack.DoNotStrategyPlay;

                                    updateTrack.UpdateTrackAndAlbum();
								}
							}

                            TrackList.FirstOrDefault( t => t.DbId.Equals( editedTrack.DbId ))?.SetStrategyOption( editedTrack.PlayAdjacentStrategy, editedTrack.DoNotStrategyPlay );

                            mEventAggregator.PublishOnCurrentThread( new Events.LibraryBackupPressure( 1, "TrackStrategyPlayEdited" ));
                        }
                    }
                });
            }
        }

        private void OnFocusRequest( long trackId ) {
			var track = mTrackProvider.GetTrack( trackId );

			if( track != null ) {
				mSelectionState.RequestFocus( track );
            }
        }
	}
}
