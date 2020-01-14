﻿using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Infrastructure.Configuration {
	public class NoiseCorePreferences {
		public	bool					        DisplayPlayTimeElapsed { get; set; }
		public	bool					        EnableRemoteAccess { get; set; }
		public	bool					        EnablePlaybackScrobbling { get; set; }
		public	bool					        HasNetworkAccess { get; set; }
		public	long					        LastLibraryUsed { get; set; }
		public	bool					        LoadLastLibraryOnStartup { get; set; }

		public	bool					        MaintainArtistSidecars { get; set; }
		public	bool					        MaintainAlbumSidecars { get; set; }

		public	ePlayStrategy			        PlayStrategy { get; set; }
		public	string					        PlayStrategyParameters { get; set; }
        public  ExhaustedStrategySpecification  ExhaustedStrategySpecification { get; set; }

		public	bool					        DeletePlayedTracks { get; set;}
		public	int						        MaximumPlayedTracks { get; set; }

		public	int								MaximumBackupPressure { get; set; }

		public NoiseCorePreferences() {
			PlayStrategy = ePlayStrategy.Next;
			PlayStrategyParameters = string.Empty;
            ExhaustedStrategySpecification = new ExhaustedStrategySpecification();

			HasNetworkAccess = true;
			LoadLastLibraryOnStartup = true;

			MaintainArtistSidecars = false;
			MaintainAlbumSidecars = true;

			DeletePlayedTracks = true;
			MaximumPlayedTracks = 12;

			EnablePlaybackScrobbling = false;
			EnableRemoteAccess = false;

			MaximumBackupPressure = 50;
		}
	}
}
