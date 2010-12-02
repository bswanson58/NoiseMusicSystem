using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;

namespace Noise.Core.PlayQueue {
	internal class PlayQueueMgr : IPlayQueue {
		private readonly IUnityContainer					mContainer;
		private readonly IDataProvider						mDataProvider;
		private readonly IEventAggregator					mEventAggregator;
		private readonly List<PlayQueueTrack>				mPlayQueue;
		private readonly List<PlayQueueTrack>				mPlayHistory;
		private	ePlayStrategy								mPlayStrategy;
		private IPlayStrategy								mStrategy;
		private ePlayExhaustedStrategy						mPlayExhaustedStrategy;
		private long										mPlayExhaustedItem;
		private IPlayExhaustedStrategy						mExhaustedStrategy;
		private	int											mReplayTrackCount;
		private PlayQueueTrack								mReplayTrack;
		private readonly AsyncCommand<DbTrack>				mTrackPlayCommand;
		private readonly AsyncCommand<IEnumerable<DbTrack>>	mTrackListPlayCommand;
		private readonly AsyncCommand<DbAlbum>				mAlbumPlayCommand;
		private readonly AsyncCommand<DbInternetStream>		mStreamPlayCommand;

		public PlayQueueMgr( IUnityContainer container ) {
			mContainer = container;
			mDataProvider = mContainer.Resolve<IDataProvider>();
			mEventAggregator = mContainer.Resolve<IEventAggregator>();

			mPlayQueue = new List<PlayQueueTrack>();
			mPlayHistory = new List<PlayQueueTrack>();

			PlayStrategy = ePlayStrategy.Next;
			SetPlayExhaustedStrategy( ePlayExhaustedStrategy.Stop, Constants.cDatabaseNullOid );

			mTrackPlayCommand = new AsyncCommand<DbTrack>( OnTrackPlayCommand );
			GlobalCommands.PlayTrack.RegisterCommand( mTrackPlayCommand );

			mTrackListPlayCommand = new AsyncCommand<IEnumerable<DbTrack>>( OnTrackListPlayCommand );
			GlobalCommands.PlayTrackList.RegisterCommand( mTrackListPlayCommand );

			mAlbumPlayCommand = new AsyncCommand<DbAlbum>( OnAlbumPlayCommand );
			GlobalCommands.PlayAlbum.RegisterCommand( mAlbumPlayCommand );

			mStreamPlayCommand = new AsyncCommand<DbInternetStream>( OnStreamPlayCommand );
			GlobalCommands.PlayStream.RegisterCommand( mStreamPlayCommand );
		}

		private void OnTrackListPlayCommand( IEnumerable<DbTrack> trackList ) {
			foreach( var track in trackList ) {
				Add( track );
			}	
		}

		private void OnTrackPlayCommand( DbTrack track ) {
			Add( track );
		}

		public void Add( DbTrack track ) {
			AddTrack( track, eStrategySource.User );

			FirePlayQueueChanged();
		}

		public void StrategyAdd( DbTrack track ) {
			AddTrack( track, eStrategySource.ExhaustedStrategy );

			FirePlayQueueChanged();
		}

		private void AddTrack( DbTrack track, eStrategySource strategySource ) {
			var album = mDataProvider.GetAlbumForTrack( track );

			if( album != null ) {
				var artist = mDataProvider.GetArtistForAlbum( album );

				if( artist != null ) {
					var file = mDataProvider.GetPhysicalFile( track );

					if( file != null ) {
						var path = mDataProvider.GetPhysicalFilePath( file );
						var newTrack = new PlayQueueTrack( artist, album, track, file, path, strategySource );

						// Place any user selected tracks before any unplayed strategy queued tracks.
						if( strategySource == eStrategySource.User ) {
							var	ptrack = mPlayQueue.Find( t => t.HasPlayed == false && t.IsPlaying == false && t.StrategySource == eStrategySource.ExhaustedStrategy );
							if( ptrack != null ) {
								mPlayQueue.Insert( mPlayQueue.IndexOf( ptrack ), newTrack );
							}
							else {
								mPlayQueue.Add( newTrack );
							}
						}
						else {
							mPlayQueue.Add( newTrack );
						}
					}
				}
			}
		}

