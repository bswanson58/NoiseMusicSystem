using System;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;

namespace Noise.UI.ViewModels {
	public class PlayerViewModel : ViewModelBase {
		private	IUnityContainer		mContainer;
		private IEventAggregator	mEvents;
		private INoiseManager		mNoiseManager;

		[Dependency]
		public IUnityContainer Container {
			get { return( mContainer ); }
			set {
				mContainer = value;

				mEvents = mContainer.Resolve<IEventAggregator>();
				mNoiseManager = mContainer.Resolve<INoiseManager>();

				mEvents.GetEvent<Events.PlayQueueChanged>().Subscribe( OnPlayQueueChanged );
				mEvents.GetEvent<Events.PlaybackStatusChanged>().Subscribe( OnPlaybackStatusChanged );
				mEvents.GetEvent<Events.PlaybackTrackChanged>().Subscribe( OnPlaybackTrackChanged );
				mEvents.GetEvent<Events.PlaybackInfoChanged>().Subscribe( OnPlaybackInfoChanged );
			}
		}

		public void OnPlayQueueChanged( IPlayQueue playQueue ) {
			PlayQueueChangedFlag++;
		}

		public void OnPlaybackStatusChanged( ePlaybackStatus status ) {
			CurrentStatus = status;
		}

		public void OnPlaybackTrackChanged( object sender ) {
			StartTrackFlag++;
		}

		public void OnPlaybackInfoChanged( object sender ) {
			InfoUpdateFlag++;
		}

		private ePlaybackStatus CurrentStatus {
			get { return( Get(() => CurrentStatus, ePlaybackStatus.Stopped ));  }
			set { Set(() => CurrentStatus, value ); }
		}

		private int PlayQueueChangedFlag {
			get{ return( Get( () => PlayQueueChangedFlag, 0 )); }
			set{ Set( () => PlayQueueChangedFlag, value  ); }
		}

		private int StartTrackFlag {
			get{ return( Get( () => StartTrackFlag, 0 )); }
			set{ Set( () => StartTrackFlag, value  ); }
		}

		private int InfoUpdateFlag {
			get{ return( Get( () => InfoUpdateFlag, 0 ));  }
			set{ Set( () => InfoUpdateFlag, value ); }
		}

		[DependsUpon( "StartTrackFlag" )]
		public string TrackName {
			get { 
				var retValue = "None";

				if( mNoiseManager.PlayController.CurrentTrack != null ) {
					var track = mNoiseManager.PlayController.CurrentTrack;

					retValue = track.IsStream ? track.StreamInfo != null ? String.Format( "{0} ({1}/{2})", track.StreamInfo.Title, track.StreamInfo.Artist, track.StreamInfo.Album ) :
													String.Format( "{0} - {1}", track.Stream.Name, track.Stream.Description ) :
												String.Format( "{0} ({1}/{2})", track.Track.Name, track.Artist.Name, track.Album.Name );
				}
				else if( IsInDesignMode ) {
					retValue = "The Flying Dutchmens Tribute";
				}

				return( retValue );
			} 
		}

		[DependsUpon( "InfoUpdateFlag" )]
		public TimeSpan TrackTime {
			get { return( mNoiseManager.PlayController.TrackTime ); }
		}

		[DependsUpon( "InfoUpdateFlag" )]
		public long TrackPosition {
			get { return( mNoiseManager.PlayController.PlayPosition ); }
			set { mNoiseManager.PlayController.PlayPosition = value; }
		}

		[DependsUpon( "InfoUpdateFlag" )]
		public long TrackEndPosition {
			get { return( mNoiseManager.PlayController.TrackEndPosition ); }
		}

		[DependsUpon( "InfoUpdateFlag" )]
		public double Volume {
			get{ return( mNoiseManager.PlayController.Volume ); }
			set{ mNoiseManager.PlayController.Volume = (float)value; }
		}

		[DependsUpon( "InfoUpdateFlag" )]
		public double PlaySpeed {
			get{ return( mNoiseManager.PlayController.PlaySpeed ); }
			set{ mNoiseManager.PlayController.PlaySpeed = (float)value; }
		}

		[DependsUpon( "InfoUpdateFlag" )]
		public double PanPosition {
			get{ return( mNoiseManager.PlayController.PanPosition ); }
			set{ mNoiseManager.PlayController.PanPosition = (float)value; }
		}

		public bool RandomPlay {
			get{ return( mNoiseManager.PlayQueue.PlayStrategy == ePlayStrategy.Random ); }
			set{ mNoiseManager.PlayQueue.PlayStrategy = value ? ePlayStrategy.Random : ePlayStrategy.Next; }
		}

		public bool QueueReplay {
			get{ return( mNoiseManager.PlayQueue.PlayExhaustedStrategy == ePlayExhaustedStrategy.Replay ); }
			set{ mNoiseManager.PlayQueue.PlayExhaustedStrategy = value ? ePlayExhaustedStrategy.Replay : ePlayExhaustedStrategy.Stop; }
		}

		[DependsUpon( "InfoUpdateFlag" )]
		public double LeftLevel {
			get { return( mNoiseManager.PlayController.LeftLevel ); }
		}

		[DependsUpon( "InfoUpdateFlag" )]
		public double RightLevel {
			get { return( mNoiseManager.PlayController.RightLevel ); }
		}

		public void Execute_Play( object sender ) {
			mNoiseManager.PlayController.Play();
		}
		[DependsUpon( "CurrentStatus" )]
		[DependsUpon( "PlayQueueChangedFlag" )]
		public bool CanExecute_Play( object sender ) {
			return( mNoiseManager.PlayController.CanPlay );
		}

		public void Execute_Pause( object sender ) {
			mNoiseManager.PlayController.Pause();
		}
		[DependsUpon( "CurrentStatus" )]
		public bool CanExecute_Pause( object sender ) {
			return( mNoiseManager.PlayController.CanPause );
		}

		public void Execute_Stop( object sender ) {
			mNoiseManager.PlayController.Stop();
		}
		[DependsUpon( "CurrentStatus" )]
		public bool CanExecute_Stop( object sender ) {
			return( mNoiseManager.PlayController.CanStop );
		}

		public void Execute_NextTrack( object sender ) {
			mNoiseManager.PlayController.PlayNextTrack();
		}
		[DependsUpon( "PlayQueueChangedFlag" )]
		[DependsUpon( "StartTrackFlag" )]
		public bool CanExecute_NextTrack( object sender ) {
			return( mNoiseManager.PlayController.CanPlayNextTrack );
		}

		public void Execute_PreviousTrack( object sender ) {
			mNoiseManager.PlayController.PlayPreviousTrack();
		}
		[DependsUpon( "PlayQueueChangedFlag" )]
		[DependsUpon( "StartTrackFlag" )]
		public bool CanExecute_PreviousTrack( object sender ) {
			return( mNoiseManager.PlayController.CanPlayPreviousTrack );
		}

		public void Execute_ClearQueue( object sender ) {
			if( mNoiseManager != null ) {
				mNoiseManager.PlayQueue.ClearQueue();
			}
		}
		[DependsUpon( "PlayQueueChangedFlag" )]
		public bool CanExecute_ClearQueue( object sender ) {
			var retValue = true;

			if( mNoiseManager != null ) {
				retValue = !mNoiseManager.PlayQueue.IsQueueEmpty;
			}

			return( retValue );
		}
	}
}
