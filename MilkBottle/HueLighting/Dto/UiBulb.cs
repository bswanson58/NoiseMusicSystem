using System.Windows.Media;

namespace HueLighting.Dto {
    public class UiBulb {
        private readonly Bulb       mBulb;

        public  Color               LightColor { get; }

        public  double              Top => ( mBulb.Location.Y * 100 ) + 100;
        public  double              Left => ( mBulb.Location.X * 100 ) + 100;
        public  double              Height => mBulb.Location.Z + 2;

        public  string              Description => $"{mBulb.Name} ({mBulb.Id})";

        public UiBulb( Bulb bulb, Color color ) {
            mBulb = bulb;
            LightColor = color;
        }
    }
}
