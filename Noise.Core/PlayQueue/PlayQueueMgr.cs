﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayQueue {
	internal class PlayQueueMgr : IPlayQueue {
		private readonly IUnityContainer		mContainer;
		private readonly IDataProvider			mDataProvider;
		private readonly IEventAggregator		mEventAggregator;
		private readonly List<PlayQueueTrack>	mPlayQueue;
		private readonly List<PlayQueueTrack>	mPlayHistory;
		private	ePlayStrategy					mPlayStrategy;
		private IPlayStrategy					mStrategy;
		private ePlayExhaustedStrategy			mPlayExhaustedStrategy;
		private IPlayExhaustedStrategy			mExhaustedStrategy;

		public PlayQueueMgr( IUnityContainer container ) {
			mContainer = container;
			mDataProvider = mContainer.Resolve<IDataProvider>();
			mEventAggregator = mContainer.Resolve<IEventAggregator>();

			mPlayQueue = new List<PlayQueueTrack>();
			mPlayHistory = new List<PlayQueueTrack>();

			PlayStrategy = ePlayStrategy.Next;
			PlayExhaustedStrategy = ePlayExhaustedStrategy.Stop;

			mEventAggregator.GetEvent<Events.AlbumPlayRequested>().Subscribe( OnAlbumPlayRequest );
			mEventAggregator.GetEvent<Events.TrackPlayRequested>().Subscribe( OnTrackPlayRequest );
			mEventAggregator.GetEvent<Events.StreamPlayRequested>().Subscribe( OnStreamPlayRequest );
		}

		public void OnTrackPlayRequest( DbTrack track ) {
			Add( track );
		}

		public void Add( DbTrack track ) {
			AddTrack( track, false );

			FirePlayQueueChanged();
		}

		public void StrategyAdd( DbTrack track ) {
			AddTrack( track, true );

			FirePlayQueueChanged();
		}

		private void AddTrack( DbTrack track, bool strategyRequest ) {
			DbArtist	artist = null;

			var album = mDataProvider.GetAlbumForTrack( track );
			if( album != null ) {
				artist = mDataProvider.GetArtistForAlbum( album );
			}

			mPlayQueue.Add( new PlayQueueTrack( artist, album, track, mDataProvider.GetPhysicalFile( track ), strategyRequest ));
		}

		public void OnAlbumPlayRequest( DbAlbum album ) {
			Add( album );
		}

		public void Add( DbAlbum album ) {
			AddAlbum( album );

			FirePlayQueueChanged();
		}

		private void AddAlbum( DbAlbum album ) {
			using( var tracks = mDataProvider.GetTrackList( album )) {
				foreach( DbTrack track in tracks.List ) {
					AddTrack( track, false );
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

		public void OnStreamPlayRequest( DbInternetStream stream ) {
			Add( stream  );
		}

		public void Add( DbInternetStream stream ) {
			mPlayQueue.Add( new PlayQueueTrack( stream ));

			FirePlayQueueChanged();
		}

		public void ClearQueue() {
			mPlayQueue.Clear();
			mPlayHistory.Clear();

			FirePlayQueueChanged();
		}

		public bool IsQueueEmpty {
			get{ return( mPlayQueue.Count == 0 ); }
		}

		public int UnplayedTrackCount {
			get { return(( from PlayQueueTrack track in mPlayQueue where !track.HasPlayed select track ).Count()); }
		}

		public bool StrategyRequestsQueued {
			get{ return(( from PlayQueueTrack track in mPlayQueue where track.IsStrategyQueued select track ).Count() > 0 ); }
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
				var	retValue = mStrategy.NextTrack( mPlayQueue );

				if(( retValue == null ) &&
				   ( mPlayQueue.Count > 0 )) {
					if( mExhaustedStrategy.QueueExhausted( this )) {
						retValue = mStrategy.NextTrack( mPlayQueue );
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
				}
			}
		}

		public ePlayExhaustedStrategy PlayExhaustedStrategy {
			get { return( mPlayExhaustedStrategy ); }
			set {
				mPlayExhaustedStrategy = value;

				switch( mPlayExhaustedStrategy ) {
					case ePlayExhaustedStrategy.Stop:
						mExhaustedStrategy = new PlayExhaustedStrategyStop();
						break;

					case ePlayExhaustedStrategy.Replay:
						mExhaustedStrategy = new PlayQueueExhaustedStrategyReplay();
						break;

					case ePlayExhaustedStrategy.PlayFavorites:
						mExhaustedStrategy = new PlayExhaustedStrategyFavorites( mContainer );
						break;
				}
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
