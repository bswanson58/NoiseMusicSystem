using System.Collections.Generic;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayStrategies.Exhausted {
    class ExhaustedSelectionContext : IExhaustedSelectionContext {
        public  IPlayQueue                          PlayQueue { get; }

        public  IDictionary<long, IList<string>>    BreadCrumbs { get; }
        public  IList<DbTrack>                      SelectedTracks { get; }

        public  long                                SuggesterParameter { get; }

        public ExhaustedSelectionContext( IPlayQueue playQueue, long parameter ) {
            PlayQueue = playQueue;

            BreadCrumbs = new Dictionary<long, IList<string>>();
            SelectedTracks = new List<DbTrack>();

            SuggesterParameter = parameter;
        }
    }
}
