using Noise.Infrastructure.Dto;

namespace Noise.TenFoot.Ui.Input {
	public class Events {
		public class NavigateHome { }

		public class NavigateToScreen {
			public	object	ToScreen { get; private set; }

			public NavigateToScreen( object toScreen ) {
				ToScreen = toScreen;
			}
		}

		public class NavigateReturn {
			public	object	FromScreen { get; private set; }
			public	bool	CloseScreen { get; private set; }

			public NavigateReturn( object fromScreen, bool closeScreen ) {
				FromScreen = fromScreen;
				CloseScreen = closeScreen;
			}
		}

		public class DequeueTrack {
			public DbTrack	Track { get; private set; }

			public DequeueTrack( DbTrack track ) {
				Track = track;
			}
		}

		public class DequeueAlbum {
			public DbAlbum	Album { get; private set; }

			public DequeueAlbum( DbAlbum album ) {
				Album = album;
			}
		}

		public class UserNotification {
			public string	NotificationContent { get; private set; }

			public UserNotification( string text ) {
				NotificationContent = text;
			}
		}
	}
}
