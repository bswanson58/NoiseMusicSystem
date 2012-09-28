using Noise.Infrastructure.Dto;

namespace Noise.TenFoot.Ui.Input {
	public class Events {
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
	}
}
