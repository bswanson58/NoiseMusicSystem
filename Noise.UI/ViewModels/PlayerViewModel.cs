using System;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;

namespace Noise.UI.ViewModels {
	public class ExhaustedStrategyItem {
		public string		Title { get; private set; }
		public ePlayExhaustedStrategy	Strategy { get; private set; }

		public ExhaustedStrategyItem( ePlayExhaustedStrategy strategy, string title ) {
			Strategy = strategy;
			Title = title;
		}
	}

	public class PlayerViewModel : ViewModelBase {
		private	IUnityContainer			mContainer;
		private IEventAggregator		mEvents;
		private INoiseManager			mNoiseManager;

		private	readonly ObservableCollectionEx<ExhaustedStrategyItem>	mExhaustedStrategies;

		public PlayerViewModel() {
			mExhaustedStrategies = new ObservableCollectionEx<ExhaustedStrategyItem>{
												new ExhaustedStrategyItem( ePlayExhaustedStrategy.Stop, "Stop" ),
												new ExhaustedStrategyItem( ePlayExhaustedStrategy.Replay, "Replay" ),
												new ExhaustedStrategyItem( ePlayExhaustedStrategy.PlayFavorites, "Play Favorites" ) };
		}

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

				var	systemConfig = mContainer.Resolve<ISystemConfiguration>();
				var configuration = systemConfig.RetrieveConfiguration<ExplorerConfiguration>( ExplorerConfiguration.SectionName );

				if( configuration != null ) {
					mNoiseManager.PlayQueue.PlayExhaustedStrategy = configuration.PlayExhaustedStrategy;
				}
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

		public void Execute_ToggleTimeDisplay() {
			mNoiseManager.PlayController.ToggleTimeDisplay();
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

		public ObservableCollectionEx<ExhaustedStrategyItem> ExhaustedStrategyList {
			get{ return( mExhaustedStrategies ); }
		}

		public ePlayExhaustedStrategy ExhaustedStrategy {
			get{ return( mNoiseManager.PlayQueue.PlayExhaustedStrategy ); }
			set {
				mNoiseManager.PlayQueue.PlayExhaustedStrategy = value;

				var	systemConfig = mContainer.Resolve<ISystemConfiguration>();
				var configuration = systemConfig.RetrieveConfiguration<ExplorerConfiguration>( ExplorerConfiguration.SectionName );

				if( configuration != null ) {
					configuration.PlayExhaustedStrategy = value;

					systemConfig.Save( configuration );
				}
			}
		}

		[DependsUpon( "InfoUpdateFlag" )]
		public double LeftLevel {
			get { return( mNoiseManager.PlayController.LeftLevel ); }
		}

		[DependsUpon( "InfoUpdateFlag" )]
		public double RightLevel {
			get { return( mNoiseManager.PlayController.RightLevel ); }
		}

		[DependsUpon( "StartTrackFlag" )]
		public bool IsFavorite {
			get { return( mNoiseManager.PlayController.IsFavorite ); }
			set { mNoiseManager.PlayController.IsFavorite = value; }
		}

		[DependsUpon( "StartTrackFlag" )]
		public Int16 Rating {
			get{ return( mNoiseManager.PlayController.Rating ); }
			set { mNoiseManager.PlayController.Rating = value; }
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
