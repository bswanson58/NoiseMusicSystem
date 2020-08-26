using System.Drawing;

namespace LightPipe.Dto {
    public class ZoneDefinition {
        public  string      ZoneName { get; }
        public  RectangleF  ZoneArea { get; }

        public ZoneDefinition( string name, RectangleF area ) {
            ZoneName = name;
            ZoneArea = area;
        }

        public bool ContainsPoint( PointF point ) {
            return ZoneArea.Contains( point );
        }
    }
}
