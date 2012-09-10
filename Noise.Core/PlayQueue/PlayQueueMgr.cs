using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;

namespace Noise.Core.PlayQueue {
	internal class PlayQueueMgr : IPlayQueue,
								  IHandle<Events.TrackUserUpdate> {
		private readonly IEventAggregator					mEventAggregator;
		private readonly IArtistProvider					mArtistProvider;
		private readonly IAlbumProvider						mAlbumProvider;
		private readonly ITrackProvider						mTrackProvider;
		private readonly IStorageFileProvider				mStorageFileProvider;
		private readonly IStorageFolderSupport				mStorageFolderSupport;
		private readonly List<PlayQueueTrack>				mPlayQueue;
		private readonly List<PlayQueueTrack>				mPlayHistory;
		private	ePlayStrategy								mPlayStrategy;
		private readonly IPlayStrategyFactory				mPlayStrategyFactory;
		private IPlayStrategy								mStrategy;
		private ePlayExhaustedStrategy						mPlayExhaustedStrategy;
		private readonly IPlayExhaustedFactory				mPlayExhaustedFactory;
		private long										mPlayExhaustedItem;
		private IPlayExhaustedStrategy						mExhaustedStrategy;
		private	int											mReplayTrackCount;
		private PlayQueueTrack								mReplayTrack;
		private readonly AsyncCommand<DbTrack>				mTrackPlayCommand;
		private readonly AsyncCommand<IEnumerable<DbTrack>>	mTrackListPlayCommand;
		private readonly AsyncCommand<DbAlbum>				mAlbumPlayCommand;
		private readonly AsyncCommand<DbInternetStream>		mStreamPlayCommand;

		public PlayQueueMgr( IEventAggregator eventAggregator,
							 IArtistProvider artistProvider, IAlbumProvider albumProvider, ITrackProvider trackProvider,
							 IStorageFolderSupport storageFolderSupport, IStorageFileProvider storageFileProvider,
							 IPlayStrategyFactory strategyFactory, IPlayExhaustedFactory exhaustedFactory ) {
			mEventAggregator = eventAggregator;
			mArtistProvider = artistProvider;
			mAlbumProvider = albumProvider;
			mTrackProvider = trackProvider;
			mStorageFileProvider = storageFileProvider;
			mStorageFolderSupport = storageFolderSupport;
			mPlayStrategyFactory = strategyFactory;
			mPlayExhaustedFactory = exhaustedFactory;

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

			mEventAggregator.Subscribe( this );
			NoiseLogger.Current.LogInfo( "PlayQueue created" );
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
			var album = mAlbumProvider.GetAlbumForTrack( track );

			if( album != null ) {
				var artist = mArtistProvider.GetArtistForAlbum( album );

				if( artist != null ) {
					var file = mStorageFileProvider.GetPhysicalFile( track );

					if( file != null ) {
						var path = mStorageFolderSupport.GetPath( file );
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
				var album = mAlbumProvider.GetAlbumForTrack( track );

				if( album != null ) {
					var artist = mArtistProvider.GetArtistForAlbum( album );

					if( artist != null ) {
						var file = mStorageFileProvider.GetPhysicalFile( track );

						if( file != null ) {
							var path = mStorageFolderSupport.GetPath( file );
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
			using( var tracks = mTrackProvider.GetTrackList( album )) {
				var firstTrack = true;
				var sortedList = new List<DbTrack>( from DbTrack track in tracks.List
													orderby track.VolumeName, track.TrackNumber ascending select track );

				foreach( DbTrack track in sortedList ) {
					AddTrack( track, eStrategySource.User );

					if( firstTrack ) {
						FirePlayQueueChanged();

						firstTrack = false;
					}
				}
			}
		}

		public void Add( DbArtist artist ) {
			using( var albums = mAlbumProvider.GetAlbumList( artist )) {
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

		public void Handle( Events.TrackUserUpdate eventArgs ) {
			foreach( var queueTrack in mPlayQueue.Where( queueTrack => ( queueTrack.Track != null ) &&
			                                                           ( queueTrack.Track.DbId == eventArgs.TrackId ))) {
				queueTrack.UpdateTrack( mTrackProvider.GetTrack( eventArgs.TrackId ));

				mEventAggregator.Publish( new Events.PlaybackTrackUpdated( queueTrack ));
			}
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

				track.IsFaulted = false;
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
			else if( track.Stream != null ) {
				var	queuedTrack= mPlayQueue.Find( item => item.Stream != null ? item.Stream.DbId == track.Stream.DbId : false );

				if( queuedTrack != null ) {
					mPlayQueue.Remove(  queuedTrack );
				}

				queuedTrack= mPlayHistory.Find( item => item.Stream != null ? item.Stream.DbId == track.Stream.DbId : false );

				if( queuedTrack != null ) {
					mPlayHistory.Remove(  queuedTrack );
				}
			}

			FirePlayQueueChanged();
		}

		public void RemovePlayedTracks() {
			mPlayQueue.RemoveAll( track => track.HasPlayed && !track.IsPlaying );
			mPlayHistory.Clear();

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

		public int PlayedTrackCount {
			get { return(( from PlayQueueTrack track in mPlayQueue where track.HasPlayed && !track.IsPlaying select track ).Count()); }
		}

		public bool StrategyRequestsQueued {
			get{ return(( from PlayQueueTrack track in mPlayQueue where track.IsStrategyQueued select track ).Any()); }
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

		public void ContinuePlayFromTrack( PlayQueueTrack track ) {
			if( mPlayQueue.Contains( track )) {
				bool	hasPlayedSetting = true;

				foreach( var t in mPlayQueue ) {
					if( t == track ) {
						hasPlayedSetting = false;
					}

					if(!t.IsPlaying ) {
						t.HasPlayed = hasPlayedSetting;
					}
				}
			}
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
				mStrategy = mPlayStrategyFactory.ProvidePlayStrategy( mPlayStrategy );
			}
		}

		public ePlayExhaustedStrategy PlayExhaustedStrategy {
			get { return( mPlayExhaustedStrategy ); }
		}

		public void SetPlayExhaustedStrategy( ePlayExhaustedStrategy strategy, long itemId ) {
			mPlayExhaustedStrategy = strategy;
			mPlayExhaustedItem = itemId;
			mExhaustedStrategy = mPlayExhaustedFactory.ProvideExhaustedStrategy( mPlayExhaustedStrategy );
		}

		public IEnumerable<PlayQueueTrack> PlayList {
			get{ return( from track in mPlayQueue select track ); }
		}

		private void FirePlayQueueChanged() {
			mEventAggregator.Publish( new Events.PlayQueueChanged( this ));
		}
	}
}