		public void StrategyAdd( DbTrack track, PlayQueueTrack afterTrack ) {
			if(( track != null ) &&
			   ( afterTrack != null )) { 
				var album = mDataProvider.GetAlbumForTrack( track );

				if( album != null ) {
					var artist = mDataProvider.GetArtistForAlbum( album );

					if( artist != null ) {
						var file = mDataProvider.GetPhysicalFile( track );

						if( file != null ) {
							var path = mDataProvider.GetPhysicalFilePath( file );
							var newTrack = new PlayQueueTrack( artist, album, track, file, path, eStrategySource.PlayStrategy );
							var trackIndex = mPlayQueue.IndexOf( afterTrack );

							mPlayQueue.Insert( trackIndex + 1, newTrack );

							FirePlayQueueChanged();
						}
					}
				}
			}
		}

		private void OnAlbumPlayCommand( DbAlbum album ) {
			Add( album );
		}

		public void Add( DbAlbum album ) {
			AddAlbum( album );

			FirePlayQueueChanged();
		}

		private void AddAlbum( DbAlbum album ) {
			using( var tracks = mDataProvider.GetTrackList( album )) {
				var firstTrack = true;

				foreach( DbTrack track in tracks.List ) {
					AddTrack( track, eStrategySource.User );

					if( firstTrack ) {
						FirePlayQueueChanged();

						firstTrack = false;
					}
				}
			}
		}

		public void Add( DbArtist artist ) {
			using( var albums = mDataProvider.GetAlbumList( artist )) {
				foreach( DbAlbum album in albums.List ) {
					Add( album );
				}
			}
		}

		private void OnStreamPlayCommand( DbInternetStream stream ) {
			Add( stream  );
		}

		public void Add( DbInternetStream stream ) {
			mPlayQueue.Add( new PlayQueueTrack( stream ));

			FirePlayQueueChanged();
		}

		public void ClearQueue() {
			mPlayQueue.Clear();
			mPlayHistory.Clear();
			PlayingTrackReplayCount = 0;

			FirePlayQueueChanged();
		}

		public bool IsQueueEmpty {
			get{ return( mPlayQueue.Count == 0 ); }
		}

		public bool IsTrackQueued( DbTrack track ) {
			return( mPlayQueue.Exists( item => item.Track.DbId == track.DbId ));
		}

		public void ReplayQueue() {
			foreach( var track in mPlayQueue ) {
				if(!track.IsPlaying ) {
					track.HasPlayed = false;
				}
			}
		}

		public void RemoveTrack( PlayQueueTrack track ) {
			if( track.Track !=null ) {
				var	queuedTrack= mPlayQueue.Find( item => item.Track != null ? item.Track.DbId == track.Track.DbId : false );

				if( queuedTrack != null ) {
					mPlayQueue.Remove(  queuedTrack );
				}

				queuedTrack= mPlayHistory.Find( item => item.Track != null ? item.Track.DbId == track.Track.DbId : false );

				if( queuedTrack != null ) {
					mPlayHistory.Remove(  queuedTrack );
				}
			}

			FirePlayQueueChanged();
		}

		public void	ReorderQueueItem( int fromIndex, int toIndex ) {
			if(( fromIndex < mPlayQueue.Count ) &&
			   ( toIndex < mPlayQueue.Count )) {
				var track = mPlayQueue[fromIndex];

				mPlayQueue.Remove( track );
				mPlayQueue.Insert( toIndex, track );

				var playingTrack = PlayingTrack;
				if( playingTrack != null ) {
					var playingIndex = mPlayQueue.IndexOf( playingTrack );

					if( playingIndex > toIndex ) {
						track.HasPlayed = true;
					}
					if( playingIndex < toIndex ) {
						track.HasPlayed = false;
					}
					if( playingIndex == toIndex ) {
						foreach( var queuedTrack in mPlayQueue ) {
							queuedTrack.HasPlayed = mPlayQueue.IndexOf( queuedTrack ) < playingIndex;
						}
					}
				}

				FirePlayQueueChanged();
			}
		}

		public int UnplayedTrackCount {
			get { return(( from PlayQueueTrack track in mPlayQueue where !track.HasPlayed select track ).Count()); }
		}

		public bool StrategyRequestsQueued {
			get{ return(( from PlayQueueTrack track in mPlayQueue where track.IsStrategyQueued select track ).Count() > 0 ); }
		}

		public int PlayingTrackReplayCount {
			get{ return( mReplayTrackCount ); }
			set {
				if( value > 0 ) {
					if( mReplayTrack == null ) {
						mReplayTrack = PlayingTrack;
					}
					if( mReplayTrack != null ) {
						mReplayTrackCount = value;
					}
				}
				else {
					mReplayTrack = null;
					mReplayTrackCount = 0;
				}
			}
		}

