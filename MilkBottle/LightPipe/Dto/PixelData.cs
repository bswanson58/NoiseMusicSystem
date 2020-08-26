using System;
using System.Windows.Media;

namespace LightPipe.Dto {
    public class PixelData {
        public  int     Row { get; }
        public  int     Column { get; }
        public  byte    Blue { get; }
        public  byte    Green { get; }
        public  byte    Red { get; }
        public  byte    Alpha { get; }
        public  string  ZoneId { get; private set; }
        public  Color   ColorBin { get; private set; }
        
        public  bool    IsInZone => !String.IsNullOrWhiteSpace( ZoneId );

        public PixelData( int row, int column, byte b, byte g, byte r, byte a ) {
            Row = row;
            Column = column;
            Blue = b;
            Green = g;
            Red = r;
            Alpha = a;
        }

        public PixelData( int row, int column, byte[] colorComponents ) {
            Row = row;
            Column = column;
            Blue = colorComponents[0];
            Green = colorComponents[1];
            Red = colorComponents[2];
            Alpha = colorComponents[3];
        }

        public void SetZone( string zone ) {
            ZoneId = zone;
        }

        public void SetZone( ZoneDefinition zone ) {
            if( zone != null ) {
                ZoneId = zone.ZoneName;
            }
        }

        public void SetBin( Color bin ) {
            ColorBin = bin;
        }
    }
}
