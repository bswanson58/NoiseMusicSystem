using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AutoMapper;
using Caliburn.Micro;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Prism;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Dto;
using Noise.UI.Interfaces;
using Noise.UI.Logging;
using ReusableBits;

namespace Noise.UI.ViewModels {
	internal class ArtistTracksViewModel : ViewModelBase, IActiveAware,
											IHandle<Events.DatabaseClosing> {
		private readonly IEventAggregator	mEventAggregator;
		private readonly IUiLog				mLog;
		private readonly ISelectionState	mSelectionState;
		private readonly IAlbumProvider		mAlbumProvider;
		private readonly ITrackProvider		mTrackProvider;
		private	bool						mIsActive;
		private TaskHandler<IEnumerable<UiArtistTrackNode>>	mUpdateTask;
		private CancellationTokenSource						mCancellationTokenSource;

		public	event EventHandler						IsActiveChanged = delegate { };
		public	BindableCollection<UiArtistTrackNode>	TrackList { get; private set; }

		public ArtistTracksViewModel( IEventAggregator eventAggregator, ISelectionState selectionState,
									  IAlbumProvider albumProvider, ITrackProvider trackProvider, IUiLog log ) {
			mEventAggregator = eventAggregator;
			mLog = log;
			mSelectionState = selectionState;
			mAlbumProvider = albumProvider;
			mTrackProvider = trackProvider;

			mEventAggregator.Subscribe( this );

			TrackList = new BindableCollection<UiArtistTrackNode>();
			TracksValid = false;

			mSelectionState.CurrentArtistChanged.Subscribe( OnArtistChanged );
		}

		public bool IsActive {
			get { return( mIsActive ); }
			set {
				mIsActive = value;

				if( mIsActive ) {
					if( mSelectionState.CurrentArtist != null ) {
						UpdateTrackList( mSelectionState.CurrentArtist );
					}
				}
				else {
					CancelRetrievalTask();
					TrackList.Clear();
				}

				IsActiveChanged( this, new EventArgs());
			}
		}

		public bool TracksValid {
			get{ return( Get( () => TracksValid )); }
			set{ Set( () => TracksValid, value ); }
		}

		public void Handle( Events.DatabaseClosing args ) {
			TrackList.Clear();
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
			TrackList.Clear();
			TracksValid = false;

			AlbumCount = 0;
			UniqueTrackCount = 0;
		}

		internal TaskHandler<IEnumerable<UiArtistTrackNode>>  UpdateTask {
			get {
				if( mUpdateTask == null ) {
					Execute.OnUIThread( () => mUpdateTask = new TaskHandler<IEnumerable<UiArtistTrackNode>>());
				}

				return( mUpdateTask );
			}
			set{ mUpdateTask = value; }
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

			var cancellationToken = GenerateCanellationToken();

			if(( artist != null ) &&
			   ( mIsActive )) {
				UpdateTask.StartTask( () => BuildTrackList( artist, cancellationToken ), 
									  SetTrackList,
									  ex => mLog.LogException( string.Format( "UpdateTrackList for {0}", artist ), ex ),
									  cancellationToken );
			}
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
					if(!cancellationToken.IsCancellationRequested ) {
						var item = new UiArtistTrackNode( TransformTrack( track ),
														  TransformAlbum( albumList[track.Album]));
						UiArtistTrackNode	parent;
						trackSet.TryGetValue( item.Track.Name.ToLower(), out parent );

						if( parent != null ) {
							if(!parent.Children.Any()) {
								parent.Children.Add( new UiArtistTrackNode( parent.Track, parent.Album ));
							}
							parent.Children.Add( item );
						}
						else {
							item.Level = 1;
							trackSet.Add( item.Track.Name.ToLower(), item );

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
			TrackList.Clear();
			TrackList.AddRange( from node in list orderby node.Track.Name ascending select node );

			TracksValid = true;
		}

		private UiAlbum TransformAlbum( DbAlbum dbAlbum ) {
			var retValue = new UiAlbum();

			Mapper.DynamicMap( dbAlbum, retValue );
//			retValue.DisplayGenre = mTagManager.GetGenre( dbAlbum.Genre );

			return( retValue );
		}

		private UiTrack TransformTrack( DbTrack dbTrack ) {
			var retValue = new UiTrack( OnTrackPlay, null );

			Mapper.DynamicMap( dbTrack, retValue );

			return( retValue );
		}

		private void OnTrackPlay( long trackId ) {
			GlobalCommands.PlayTrack.Execute( mTrackProvider.GetTrack( trackId ));
		}
	}
}
