using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using CuttingEdge.Conditions;
using Noise.Core.Logging;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;

namespace Noise.Core.PlayQueue {
	internal class PlayQueueMgr : IPlayQueue, IRequireInitialization,
								  IHandle<Events.TrackUserUpdate>, IHandle<Events.DatabaseOpened>, IHandle<Events.DatabaseClosing> {
		private readonly IEventAggregator			mEventAggregator;
		private readonly ILogPlayQueue				mLog;
		private readonly IArtistProvider			mArtistProvider;
		private readonly IAlbumProvider				mAlbumProvider;
		private readonly ITrackProvider				mTrackProvider;
		private readonly IStorageFileProvider		mStorageFileProvider;
		private readonly IStorageFolderSupport		mStorageFolderSupport;
		private readonly List<PlayQueueTrack>		mPlayQueue;
		private readonly List<PlayQueueTrack>		mPlayHistory;
		private readonly List<IPlayQueueSupport>	mPlayQueueSupporters; 
		private readonly IPlayStrategyFactory		mPlayStrategyFactory;
		private readonly IPlayExhaustedFactory		mPlayExhaustedFactory;
		private readonly IPreferences				mPreferences;
		private IPlayStrategy						mPlayStrategy;
		private IPlayExhaustedStrategy				mPlayExhaustedStrategy;
		private	int									mReplayTrackCount;
		private PlayQueueTrack						mReplayTrack;
		private bool								mAddingMoreTracks;
		private AsyncCommand<DbTrack>				mTrackPlayCommand;
		private AsyncCommand<IEnumerable<DbTrack>>	mTrackListPlayCommand;
		private AsyncCommand<DbAlbum>				mAlbumPlayCommand;
		private AsyncCommand<DbTrack>				mVolumePlayCommand; 
		private AsyncCommand<DbInternetStream>		mStreamPlayCommand;

		public PlayQueueMgr( IEventAggregator eventAggregator, ILifecycleManager lifecycleManager,
							 IArtistProvider artistProvider, IAlbumProvider albumProvider, ITrackProvider trackProvider,
							 IStorageFolderSupport storageFolderSupport, IStorageFileProvider storageFileProvider,
							 IPlayStrategyFactory strategyFactory, IPlayExhaustedFactory exhaustedFactory,
							 IEnumerable<IPlayQueueSupport> playQueueSupporters, IPreferences preferences, ILogPlayQueue log ) {
			mEventAggregator = eventAggregator;
			mLog = log;
			mArtistProvider = artistProvider;
			mAlbumProvider = albumProvider;
			mTrackProvider = trackProvider;
			mStorageFileProvider = storageFileProvider;
			mStorageFolderSupport = storageFolderSupport;
			mPlayStrategyFactory = strategyFactory;
			mPlayExhaustedFactory = exhaustedFactory;
			mPreferences = preferences;

			mPlayQueue = new List<PlayQueueTrack>();
			mPlayHistory = new List<PlayQueueTrack>();
			mPlayQueueSupporters = new List<IPlayQueueSupport>( playQueueSupporters );

			mPlayStrategy = mPlayStrategyFactory.ProvidePlayStrategy( ePlayStrategy.Next );
			mPlayExhaustedStrategy = mPlayExhaustedFactory.ProvideExhaustedStrategy( ePlayExhaustedStrategy.Stop );

			lifecycleManager.RegisterForInitialize( this );
		}

		public void Initialize() {
			foreach( var supporter in mPlayQueueSupporters ) {
				try {
					supporter.Initialize( this );
				}
				catch( Exception ex ) {
					mLog.LogQueueException( string.Format( "Initializing queue supporte: {0}", supporter.GetType()), ex );
				}
			}

			mTrackPlayCommand = new AsyncCommand<DbTrack>( OnTrackPlayCommand );
			GlobalCommands.PlayTrack.RegisterCommand( mTrackPlayCommand );

			mTrackListPlayCommand = new AsyncCommand<IEnumerable<DbTrack>>( OnTrackListPlayCommand );
			GlobalCommands.PlayTrackList.RegisterCommand( mTrackListPlayCommand );

			mAlbumPlayCommand = new AsyncCommand<DbAlbum>( OnAlbumPlayCommand );
			GlobalCommands.PlayAlbum.RegisterCommand( mAlbumPlayCommand );

			mVolumePlayCommand = new AsyncCommand<DbTrack>( OnVolumePlayCommand );
			GlobalCommands.PlayVolume.RegisterCommand( mVolumePlayCommand );

			mStreamPlayCommand = new AsyncCommand<DbInternetStream>( OnStreamPlayCommand );
			GlobalCommands.PlayStream.RegisterCommand( mStreamPlayCommand );

			mEventAggregator.Subscribe( this );
		}

		public void Handle( Events.DatabaseOpened args ) {
			var preferences = mPreferences.Load<NoiseCorePreferences>();

			SetPlayExhaustedStrategy( preferences.PlayExhaustedStrategy, PlayStrategyParametersFactory.FromString( preferences.PlayExhaustedParameters ));
			SetPlayStrategy( preferences.PlayStrategy, PlayStrategyParametersFactory.FromString( preferences.PlayStrategyParameters ));
		}

		public void Shutdown() {
			mEventAggregator.Unsubscribe( this );

			GlobalCommands.PlayTrack.UnregisterCommand( mTrackPlayCommand );
			GlobalCommands.PlayTrackList.UnregisterCommand( mTrackListPlayCommand );
			GlobalCommands.PlayAlbum.UnregisterCommand( mAlbumPlayCommand );
			GlobalCommands.PlayVolume.UnregisterCommand( mVolumePlayCommand );
			GlobalCommands.PlayStream.UnregisterCommand( mStreamPlayCommand );
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

		public void Add( IEnumerable<DbTrack> trackList ) {
			foreach( var track in trackList ) {
				AddTrack( track, eStrategySource.User );
			}

			FirePlayQueueChanged();
		}

		public void StrategyAdd( DbTrack track ) {
			AddTrack( track, eStrategySource.ExhaustedStrategy );

			FirePlayQueueChanged();
		}

		private string ResolvePath( StorageFile file ) {
			return ( mStorageFolderSupport.GetPath( file ));
		}

		private StorageFile ResolveStorageFile( DbTrack track ) {
			return( mStorageFileProvider.GetPhysicalFile( track ));
		}

		private void AddTrack( DbTrack track, eStrategySource strategySource ) {
			var album = mAlbumProvider.GetAlbumForTrack( track );

			if( album != null ) {
				var artist = mArtistProvider.GetArtistForAlbum( album );

				if( artist != null ) {
					var newTrack = new PlayQueueTrack( artist, album, track, ResolveStorageFile, ResolvePath, strategySource );

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

					mLog.AddedTrack( newTrack );
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
						var newTrack = new PlayQueueTrack( artist, album, track, ResolveStorageFile, ResolvePath, eStrategySource.PlayStrategy );
						var trackIndex = mPlayQueue.IndexOf( afterTrack );

						mPlayQueue.Insert( trackIndex + 1, newTrack );

						FirePlayQueueChanged();
						mLog.AddedTrack( newTrack );
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
													orderby track.VolumeName, track.TrackNumber ascending select track ).ToList();

				mAddingMoreTracks = sortedList.Count > 2;

				foreach( DbTrack track in sortedList ) {
					AddTrack( track, eStrategySource.User );

					if( firstTrack ) {
						FirePlayQueueChanged();

						firstTrack = false;
					}
				}

				mAddingMoreTracks = false;
			}
		}

		private void OnVolumePlayCommand( DbTrack track ) {
			var album = mAlbumProvider.GetAlbum( track.Album );

			if( album != null ) {
				Add( album, track.VolumeName );
			}
		}

		public void Add( DbAlbum album, string volumeName ) {
			using( var tracks = mTrackProvider.GetTrackList( album )) {
				var firstTrack = true;
				var sortedList = new List<DbTrack>( from DbTrack track in tracks.List
													where string.Equals( track.VolumeName, volumeName )
													orderby track.VolumeName, track.TrackNumber ascending select track ).ToList();

				mAddingMoreTracks = sortedList.Count > 2;

				foreach( DbTrack track in sortedList ) {
					AddTrack( track, eStrategySource.User );

					if( firstTrack ) {
						FirePlayQueueChanged();

						firstTrack = false;
					}
				}

				mAddingMoreTracks = false;
				FirePlayQueueChanged();
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
			var newTrack = new PlayQueueTrack( stream );

			mPlayQueue.Add( newTrack );
			mLog.AddedTrack( newTrack );

			FirePlayQueueChanged();
		}

		public void Handle( Events.TrackUserUpdate eventArgs ) {
			foreach( var queueTrack in mPlayQueue.Where( queueTrack => ( queueTrack.Track != null ) &&
																	   ( queueTrack.Track.DbId == eventArgs.TrackId ))) {
				queueTrack.UpdateTrack( mTrackProvider.GetTrack( eventArgs.TrackId ));

				mEventAggregator.Publish( new Events.PlaybackTrackUpdated( queueTrack ));
			}
		}

		public void Handle( Events.DatabaseClosing args ) {
			ClearQueue();
		}

		public void ClearQueue() {
			mPlayQueue.Clear();
			mPlayHistory.Clear();
			PlayingTrackReplayCount = 0;
			mLog.ClearedQueue();

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

					mLog.StatusChanged( track );
				}

				track.IsFaulted = false;
			}
		}

		public bool ReplayTrack( long itemId ) {
			bool	retValue = false;
			var		track = mPlayQueue.FirstOrDefault( item => item.Uid == itemId );

			if( track != null ) {
				track.HasPlayed = false;

				FirePlayQueueChanged();
				mLog.StatusChanged( track );

				retValue = true;
			}

			return( retValue );
		}

		public bool RemoveTrack( long itemId ) {
			var retValue = false;
			var track = mPlayQueue.FirstOrDefault( item => item.Uid == itemId );

			if( track != null ) {
				RemoveTrack( track );
				mLog.RemovedTrack( track );

				retValue = true;
			}

			return( retValue );
		}

		public void RemoveTrack( PlayQueueTrack track ) {
			if( track.Track !=null ) {
				var	queuedTrack= mPlayQueue.Find( item => item.Track != null && item.Track.DbId == track.Track.DbId );

				if( queuedTrack != null ) {
					mPlayQueue.Remove(  queuedTrack );
					mLog.RemovedTrack( queuedTrack );
				}

				queuedTrack= mPlayHistory.Find( item => item.Track != null && item.Track.DbId == track.Track.DbId );

				if( queuedTrack != null ) {
					mPlayHistory.Remove(  queuedTrack );
				}
			}
			else if( track.Stream != null ) {
				var	queuedTrack= mPlayQueue.Find( item => item.Stream != null && item.Stream.DbId == track.Stream.DbId );

				if( queuedTrack != null ) {
					mPlayQueue.Remove(  queuedTrack );
					mLog.RemovedTrack( queuedTrack );
				}

				queuedTrack= mPlayHistory.Find( item => item.Stream != null && item.Stream.DbId == track.Stream.DbId );

				if( queuedTrack != null ) {
					mPlayHistory.Remove(  queuedTrack );
				}
			}

			FirePlayQueueChanged();
		}

		public void RemovePlayedTracks() {
			var removeList = from track in mPlayQueue where track.HasPlayed && !track.IsPlaying select  track;

			foreach( var track in removeList ) {
				mPlayQueue.Remove( track );
				mLog.RemovedTrack( track );
			}

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
						if(!track.HasPlayed ) {
							track.HasPlayed = true;

							mLog.StatusChanged( track );
						}
					}
					if( playingIndex < toIndex ) {
						if( track.HasPlayed ) {
							track.HasPlayed = false;

							mLog.StatusChanged( track );
						}
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

		public void StartPlayStrategy() {
			if( CanStartPlayStrategy ) {
				mPlayExhaustedStrategy.QueueTracks();
			}
		}

		public bool CanStartPlayStrategy {
			get {
				return(!mPlayQueue.Any()) &&
					  ( mPlayExhaustedStrategy != null ) &&
					  ( mPlayExhaustedStrategy.StrategyId != ePlayExhaustedStrategy.Stop );
			}
		}

		public PlayQueueTrack PlayNextTrack() {
			var	track = PlayingTrack;

			if( track != null ) {
				track.HasPlayed = true;
				track.IsPlaying = false;

				mPlayHistory.Add( track );
				mLog.StatusChanged( track );
			}

			track = NextTrack;
			if( track != null ) {
				track.IsPlaying = true;

				if( PlayingTrackReplayCount > 0 ) {
					PlayingTrackReplayCount--;
				}

				mLog.StatusChanged( track );
			}

			if(( mPlayExhaustedStrategy != null ) &&
			   (!mAddingMoreTracks )) {
				mPlayExhaustedStrategy.QueueTracks();
			}

			return( track );
		}

		public void StopPlay() {
			var track = PlayingTrack;

			if( track != null ) {
				track.IsPlaying = false;

				mLog.StatusChanged( track );
			}
		}

		public PlayQueueTrack NextTrack {
			get {
				PlayQueueTrack	retValue = null;

				if( PlayingTrackReplayCount > 0 ) {
					retValue = mReplayTrack;
				}

				if( retValue == null ) {
					retValue = mPlayStrategy.NextTrack();

					if(( retValue == null ) &&
					   ( mPlayExhaustedStrategy != null ) &&
					   (!mAddingMoreTracks ) &&
					   ( mPlayQueue.Count > 0 )) {
						if( mPlayExhaustedStrategy.QueueTracks()) {
							retValue = mPlayStrategy.NextTrack();
						}
					}
				}

				return( retValue );
			}
		}

		public bool CanPlayNextTrack() {
			return( mPlayQueue.FirstOrDefault( track => ( !track.IsPlaying ) && ( !track.HasPlayed )) != null );
		}

		public PlayQueueTrack PlayPreviousTrack() {
			var	track = PlayingTrack;

			if( track != null ) {
				track.IsPlaying = false;

				mLog.StatusChanged( track );
			}

			track = PreviousTrack;
			if( track != null ) {
				track.HasPlayed = false;
				track.IsPlaying = true;

				mLog.StatusChanged( track );

				mPlayHistory.Remove( mPlayHistory.Last());
			}

			return( track );
		}

		public bool ContinuePlayFromTrack( long itemId ) {
			var retValue = false;
			var track = mPlayQueue.FirstOrDefault( item => item.Uid == itemId );

			if( track != null ) {
				ContinuePlayFromTrack( track );

				FirePlayQueueChanged();

				retValue = true;
			}

			return( retValue );
		}
		public void ContinuePlayFromTrack( PlayQueueTrack track ) {
			if( mPlayQueue.Contains( track )) {
				bool	hasPlayedSetting = true;

				foreach( var t in mPlayQueue ) {
					if( t == track ) {
						hasPlayedSetting = false;
					}

					if(!t.IsPlaying ) {
						if( t.HasPlayed != hasPlayedSetting ) {
							t.HasPlayed = hasPlayedSetting;

							mLog.StatusChanged( t );
						}
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

		public bool CanPlayPreviousTrack() {
			return( mPlayHistory.Count > 0 );
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

						mLog.StatusChanged( currentTrack );
					}

					value.IsPlaying = true;
					value.HasPlayed = false;

					mLog.StatusChanged( value );
				}
			}
		}

		public IPlayStrategy PlayStrategy {
			get { return( mPlayStrategy ); }
		}

		public void SetPlayStrategy( ePlayStrategy strategyId, IPlayStrategyParameters parameters ) {
			mPlayStrategy = mPlayStrategyFactory.ProvidePlayStrategy( strategyId );

			if( mPlayStrategy != null ) {
				mPlayStrategy.Initialize( this, parameters );
			}

			var preferences = mPreferences.Load<NoiseCorePreferences>();

			preferences.PlayStrategy = strategyId;
			preferences.PlayStrategyParameters = PlayStrategyParametersFactory.ToString( parameters );
			mPreferences.Save( preferences );

			mLog.StrategySet( strategyId, parameters );

			Condition.Requires( mPlayStrategy ).IsNotNull();
		}

		public IPlayExhaustedStrategy PlayExhaustedStrategy {
			get { return( mPlayExhaustedStrategy ); }
		}

		public void SetPlayExhaustedStrategy( ePlayExhaustedStrategy strategy, IPlayStrategyParameters parameters ) {
			mPlayExhaustedStrategy = mPlayExhaustedFactory.ProvideExhaustedStrategy( strategy );
			if( mPlayExhaustedStrategy != null ) {
				mPlayExhaustedStrategy.Initialize( this, parameters );

				mEventAggregator.Publish( new Events.PlayExhaustedStrategyChanged( mPlayExhaustedStrategy.StrategyId, parameters ));
			}

			var preferences = mPreferences.Load<NoiseCorePreferences>();

			preferences.PlayExhaustedStrategy = strategy;
			preferences.PlayExhaustedParameters = PlayStrategyParametersFactory.ToString( parameters );
			mPreferences.Save( preferences );

			mLog.StrategySet( strategy, parameters );

			Condition.Requires( mPlayExhaustedStrategy ).IsNotNull();
		}

		public IEnumerable<PlayQueueTrack> PlayList {
			get{ return( mPlayQueue ); }
		}

		private void FirePlayQueueChanged() {
			mEventAggregator.Publish( new Events.PlayQueueChanged( this ));
		}
	}
}
