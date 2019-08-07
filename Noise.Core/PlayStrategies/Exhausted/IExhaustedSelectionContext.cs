using System.Collections.Generic;
using Noise.Infrastructure.Dto;

namespace Noise.Core.PlayStrategies.Exhausted {
    interface IExhaustedSelectionContext {
        DbArtist        Artist { get; set; }
        DbAlbum         Album { get; set; }
        IList<DbTrack>  AlbumTracks { get; }
        IList<DbTrack>  SelectedTracks { get; }
    }
}
