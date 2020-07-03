namespace Noise.Infrastructure.Configuration {
	public class NoiseCorePreferences {
		public	int								DisplayPlayTimeMode { get; set; }
		public	bool					        EnableRemoteAccess { get; set; }
		public	bool					        EnablePlaybackScrobbling { get; set; }
		public	bool					        HasNetworkAccess { get; set; }
		public	long					        LastLibraryUsed { get; set; }
		public	bool					        LoadLastLibraryOnStartup { get; set; }

		public	bool					        MaintainArtistSidecars { get; set; }
		public	bool					        MaintainAlbumSidecars { get; set; }

		public	bool					        DeletePlayedTracks { get; set;}
		public	int						        MaximumPlayedTracks { get; set; }

		public	int								MaximumBackupPressure { get; set; }
		public	bool							EnforceBackupCopyLimit { get; set; }
		public	int								MaximumBackupCopies { get; set; }

		public NoiseCorePreferences() {
			HasNetworkAccess = true;
			LoadLastLibraryOnStartup = true;

			MaintainArtistSidecars = false;
			MaintainAlbumSidecars = true;

			DeletePlayedTracks = true;
			MaximumPlayedTracks = 12;

			EnablePlaybackScrobbling = false;
			EnableRemoteAccess = false;

			MaximumBackupPressure = 50;
			EnforceBackupCopyLimit = true;
			MaximumBackupCopies = 10;
		}
	}
}
