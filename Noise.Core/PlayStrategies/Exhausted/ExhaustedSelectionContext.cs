using System.Collections.Generic;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayStrategies.Exhausted {
    class ExhaustedSelectionContext : IExhaustedSelectionContext {
        public  IPlayQueue      PlayQueue { get; }

        public  DbArtist        Artist { get; set; }
        public  DbAlbum         Album { get; set; }
        public  IList<DbTrack>  AlbumTracks { get; }

        public  IList<DbTrack>  SelectedTracks { get; }

        public ExhaustedSelectionContext( IPlayQueue playQueue ) {
            PlayQueue = playQueue;

            AlbumTracks = new List<DbTrack>();
            SelectedTracks = new List<DbTrack>();
        }
    }
}
