using Noise.Infrastructure.Dto;
using Noise.UI.Support;

namespace Noise.UI.ViewModels {
    public class TrackStrategyOptionsDialogModel : DialogModelBase {
        public  DbTrack    Track { get; }
        
        public  bool       PlayNext { get; set; }
        public  bool       PlayPrevious { get; set; }

        public TrackStrategyOptionsDialogModel( DbTrack forTrack ) {
            Track = forTrack;

            switch( Track.PlayStrategyOptions ) {
                case ePlayAdjacentStrategy.PlayNext:
                    PlayNext = true;
                    break;

                case ePlayAdjacentStrategy.PlayPrevious:
                    PlayPrevious = true;
                    break;

                case ePlayAdjacentStrategy.PlayNextPrevious:
                    PlayNext = true;
                    PlayPrevious = true;
                    break;
            }
        }
    }
}
