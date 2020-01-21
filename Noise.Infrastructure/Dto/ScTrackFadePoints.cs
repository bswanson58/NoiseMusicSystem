namespace Noise.Infrastructure.Dto {
    public class ScTrackFadePoints {
        public  long    FadeInPoint { get; set; }
        public  long    FadeOutPoint { get; set; }

        public  ScTrackFadePoints() {
            FadeInPoint = 0L;
            FadeOutPoint = 0L;
        }
    }
}
