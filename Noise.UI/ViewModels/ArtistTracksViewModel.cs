using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Data;
using AutoMapper;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Dto;
using Noise.UI.Interfaces;
using Noise.UI.Logging;
using Prism;
using ReusableBits;
using ReusableBits.ExtensionClasses;
using ReusableBits.ExtensionClasses.MoreLinq;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
	internal class ArtistTracksViewModel : AutomaticPropertyBase, IActiveAware, IDisposable, IHandle<Events.DatabaseClosing> {
		private readonly IEventAggregator	mEventAggregator;
		private readonly IUiLog				mLog;
		private readonly ISelectionState	mSelectionState;
		private readonly IAlbumProvider		mAlbumProvider;
		private readonly ITrackProvider		mTrackProvider;
		private readonly IPlayCommand		mPlayCommand;
        private readonly BindableCollection<UiArtistTrackNode>	mTrackList;
        private ICollectionView				mTrackView;
		private	bool						mIsActive;
		private TaskHandler<IEnumerable<UiArtistTrackNode>>	mUpdateTask;
		private CancellationTokenSource						mCancellationTokenSource;

		public	bool									IsListFiltered => !String.IsNullOrWhiteSpace( FilterText );
		public	event EventHandler						IsActiveChanged = delegate { };

        public ArtistTracksViewModel( IEventAggregator eventAggregator, ISelectionState selectionState, IPlayCommand playCommand,
									  IAlbumProvider albumProvider, ITrackProvider trackProvider, IUiLog log ) {
			mEventAggregator = eventAggregator;
			mLog = log;
			mSelectionState = selectionState;
			mAlbumProvider = albumProvider;
			mTrackProvider = trackProvider;
			mPlayCommand = playCommand;

			mEventAggregator.Subscribe( this );

			mTrackList = new BindableCollection<UiArtistTrackNode>();
			TracksValid = false;

			mSelectionState.CurrentArtistChanged.Subscribe( OnArtistChanged );
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

		public bool TracksValid {
			get{ return( Get( () => TracksValid )); }
			set{ Set( () => TracksValid, value ); }
		}

		public void Handle( Events.DatabaseClosing args ) {
			mTrackList.Clear();
			TracksValid = false;
		}

		private void OnArtistChanged( DbArtist artist ) {
			if( IsActive ) {
				if( artist != null ) {
					UpdateTrackList( artist );
				}
				else {
					ClearTrackList();
				}
			}
			else {
				ClearTrackList();
			}
		}

		private void ClearTrackList() {
			CancelRetrievalTask();
			mTrackList.Clear();
			TracksValid = false;

			AlbumCount = 0;
			UniqueTrackCount = 0;
			FilterText = String.Empty;
		}

		internal TaskHandler<IEnumerable<UiArtistTrackNode>>  UpdateTask {
			get {
				if( mUpdateTask == null ) {
					Execute.OnUIThread( () => mUpdateTask = new TaskHandler<IEnumerable<UiArtistTrackNode>>());
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
			TracksValid = false;
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

        public ICollectionView TrackList {
            get{ 
                if( mTrackView == null ) {
                    mTrackView = CollectionViewSource.GetDefaultView( mTrackList );

                    mTrackView.Filter += OnTrackFilter;
                }

                return( mTrackView );
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
               ( node is UiArtistTrackNode trackNode )) {
                if( trackNode.TrackName.IndexOf( FilterText, StringComparison.OrdinalIgnoreCase ) == -1 ) {
                    retValue = false;
                }
            }

            return ( retValue );
        }

        public int AlbumCount {
			get{ return( Get( () => AlbumCount )); }
			set{ Set( () => AlbumCount, value ); }
		}

		public int TrackCount {
			get { return( Get( () => TrackCount )); }
			set { Set( () => TrackCount, value ); }
		}

		public int UniqueTrackCount {
			get { return( Get( () => UniqueTrackCount )); }
			set { Set( () => UniqueTrackCount, value ); }
		}

		private IEnumerable<UiArtistTrackNode> BuildTrackList( DbArtist forArtist, CancellationToken cancellationToken ) {
			var trackSet = new Dictionary<string, UiArtistTrackNode>();
			var albumList = new Dictionary<long, DbAlbum>();

			AlbumCount = 0;
			TrackCount = 0;
			UniqueTrackCount = 0;

			using( var albums = mAlbumProvider.GetAlbumList( forArtist.DbId )) {
				albums.List.ForEach( album => albumList.Add( album.DbId, album ));
			}

			AlbumCount = albumList.Count;

			using( var trackList = mTrackProvider.GetTrackList( forArtist )) {
				foreach( var track in trackList.List ) {
					if((!cancellationToken.IsCancellationRequested ) &&
					   ( albumList.ContainsKey( track.Album ))) {
						var uiTrack = TransformTrack( track );
						var dbAlbum = albumList[track.Album];
						var key = track.Name.RemoveSpecialCharacters().ToLower();

                        trackSet.TryGetValue( key, out var existing );

						if( existing != null ) {
							existing.AddAlbum( dbAlbum, uiTrack );
						}
						else {
							trackSet.Add( key, new UiArtistTrackNode( dbAlbum, uiTrack ));

							UniqueTrackCount++;
						}

						TrackCount++;
					}
					else {
						break;
					}
				}
			}

			return( trackSet.Values );
		}

		private void SetTrackList( IEnumerable<UiArtistTrackNode> list ) {
			mTrackList.Clear();
			mTrackList.AddRange( from node in list orderby node.TrackName ascending select node );

			TracksValid = true;
		}

		private UiTrack TransformTrack( DbTrack dbTrack ) {
			var retValue = new UiTrack( OnTrackPlay, null, null );

			Mapper.Map( dbTrack, retValue );

			return( retValue );
		}

		private void OnTrackPlay( long trackId ) {
			mPlayCommand.Play( mTrackProvider.GetTrack( trackId ));
		}

        public void Dispose() {
            mCancellationTokenSource?.Dispose();
			mEventAggregator?.Unsubscribe( this );
        }
    }
}
