using Noise.Infrastructure.Interfaces;

namespace Noise.Infrastructure.Configuration {
	public class NoiseCorePreferences {
		public	bool					DisplayPlayTimeElapsed { get; set; }
		public	bool					EnableRemoteAccess { get; set; }
		public bool						EnableSpeechCommands { get; set; }
		public	bool					EnablePlaybackScrobbling { get; set; }
		public	bool					HasNetworkAccess { get; set; }
		public	long					LastLibraryUsed { get; set; }
		public	bool					LoadLastLibraryOnStartup { get; set; }

		public bool						MaintainArtistSidecars { get; set; }
		public bool						MaintainAlbumSidecars { get; set; }

		public	ePlayExhaustedStrategy	PlayExhaustedStrategy { get; set; }
		public	string					PlayExhaustedParameters { get; set; }
		public	ePlayStrategy			PlayStrategy { get; set; }
		public	string					PlayStrategyParameters { get; set; }

		public bool						DeletePlayedTracks { get; set;}
		public int						MaximumPlayedTracks { get; set; }

		public NoiseCorePreferences() {
			PlayExhaustedStrategy = ePlayExhaustedStrategy.PlayFavorites;
			PlayExhaustedParameters = string.Empty;
			PlayStrategy = ePlayStrategy.Next;
			PlayStrategyParameters = string.Empty;

			HasNetworkAccess = true;
			LoadLastLibraryOnStartup = true;

			MaintainArtistSidecars = false;
			MaintainAlbumSidecars = true;

			DeletePlayedTracks = true;
			MaximumPlayedTracks = 14;

			EnablePlaybackScrobbling = false;
			EnableRemoteAccess = false;
			EnableSpeechCommands = false;
		}
	}
}
