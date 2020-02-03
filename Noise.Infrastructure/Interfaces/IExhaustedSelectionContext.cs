using System.Collections.Generic;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
    public interface IExhaustedSelectionContext {
        IPlayQueue                          PlayQueue { get; }

        IList<DbTrack>                      SelectedTracks { get; }
        IDictionary<long, IList<string>>    BreadCrumbs { get; }

        long                                SuggesterParameter { get; }
    }
}
