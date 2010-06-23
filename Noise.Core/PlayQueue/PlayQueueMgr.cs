using System.Collections.Generic;
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
		private	ePlayStrategy					mPlayStrategy;
		private IPlayStrategy					mStrategy;

		public PlayQueueMgr( IUnityContainer container ) {
			mContainer = container;
			mDataProvider = mContainer.Resolve<IDataProvider>();
			mEventAggregator = mContainer.Resolve<IEventAggregator>();

			mPlayQueue = new List<PlayQueueTrack>();

			PlayStrategy = ePlayStrategy.PlaySingle;
		}

		public void Add( DbTrack track ) {
			mPlayQueue.Add( new PlayQueueTrack( track, mDataProvider.GetPhysicalFile( track )));

			FirePlayQueueChanged();
		}

		public void Add( DbAlbum album ) {
			var tracks = mDataProvider.GetTrackList( album );

			foreach( DbTrack track in tracks ) {
				Add( track );
			}
		}

		public void Add( DbArtist artist ) {
			var	albums = mDataProvider.GetAlbumList( artist );

			foreach( DbAlbum album in albums ) {
				Add( album );
			}
		}

		public void ClearQueue() {
			mPlayQueue.Clear();

			FirePlayQueueChanged();
		}

		public PlayQueueTrack PlayNextTrack() {
			var	track = PlayingTrack;

			if( track != null ) {
				track.HasPlayed = true;
				track.IsPlaying = false;
			}

			track = NextTrack;
			if( track != null ) {
				track.IsPlaying = true;
			}

			return( track );
		}

		public void StopPlay() {
			var track = PlayingTrack;

			if( track != null ) {
				track.IsPlaying = false;
			}
		}

		public PlayQueueTrack NextTrack {
			get{ return( mStrategy.NextTrack( mPlayQueue )); }
		}

		public PlayQueueTrack PlayingTrack {
			get { return( mPlayQueue.FirstOrDefault( track => ( track.IsPlaying ))); }
		}

		public ePlayStrategy PlayStrategy {
			get { return( mPlayStrategy ); }
			set {
				mPlayStrategy = value;

				switch( mPlayStrategy ) {
					case ePlayStrategy.PlaySingle:
						mStrategy = new PlayStrategySingle();
						break;

					case ePlayStrategy.Random:
						mStrategy = new	PlayStrategyRandom();
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
