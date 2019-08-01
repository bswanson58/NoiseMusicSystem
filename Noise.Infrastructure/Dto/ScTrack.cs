using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Noise.Infrastructure.Dto {
	[DebuggerDisplay("ScAlbum = {" + nameof( TrackName ) + "}")]
	public class ScTrack {
		public string			TrackName { get; set; }
		public Int16			TrackNumber { get; set; }
		public string			VolumeName { get; set; }
		public Int16			Rating { get; set; }
		public bool				IsFavorite { get; set; }
        public List<String>     Tags { get; set; }
		public float			ReplayGainAlbumGain { get; set; }
		public float			ReplayGainAlbumPeak { get; set; }
		public float			ReplayGainTrackGain { get; set; }
		public float			ReplayGainTrackPeak { get; set; }
		public ScPlayContext	PlaybackContext {  get; set; }

		public ScTrack() {
			TrackName = string.Empty;
			VolumeName = string.Empty;
            Tags = new List<string>();
		}

		public ScTrack( DbTrack track ) :
			this() {
			TrackName = track.Name;
			TrackNumber = track.TrackNumber;
			VolumeName = track.VolumeName;

			IsFavorite = track.IsFavorite;
			Rating = track.Rating;
			ReplayGainAlbumGain = track.ReplayGainAlbumGain;
			ReplayGainAlbumPeak = track.ReplayGainAlbumPeak;
			ReplayGainTrackGain = track.ReplayGainTrackGain;
			ReplayGainTrackPeak = track.ReplayGainTrackPeak;
		}

		public void UpdateTrack( DbTrack track ) {
			track.IsFavorite = IsFavorite;
			track.Rating = Rating;
			track.ReplayGainAlbumGain = ReplayGainAlbumGain;
			track.ReplayGainAlbumPeak = ReplayGainAlbumPeak;
			track.ReplayGainTrackGain = ReplayGainTrackGain;
			track.ReplayGainTrackPeak = ReplayGainTrackPeak;
		}

		public override string ToString() {
			return( $"ScTrack \"{TrackName}\", Track #{TrackNumber}, Volume \"{VolumeName}\"" );
		}
	}
}
