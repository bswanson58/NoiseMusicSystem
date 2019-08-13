using System.Collections.Generic;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
    public interface IExhaustedSelectionContext {
        IPlayQueue      PlayQueue { get; }

        DbArtist        Artist { get; set; }
        DbAlbum         Album { get; set; }
        IList<DbTrack>  AlbumTracks { get; }
        IList<DbTrack>  SelectedTracks { get; }

        long            SuggesterParameter { get; }
    }
}
