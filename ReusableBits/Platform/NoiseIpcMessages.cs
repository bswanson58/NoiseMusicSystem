﻿using System;

namespace ReusableBits.Platform {
    public static class NoiseIpcSubject {
        public const string cCompanionApplication = "companionApplication";
        public const string cActivateApplication = "activateApplication";
        public const string cPlaybackEvent = "playbackEvent";
    }

    public class CompanionApplication {
        public  string  Name { get; set; }
        public  string  Icon { get; set; }

        public CompanionApplication() {
            Name = String.Empty;
            Icon = String.Empty;
        }

        public CompanionApplication( string name, string icon ) {
            Name = name;
            Icon = icon;
        }
    }

    public class ActivateApplication { }

    public class PlaybackEvent {
        public  string      ArtistName { get; set; }
        public  string      AlbumName { get; set; }
        public  string      TrackName {  get; set; }
        public  string      ArtistGenre { get; set; }
        public  string[]    TrackTags { get; set; }
        public  int         TrackRating { get; set; }
        public  uint        TrackLength { get; set; }
        public  bool        IsFavorite { get; set; }
        public  int         PublishedYear { get; set; }

        public PlaybackEvent() {
            ArtistName = String.Empty;
            AlbumName = String.Empty;
            TrackName = String.Empty;
            ArtistGenre = String.Empty;
            TrackTags = new string[] {};
            TrackRating = 0;
            TrackLength = 0;
            IsFavorite = false;
            PublishedYear = 0;
        }
    }
}
