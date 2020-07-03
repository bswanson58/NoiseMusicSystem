namespace Noise.Infrastructure.Dto {
    public class ScTrackPlayPoints {
        public  long    StartPlaySeconds { get; set; }
        public  long    StopPlaySeconds { get; set; }

        public  ScTrackPlayPoints() {
            StartPlaySeconds = 0L;
            StopPlaySeconds = 0L;
        }
    }
}