		public PlayQueueTrack PlayNextTrack() {
			var	track = PlayingTrack;

			if( track != null ) {
				track.HasPlayed = true;
				track.IsPlaying = false;

				mPlayHistory.Add( track );
			}

			track = NextTrack;
			if( track != null ) {
				track.IsPlaying = true;

				if( PlayingTrackReplayCount > 0 ) {
					PlayingTrackReplayCount--;
				}
			}

			mExhaustedStrategy.NextTrackPlayed();

			return( track );
		}

		public void StopPlay() {
			var track = PlayingTrack;

			if( track != null ) {
				track.IsPlaying = false;
			}
		}

		public PlayQueueTrack NextTrack {
			get {
				PlayQueueTrack	retValue = null;

				if( PlayingTrackReplayCount > 0 ) {
					retValue = mReplayTrack;
				}

				if( retValue == null ) {
					retValue = mStrategy.NextTrack( this, mPlayQueue );

					if(( retValue == null ) &&
					   ( mPlayQueue.Count > 0 )) {
						if( mExhaustedStrategy.QueueExhausted( this, mPlayExhaustedItem )) {
							retValue = mStrategy.NextTrack( this, mPlayQueue );
						}
					}
				}

				return( retValue );
			}
		}

		public PlayQueueTrack PlayPreviousTrack() {
			var	track = PlayingTrack;

			if( track != null ) {
				track.IsPlaying = false;
			}

			track = PreviousTrack;
			if( track != null ) {
				track.HasPlayed = false;
				track.IsPlaying = true;

				mPlayHistory.Remove( mPlayHistory.Last());
			}

			return( track );
		}

		public PlayQueueTrack PreviousTrack {
			get {
				PlayQueueTrack	retValue = null;
				
				if( mPlayHistory.Count > 0 ) {
					retValue = mPlayHistory.Last();
				}

				return( retValue );
			}
		}

		public PlayQueueTrack PlayingTrack {
			get { return( mPlayQueue.FirstOrDefault( track => ( track.IsPlaying ))); }
			set {
				var currentTrack = mPlayQueue.FirstOrDefault( track => track.IsPlaying );

				if(( currentTrack != value ) &&
				   ( value != null ) &&
				   ( mPlayQueue.IndexOf( value ) != -1 )) {
					if( currentTrack != null ) {
						currentTrack.IsPlaying = false;
						currentTrack.HasPlayed = true;
					}

					value.IsPlaying = true;
					value.HasPlayed = false;
				}
			}
		}

		public ePlayStrategy PlayStrategy {
			get { return( mPlayStrategy ); }
			set {
				mPlayStrategy = value;

				switch( mPlayStrategy ) {
					case ePlayStrategy.Next:
						mStrategy = new PlayStrategySingle();
						break;

					case ePlayStrategy.Random:
						mStrategy = new	PlayStrategyRandom();
						break;

					case ePlayStrategy.TwoFers:
						mStrategy = new PlayStrategyTwoFers( mContainer );
						break;
				}
			}
		}

		public ePlayExhaustedStrategy PlayExhaustedStrategy {
			get { return( mPlayExhaustedStrategy ); }
		}

		public void SetPlayExhaustedStrategy( ePlayExhaustedStrategy strategy, long itemId ) {
			mPlayExhaustedStrategy = strategy;
			mPlayExhaustedItem = itemId;

			switch( mPlayExhaustedStrategy ) {
				case ePlayExhaustedStrategy.Stop:
					mExhaustedStrategy = new PlayExhaustedStrategyStop();
					break;

				case ePlayExhaustedStrategy.Replay:
					mExhaustedStrategy = new PlayQueueExhaustedStrategyReplay();
					break;

				case ePlayExhaustedStrategy.PlayList:
					mExhaustedStrategy = new PlayExhaustedStrategyPlayList( mContainer );
					break;

				case ePlayExhaustedStrategy.PlayFavorites:
					mExhaustedStrategy = new PlayExhaustedStrategyFavorites( mContainer );
					break;

				case ePlayExhaustedStrategy.PlaySimilar:
					mExhaustedStrategy = new PlayExhaustedStrategySimilar( mContainer );
					break;

				case ePlayExhaustedStrategy.PlayStream:
					mExhaustedStrategy = new PlayExhaustedStrategyStream( mContainer );
					break;

				case ePlayExhaustedStrategy.PlayGenre:
					mExhaustedStrategy = new PlayExhaustedStrategyGenre( mContainer );
					break;
			}
		}

		public IEnumerable<PlayQueueTrack> PlayList {
			get{ return( from track in mPlayQueue select track ); }
		}

		private void FirePlayQueueChanged() {
			mEventAggregator.GetEvent<Events.PlayQueueChanged>().Publish( this );
		}
	}
}
