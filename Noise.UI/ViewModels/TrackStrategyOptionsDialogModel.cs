using Noise.Infrastructure.Dto;
using Noise.UI.Support;

namespace Noise.UI.ViewModels {
    public class TrackStrategyOptionsDialogModel : DialogModelBase {
        private bool    mDoNotPlay;

        public  DbTrack Track { get; }
        
        public  bool    PlayNext { get; set; }
        public  bool    PlayPrevious { get; set; }
        public  bool    CanPlayAdjacent { get; set; }

        public TrackStrategyOptionsDialogModel( DbTrack forTrack ) {
            Track = forTrack;

            DoNotPlay = Track.DoNotStrategyPlay;

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

        public bool DoNotPlay {
            get => mDoNotPlay;
            set {
                mDoNotPlay = value;

                CanPlayAdjacent = !mDoNotPlay;
                if(!CanPlayAdjacent ) {
                    PlayNext = false;
                    PlayPrevious = false;
                }

                RaisePropertyChanged( () => CanPlayAdjacent );
                RaisePropertyChanged( () => PlayNext );
                RaisePropertyChanged( () => PlayPrevious );
            }
        }
    }
}
