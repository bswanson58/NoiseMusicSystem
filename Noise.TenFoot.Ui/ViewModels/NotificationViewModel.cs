using Caliburn.Micro;
using BaseEvents = Noise.Infrastructure.Events;
using Events = Noise.TenFoot.Ui.Input.Events;

namespace Noise.TenFoot.Ui.ViewModels {
	public class NotificationViewModel : Screen,
										 IHandle<Events.UserNotification>, IHandle<BaseEvents.TrackQueued>, IHandle<BaseEvents.AlbumQueued>,
										 IHandle<Events.DequeueAlbum>, IHandle<Events.DequeueTrack> {
		private const string				cSilentState = "Silent";
		private const string				cNotifyState = "Notifying";

		private	readonly IEventAggregator	mEventAggregator;
		private string						mNotificationContent;
		private string						mVisualStateName;

		public NotificationViewModel( IEventAggregator eventAggregator ) {
			mEventAggregator = eventAggregator;

			mNotificationContent = string.Empty;
			mVisualStateName = cSilentState;

			mEventAggregator.Subscribe( this );
		}

		public void Handle( BaseEvents.TrackQueued args ) {
			SendNotification( string.Format( "Added track '{0}' to play list.", args.QueuedTrack.Name ));
		}

		public void Handle( BaseEvents.AlbumQueued args ) {
			SendNotification( string.Format( "Added album '{0}' to play list.", args.QueuedAlbum.Name ));
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
