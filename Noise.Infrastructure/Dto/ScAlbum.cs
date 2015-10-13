﻿using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Noise.Infrastructure.Dto {
	[DebuggerDisplay("ScAlbum = {AlbumName}")]
	public class ScAlbum {
		public string			AlbumName { get; set; }
		public bool				IsFavorite { get; set; }
		public Int16			Rating { get; set; }
		public Int32			PublishedYear { get; set; }
		public float			ReplayGainAlbumGain { get; set; }
		public float			ReplayGainAlbumPeak { get; set; }
		public long				Version { get; set; }
		public List<ScTrack>	TrackList { get; set; }
		public ScPlayContext	PlaybackContext {  get; set; }

		public ScAlbum() {
			AlbumName = string.Empty;
			TrackList = new List<ScTrack>();
		} 

		public ScAlbum( DbAlbum album ) :
			this() {
			AlbumName = album.Name;

			IsFavorite = album.IsFavorite;
			Rating = album.Rating;
			PublishedYear = album.PublishedYear;
			ReplayGainAlbumGain = album.ReplayGainAlbumGain;
			ReplayGainAlbumPeak = album.ReplayGainAlbumPeak;
			Version = album.Version;
		}

		public void UpdateAlbum( DbAlbum album ) {
			album.IsFavorite = IsFavorite;
			album.Rating = Rating;
			album.PublishedYear = PublishedYear;
			album.ReplayGainAlbumGain = ReplayGainAlbumGain;
			album.ReplayGainAlbumPeak = ReplayGainAlbumPeak;
			album.Version = Version;
		}

		public override string ToString() {
			return( string.Format( "ScAlbum \"{0}\", Version:{1}", AlbumName, Version ));
		}
	}
}
