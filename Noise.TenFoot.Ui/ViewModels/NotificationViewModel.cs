using Caliburn.Micro;
using Microsoft.Practices.Prism.Commands;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Events = Noise.TenFoot.Ui.Input.Events;

namespace Noise.TenFoot.Ui.ViewModels {
	public class NotificationViewModel : Screen,
										 IHandle<Events.UserNotification>, IHandle<Events.DequeueAlbum>, IHandle<Events.DequeueTrack> {
		private const string				cSilentState = "Silent";
		private const string				cNotifyState = "Notifying";

		private	readonly IEventAggregator			mEventAggregator;
		private readonly DelegateCommand<DbTrack>	mPlayTrackCommand;
		private readonly DelegateCommand<DbAlbum>	mPlayAlbumCommand; 
		private string								mNotificationContent;
		private string								mVisualStateName;

		public NotificationViewModel( IEventAggregator eventAggregator ) {
			mEventAggregator = eventAggregator;

			mNotificationContent = string.Empty;
			mVisualStateName = cSilentState;

			mPlayTrackCommand = new DelegateCommand<DbTrack>( OnPlayTrack );
			GlobalCommands.PlayTrack.RegisterCommand( mPlayTrackCommand );

			mPlayAlbumCommand = new DelegateCommand<DbAlbum>( OnPlayAlbum );
			GlobalCommands.PlayAlbum.RegisterCommand( mPlayAlbumCommand );

			mEventAggregator.Subscribe( this );
		}

		private void OnPlayTrack( DbTrack track ) {
			if( track != null ) {
				SendNotification( string.Format( "Added track '{0}' to play list.", track.Name ));
			}
		}

		private void OnPlayAlbum( DbAlbum album ) {
			if( album != null ) {
				SendNotification( string.Format( "Added album '{0}' to play list.", album.Name ));
			}
		}

		public void Handle( Events.UserNotification args ) {
			SendNotification( args.NotificationContent );
		}

		public void Handle( Events.DequeueAlbum args ) {
			SendNotification( string.Format( "Removing album '{0}' from play list.", args.Album.Name ));
		}

		public void Handle( Events.DequeueTrack args ) {
			SendNotification( string.Format( "Removing track '{0}' from play list.", args.Track.Name ));
		}

		private void SendNotification( string text ) {
			mVisualStateName = cSilentState;
			NotifyOfPropertyChange( () => VisualStateName );

			mNotificationContent = text;
			mVisualStateName = cNotifyState;

			NotifyOfPropertyChange( () => NotificationContent );
			NotifyOfPropertyChange( () => VisualStateName );
		}

		public string VisualStateName {
			get{ return( mVisualStateName ); }
		}

		public string NotificationContent {
			get{ return( mNotificationContent ); }
		}
	}
}
