using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using LightPipe.Utility;

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

        public List<Color> FindMeanColors( int maxColors ) {
            var retValue = new List<Color>();

            if( Colors.Count > maxColors ) {
                var sortedColors = ( from bin in Colors orderby HueOf( bin.Color ) select bin.Color ).ToList();
                var takeIndex = Colors.Count / (float)( maxColors - 1 );

                // Start with the predominate color
                retValue.Add( Colors.First().Color );
                retValue.AddRange( sortedColors.Where(( bin, index ) => index % (int)Math.Round( takeIndex ) == 0 ));

                if( retValue.Count > maxColors ) {
                    retValue.RemoveAt( retValue.Count - 1 );
                }
                if( retValue.Count < maxColors ) {
                    retValue.Add( sortedColors.Last());
                }
            }
            else {
                retValue.AddRange( from bin in Colors select bin.Color );
            }

            return retValue;
        }

        private double HueOf( Color color ) {
            ColorSpace.ColorToHSV( color, out var hue, out var saturation, out var value );

            return hue;
        }
    }
}
