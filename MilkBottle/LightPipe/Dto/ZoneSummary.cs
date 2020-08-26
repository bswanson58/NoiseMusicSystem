using System.Collections.Generic;
using System.Windows.Media;

namespace LightPipe.Dto {
    public class ColorBin {
        public  Color   Color { get; }
        public  int     Frequency { get; }

        public ColorBin( Color color, int frequency ) {
            Color = color;
            Frequency = frequency;
        }
    }

    public class ZoneSummary {
        public  string          ZoneId { get; }
        public  List<ColorBin>  Colors { get; }

        public ZoneSummary( string zoneId, IEnumerable<ColorBin> colors ) {
            ZoneId = zoneId;
            Colors = new List<ColorBin>( colors );
        }
    }
}
