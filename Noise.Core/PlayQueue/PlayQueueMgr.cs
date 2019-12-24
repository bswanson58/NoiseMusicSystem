using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using CuttingEdge.Conditions;
using DynamicData;
using Noise.Core.Logging;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;

namespace Noise.Core.PlayQueue {
	internal class PlayQueueMgr : IPlayQueue, IRequireInitialization,
								  IHandle<Events.TrackUserUpdate>, IHandle<Events.DatabaseOpened>, IHandle<Events.DatabaseClosing> {
		private readonly IEventAggregator			    mEventAggregator;
		private readonly ILogPlayQueue				    mLog;
		private readonly IArtistProvider			    mArtistProvider;
		private readonly IAlbumProvider				    mAlbumProvider;
		private readonly ITrackProvider				    mTrackProvider;
		private readonly IStorageFileProvider		    mStorageFileProvider;
		private readonly IStorageFolderSupport		    mStorageFolderSupport;
		private readonly SourceList<PlayQueueTrack>		mPlayList;
		private readonly List<PlayQueueTrack>		    mPlayHistory;
		private readonly List<IPlayQueueSupport>	    mPlayQueueSupporters; 
		private readonly IPlayStrategyFactory		    mPlayStrategyFactory;
        private readonly IExhaustedStrategyPlayManager  mExhaustedStrategyPlay;
		private readonly IPreferences				    mPreferences;
		private IPlayStrategy						    mPlayStrategy;
		private	int									    mReplayTrackCount;
		private PlayQueueTrack						    mReplayTrack;
		private bool								    mAddingMoreTracks;
		private bool								    mStopAtEndOfTrack;
		private bool								    mDeleteUnplayedTracks;
		private int									    mMaximumPlayedTracks;

        public	IObservable<IChangeSet<PlayQueueTrack>>	PlayQueue { get; }
        public  IEnumerable<PlayQueueTrack>             PlayList => mPlayList.Items;
		public	ISourceList<PlayQueueTrack>				PlaySource => mPlayList;
        public  bool                                    IsQueueEmpty => mPlayList.Count == 0;
        public  int                                     UnplayedTrackCount => ( from PlayQueueTrack track in PlayList where !track.HasPlayed select track ).Count();
        public  int                                     PlayedTrackCount => GetPlayedTrackList().Count();
        public  bool                                    StrategyRequestsQueued => ( from PlayQueueTrack track in PlayList where track.IsStrategyQueued select track ).Any();

		public PlayQueueMgr( IEventAggregator eventAggregator, ILifecycleManager lifecycleManager,
							 IArtistProvider artistProvider, IAlbumProvider albumProvider, ITrackProvider trackProvider,
							 IStorageFolderSupport storageFolderSupport, IStorageFileProvider storageFileProvider,
							 IPlayStrategyFactory strategyFactory,
                             IExhaustedStrategyPlayManager exhaustedStrategyPlay,
							 IEnumerable<IPlayQueueSupport> playQueueSupporters, IPreferences preferences, ILogPlayQueue log ) {
			mEventAggregator = eventAggregator;
			mLog = log;
			mArtistProvider = artistProvider;
			mAlbumProvider = albumProvider;
			mTrackProvider = trackProvider;
			mStorageFileProvider = storageFileProvider;
			mStorageFolderSupport = storageFolderSupport;
			mPlayStrategyFactory = strategyFactory;
            mExhaustedStrategyPlay = exhaustedStrategyPlay;
			mPreferences = preferences;

            mPlayList = new SourceList<PlayQueueTrack>();
			PlayQueue = mPlayList.Connect();

			mPlayHistory = new List<PlayQueueTrack>();
			mPlayQueueSupporters = new List<IPlayQueueSupport>( playQueueSupporters );

			mPlayStrategy = mPlayStrategyFactory.ProvidePlayStrategy( ePlayStrategy.Next );
            mExhaustedStrategyPlay.StrategySpecification = ExhaustedStrategySpecification.Default;

			mDeleteUnplayedTracks = false;
			mMaximumPlayedTracks = 15;

			lifecycleManager.RegisterForInitialize( this );
		}

		public void Initialize() {
			foreach( var supporter in mPlayQueueSupporters ) {
				try {
					supporter.Initialize( this );
				}
				catch( Exception ex ) {
					mLog.LogQueueException( $"Initializing queue supporter: {supporter.GetType()}", ex );
				}
			}

			mEventAggregator.Subscribe( this );
		}

		public void Handle( Events.DatabaseOpened args ) {
			var preferences = mPreferences.Load<NoiseCorePreferences>();

			try {
                SetExhaustedStrategy( preferences.ExhaustedStrategySpecification );
				SetPlayStrategy( preferences.PlayStrategy, PlayStrategyParametersFactory.FromString( preferences.PlayStrategyParameters ));

				mDeleteUnplayedTracks = preferences.DeletePlayedTracks;
				mMaximumPlayedTracks = preferences.MaximumPlayedTracks;
			}
			catch( Exception exception ) {
				mLog.LogQueueException( "Loading parameters from preferences", exception );
			}
		}

		public void Shutdown() {
			mEventAggregator.Unsubscribe( this );
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
						var	firstStrategyTrack = PlayList.FirstOrDefault( t => t.HasPlayed == false && t.IsPlaying == false && t.StrategySource == eStrategySource.ExhaustedStrategy );
						if( firstStrategyTrack != null ) {
							mPlayList.Insert( PlayList.IndexOf( firstStrategyTrack ), newTrack );
						}
						else {
							mPlayList.Add( newTrack );
						}
					}
					else {
						mPlayList.Add( newTrack );
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
						var trackIndex = PlayList.IndexOf( afterTrack );

						mPlayList.Insert( trackIndex + 1, newTrack );

						FirePlayQueueChanged();
						mLog.AddedTrack( newTrack );
					}
				}
			}
		}

		public void Add( DbAlbum album ) {
			AddAlbum( album );

			FirePlayQueueChanged();
		}

		private void AddAlbum( DbAlbum album ) {
			using( var tracks = mTrackProvider.GetTrackList( album )) {
				var firstTrack = true;
				var sortedList = new List<DbTrack>( from DbTrack track in tracks.List orderby track.VolumeName, track.TrackNumber select track ).ToList();

				mAddingMoreTracks = sortedList.Count > 2;

				foreach( var track in sortedList ) {
					AddTrack( track, eStrategySource.User );

					if( firstTrack ) {
						FirePlayQueueChanged();

						firstTrack = false;
					}
				}

				mAddingMoreTracks = false;
			}
		}

		public void Add( DbAlbum album, string volumeName ) {
			using( var tracks = mTrackProvider.GetTrackList( album )) {
				var firstTrack = true;
				var sortedList = new List<DbTrack>( from DbTrack track in tracks.List
													where string.Equals( track.VolumeName, volumeName )
													orderby track.VolumeName, track.TrackNumber select track ).ToList();

				mAddingMoreTracks = sortedList.Count > 2;

				foreach( var track in sortedList ) {
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
				foreach( var album in albums.List ) {
					Add( album );
				}
			}
		}

		public void Add( DbInternetStream stream ) {
			var newTrack = new PlayQueueTrack( stream );

			mPlayList.Add( newTrack );
			mLog.AddedTrack( newTrack );

			FirePlayQueueChanged();
		}

		public void Handle( Events.TrackUserUpdate eventArgs ) {
			foreach( var queueTrack in PlayList.Where( queueTrack => ( queueTrack.Track != null ) && 
                                                                     ( queueTrack.Track.DbId == eventArgs.Track.DbId ))) {
				queueTrack.UpdateTrack( eventArgs.Track );

				mEventAggregator.PublishOnUIThread( new Events.PlaybackTrackUpdated( queueTrack ));
			}
		}

		public void Handle( Events.DatabaseClosing args ) {
			ClearQueue();
		}

		public void ClearQueue() {
			mPlayList.Clear();
			mPlayHistory.Clear();
			PlayingTrackReplayCount = 0;
			mLog.ClearedQueue();

			FirePlayQueueChanged();
		}

        public bool IsTrackQueued( DbTrack track ) {
			return( PlayList.Any( item => item.Track.DbId == track.DbId ));
		}

		public void ReplayQueue() {
			foreach( var track in PlayList ) {
				if(!track.IsPlaying ) {
					track.HasPlayed = false;

					mLog.StatusChanged( track );
				}

				track.IsFaulted = false;
			}
		}

		public bool ReplayTrack( long itemId ) {
			var retValue = false;
			var	track = PlayList.FirstOrDefault( item => item.Uid == itemId );

			if( track != null ) {
				track.HasPlayed = false;

				FirePlayQueueChanged();
				mLog.StatusChanged( track );

				retValue = true;
			}

			return( retValue );
		}

	    public void PromoteTrackFromStrategy( PlayQueueTrack track ) {
            var queuedTrack = PlayList.FirstOrDefault( t => t.Uid.Equals( track.Uid ));

            queuedTrack?.PromoteStrategy();
            FirePlayQueueChanged();
	    }

	    public bool RemoveTrack( long itemId ) {
			var retValue = false;
			var track = PlayList.FirstOrDefault( item => item.Uid == itemId );

			if( track != null ) {
				RemoveTrack( track );

				retValue = true;
			}

			return( retValue );
		}

		public void RemoveTrack( PlayQueueTrack track ) {
			if( track != null ) {
				var	queuedTrack= PlayList.FirstOrDefault( item => item.Uid.Equals( track.Uid ));

				if( queuedTrack != null ) {
					mPlayList.Remove(  queuedTrack );
					mLog.RemovedTrack( queuedTrack );
				}

				queuedTrack= mPlayHistory.Find( item => item.Track != null && item.Track.DbId == track.Track.DbId );

				if( queuedTrack != null ) {
					mPlayHistory.Remove(  queuedTrack );
				}

				FirePlayQueueChanged();
			}
		}

		public void RemovePlayedTracks() {
			var removeList = ( from track in PlayList where track.HasPlayed && !track.IsPlaying select  track ).ToList();

			foreach( var track in removeList ) {
				mPlayList.Remove( track );
				mLog.RemovedTrack( track );
			}

			mPlayHistory.Clear();

			FirePlayQueueChanged();
		}

		public void	ReorderQueueItem( int fromIndex, int toIndex ) {
			if(( fromIndex < mPlayList.Count ) &&
			   ( toIndex < mPlayList.Count )) {
				var track = PlayList.ElementAt( fromIndex );

				mPlayList.Remove( track );
				mPlayList.Insert( toIndex, track );

				var playingTrack = PlayingTrack;
				if( playingTrack != null ) {
					var playingIndex = PlayList.IndexOf( playingTrack );

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
						foreach( var queuedTrack in PlayList ) {
							queuedTrack.HasPlayed = PlayList.IndexOf( queuedTrack ) < playingIndex;
						}
					}
				}

				FirePlayQueueChanged();
			}
		}

        private IEnumerable<PlayQueueTrack> GetPlayedTrackList() {
			return( from PlayQueueTrack track in PlayList where track.HasPlayed && !track.IsPlaying select track );
		}

        public int PlayingTrackReplayCount {
			get => ( mReplayTrackCount );
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
                SelectExhaustedTracks();
			}
		}

		public bool CanStartPlayStrategy =>
            (!PlayList.Any()) &&
            ( mExhaustedStrategyPlay.CanQueueTracks());

        public PlayQueueTrack PlayNextTrack() {
			var	playingTrack = PlayingTrack;

			if( playingTrack != null ) {
				playingTrack.HasPlayed = true;
				playingTrack.IsPlaying = false;

				mPlayHistory.Add( playingTrack );
				mLog.StatusChanged( playingTrack );

				CheckQueueHorizon();
			}

			var nextTrack = NextTrack;
			if( nextTrack != null ) {
				nextTrack.IsPlaying = true;

				if( PlayingTrackReplayCount > 0 ) {
					PlayingTrackReplayCount--;
				}

				mLog.StatusChanged( nextTrack );
			}

			if((!mAddingMoreTracks )) {
                SelectExhaustedTracks();
			}

			if( playingTrack == null ) {
				mStopAtEndOfTrack = false;
			}

			return( nextTrack );
		}

		public bool DeletedPlayedTracks {
			get => ( mDeleteUnplayedTracks );
            set {
				mDeleteUnplayedTracks = value;

				var preferences = mPreferences.Load<NoiseCorePreferences>();

				preferences.DeletePlayedTracks = mDeleteUnplayedTracks;
				mPreferences.Save( preferences );
			}
		}
		public int MaximumPlayedTracks {
			get => ( mMaximumPlayedTracks );
            set {
				mMaximumPlayedTracks = value; 
				
				var preferences = mPreferences.Load<NoiseCorePreferences>();

				preferences.MaximumPlayedTracks = mMaximumPlayedTracks;
				mPreferences.Save( preferences );
			}
		}

		private void CheckQueueHorizon() {
			if( mDeleteUnplayedTracks ) {
				while( PlayedTrackCount > mMaximumPlayedTracks ) {
					RemoveTrack( GetPlayedTrackList().FirstOrDefault());
				}
			}
		}

		public void StopPlay() {
			var track = PlayingTrack;

			if( track != null ) {
				track.IsPlaying = false;

				mLog.StatusChanged( track );
			}
		}

		public void StopAtEndOfTrack() {
			mStopAtEndOfTrack = true;
		}

		public bool CanStopAtEndOfTrack() {
			return(!mStopAtEndOfTrack );
		}

		public PlayQueueTrack NextTrack {
			get {
				PlayQueueTrack	retValue = null;

				if(!mStopAtEndOfTrack ) {
					if( PlayingTrackReplayCount > 0 ) {
						retValue = mReplayTrack;
					}

					if( retValue == null ) {
						retValue = mPlayStrategy.NextTrack();

						if(( retValue == null ) &&
						   (!mAddingMoreTracks ) &&
						   ( mPlayList.Count > 0 )) {
							if( SelectExhaustedTracks()) {
								retValue = mPlayStrategy.NextTrack();
							}
						}
					}
				}

				return( retValue );
			}
		}

        private bool SelectExhaustedTracks() {
            const int suggestionCount = 3;
            var retValue = false;

            if( UnplayedTrackCount < suggestionCount ) {
                var tracks = mExhaustedStrategyPlay.SelectTracks( this, suggestionCount - UnplayedTrackCount );

                foreach( var track in tracks ) {
                    StrategyAdd( track );

                    retValue = true;
                }
            }

            return retValue;
        }

		public bool CanPlayNextTrack() {
			return(!mStopAtEndOfTrack && 
	              ( PlayList.FirstOrDefault( track => (!track.IsPlaying ) && 
                                                      (!track.HasPlayed )) != null ));
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
			var track = PlayList.FirstOrDefault( item => item.Uid == itemId );

			if( track != null ) {
				ContinuePlayFromTrack( track );

				FirePlayQueueChanged();

				retValue = true;
			}

			return( retValue );
		}

		public void ContinuePlayFromTrack( PlayQueueTrack track ) {
			if( PlayList.Contains( track )) {
				bool	hasPlayedSetting = true;

				foreach( var t in PlayList ) {
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
			get { return( PlayList.FirstOrDefault( track => ( track.IsPlaying ))); }
			set {
				var currentTrack = PlayList.FirstOrDefault( track => track.IsPlaying );

				if(( currentTrack != value ) &&
				   ( value != null ) &&
				   ( PlayList.IndexOf( value ) != -1 )) {
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

		public IPlayStrategy PlayStrategy => ( mPlayStrategy );

        public void SetPlayStrategy( ePlayStrategy strategyId, IPlayStrategyParameters parameters ) {
			mPlayStrategy = mPlayStrategyFactory.ProvidePlayStrategy( strategyId );

            mPlayStrategy?.Initialize( this, parameters );

            var preferences = mPreferences.Load<NoiseCorePreferences>();

			preferences.PlayStrategy = strategyId;
			preferences.PlayStrategyParameters = PlayStrategyParametersFactory.ToString( parameters );
			mPreferences.Save( preferences );

			mLog.StrategySet( strategyId, parameters );

			Condition.Requires( mPlayStrategy ).IsNotNull();
		}

		public IStrategyDescription PlayExhaustedStrategy => mExhaustedStrategyPlay.CurrentStrategy;

        public ExhaustedStrategySpecification ExhaustedPlayStrategy {
            get => mExhaustedStrategyPlay?.StrategySpecification;
            set {
                SetExhaustedStrategy( value );

                var preferences = mPreferences.Load<NoiseCorePreferences>();

                preferences.ExhaustedStrategySpecification = mExhaustedStrategyPlay.StrategySpecification;
                mPreferences.Save( preferences );
            }
        }

        private void SetExhaustedStrategy( ExhaustedStrategySpecification strategy ) {
            mExhaustedStrategyPlay.StrategySpecification = strategy;
        }

        private void FirePlayQueueChanged() {
			mEventAggregator.PublishOnUIThread( new Events.PlayQueueChanged( this ));
		}
	}
}
