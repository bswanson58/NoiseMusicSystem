using System.Collections.Generic;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayStrategies.Exhausted {
    interface IExhaustedSelectionContext {
        IPlayQueue      PlayQueue { get; }

        DbArtist        Artist { get; set; }
        DbAlbum         Album { get; set; }
        IList<DbTrack>  AlbumTracks { get; }
        IList<DbTrack>  SelectedTracks { get; }
    }
}
