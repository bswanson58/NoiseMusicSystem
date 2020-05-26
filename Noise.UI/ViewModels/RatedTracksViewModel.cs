using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Data;
using Caliburn.Micro;
using Microsoft.Practices.Prism;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Dto;
using Noise.UI.Interfaces;
using Noise.UI.Logging;
using ReusableBits;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
    class RatedTracksViewModel : AutomaticPropertyBase, IActiveAware, IDisposable,
                                                IHandle<Events.DatabaseClosing> {
        private readonly IEventAggregator	    mEventAggregator;
        private readonly IUiLog				    mLog;
        private readonly ISelectionState	    mSelectionState;
        private readonly IAlbumProvider		    mAlbumProvider;
        private readonly ITrackProvider		    mTrackProvider;
        private readonly IPlayingItemHandler    mPlayingItemHandler;
        private readonly IPlayCommand		    mPlayCommand;
        private readonly BindableCollection<UiTrackAlbum>	mTrackList;
        private ICollectionView				    mTrackView;
        private	bool						    mIsActive;
        private DbArtist                        mCurrentArtist;
        private TaskHandler<IEnumerable<UiTrackAlbum>>      mUpdateTask;
        private CancellationTokenSource					    mCancellationTokenSource;

        public	bool									    IsListFiltered => !String.IsNullOrWhiteSpace( FilterText );
        public  string                                      ListCount => mTrackList.Any() ? $"{mTrackList.Count}" : "No";
        public  string                                      ArtistName => mCurrentArtist != null ? mCurrentArtist.Name : String.Empty;
        public	event EventHandler						    IsActiveChanged = delegate { };

        public RatedTracksViewModel( IEventAggregator eventAggregator, ISelectionState selectionState, IPlayCommand playCommand, IPlayingItemHandler playingItemHandler,
                                      IAlbumProvider albumProvider, ITrackProvider trackProvider, IUiLog log ) {
            mEventAggregator = eventAggregator;
            mLog = log;
            mSelectionState = selectionState;
            mAlbumProvider = albumProvider;
            mTrackProvider = trackProvider;
            mPlayCommand = playCommand;
            mPlayingItemHandler = playingItemHandler;

            mEventAggregator.Subscribe( this );

            mTrackList = new BindableCollection<UiTrackAlbum>();

            mSelectionState.CurrentArtistChanged.Subscribe( OnArtistChanged );
            mPlayingItemHandler.StartHandler( mTrackList );
        }

        public bool IsActive {
            get => ( mIsActive );
            set {
                mIsActive = value;

                if( mIsActive ) {
                    if( mSelectionState.CurrentArtist != null ) {
                        UpdateTrackList( mSelectionState.CurrentArtist );
                    }
                }
                else {
                    CancelRetrievalTask();
                    mTrackList.Clear();
                }

                IsActiveChanged( this, new EventArgs());
            }
        }

        private void OnArtistChanged( DbArtist artist ) {
            mCurrentArtist = artist;

            if( IsActive ) {
                if( mCurrentArtist != null ) {
                    UpdateTrackList( artist );
                }
                else {
                    ClearTrackList();
                }
            }
            else {
                ClearTrackList();
            }

            RaisePropertyChanged( () => ArtistName );
        }

        private void ClearTrackList() {
            CancelRetrievalTask();
            mTrackList.Clear();

            FilterText = String.Empty;
            RaisePropertyChanged( () => ListCount );
        }

        public void Handle( Events.DatabaseClosing args ) {
            mTrackList.Clear();
        }

        public ICollectionView TrackList {
            get{ 
                if( mTrackView == null ) {
                    mTrackView = CollectionViewSource.GetDefaultView( mTrackList );

                    mTrackView.Filter += OnTrackFilter;
                }

                return mTrackView;
            }
        }

        public string FilterText {
            get { return( Get(() => FilterText )); }
            set {
                Set(() => FilterText, value );

                mTrackView?.Refresh();
                RaisePropertyChanged( () => IsListFiltered );
            }
        }

        private bool OnTrackFilter( object node ) {
            var retValue = true;

            if((!string.IsNullOrWhiteSpace( FilterText )) &&
               ( node is UiTrackAlbum trackNode )) {
                if( trackNode.TrackName.IndexOf( FilterText, StringComparison.OrdinalIgnoreCase ) == -1 ) {
                    retValue = false;
                }
            }

            return retValue;
        }

		internal TaskHandler<IEnumerable<UiTrackAlbum>>  UpdateTask {
			get {
				if( mUpdateTask == null ) {
					Execute.OnUIThread( () => mUpdateTask = new TaskHandler<IEnumerable<UiTrackAlbum>>());
				}

				return( mUpdateTask );
			}
			set => mUpdateTask = value;
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

		private void UpdateTrackList( DbArtist artist ) {
			CancelRetrievalTask();
			FilterText = String.Empty;

			var cancellationToken = GenerateCanellationToken();

			if(( artist != null ) &&
			   ( mIsActive )) {
				UpdateTask.StartTask( () => BuildTrackList( artist, cancellationToken ), 
									  SetTrackList,
									  ex => mLog.LogException( $"UpdateTrackList for {artist}", ex ),
									  cancellationToken );
			}
		}

        private IEnumerable<UiTrackAlbum> BuildTrackList( DbArtist forArtist, CancellationToken cancellationToken ) {
            var retValue = new List<UiTrackAlbum>();

            using( var trackList = mTrackProvider.GetRatedTracks( forArtist )) {
                foreach( var track in trackList.List ) {
                    if(!cancellationToken.IsCancellationRequested ) {
                        var album = mAlbumProvider.GetAlbum( track.Album );

                        if( album != null ) {
                            retValue.Add( new UiTrackAlbum( album, track, OnTrackPlay ));
                        }
                    }
                    else {
                        break;
                    }
                }
            }

            return retValue;
        }

        private void SetTrackList( IEnumerable<UiTrackAlbum> list ) {
            mTrackList.Clear();
            mTrackList.AddRange( from node in list orderby node.SortRating descending, node.TrackName select node );

            mPlayingItemHandler.UpdateList();
            RaisePropertyChanged( () => ListCount );
        }

        private void OnTrackPlay( long trackId ) {
            mPlayCommand.Play( mTrackProvider.GetTrack( trackId ));
        }

        public void Dispose() {
            mCancellationTokenSource?.Dispose();
            mEventAggregator?.Unsubscribe( this );
            mPlayingItemHandler.StopHandler();
        }
    }
}
