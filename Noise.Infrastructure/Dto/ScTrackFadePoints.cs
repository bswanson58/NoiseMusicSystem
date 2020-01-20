namespace Noise.Infrastructure.Dto {
    public class ScTrackFadePoints {
        public  double      FadeInPoint { get; set; }
        public  double      FadeOutPoint { get; set; }
        public  double      FadeDuration {  get; set; }

        public  ScTrackFadePoints() {
            FadeInPoint = 0.0;
            FadeOutPoint = 0.0;
            FadeDuration = 1.0;
        }
    }
}
