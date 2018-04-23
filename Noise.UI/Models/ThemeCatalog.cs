using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using MahApps.Metro;

namespace Noise.UI.Models {
    public abstract class ColorBase {
        public String   Name { get; protected set; }
        public String   Id { get; protected set; }
        public Color    Color { get; protected set; }
    }

    public class ThemeColors : ColorBase {
        public ThemeColors( AppTheme theme ) {
            Id = theme.Name;
            Name = theme.Name.Replace( "Base", String.Empty );
            try {
                var color = theme.Resources["WindowBackgroundBrush"];

                if( color is SolidColorBrush brush ) {
                    Color = brush.Color;
                }
            }
            catch( Exception ) {
                Color = Colors.Black;
            }
        }
    }

    public class AccentColors : ColorBase {
        public AccentColors( Accent accent ) {
            Id = accent.Name;
            Name = accent.Name;

            try {
                var color = accent.Resources["AccentColor"];

                if( color != null ) {
                    Color = (Color)color;
                }
            }
            catch( Exception ) {
                Color = Colors.Black;
            }
        }
    }

    public class SignatureColors : ColorBase {
        public String   Location { get; }

        public SignatureColors( string name, Color color, string location ) {
            Name = name;
            Id = name;
            Location = location;
            Color = color;
        }
    }

    public class ThemeCatalog {
        public IEnumerable<ThemeColors>     Themes { get; }
        public IEnumerable<AccentColors>    Accents { get; }
        public IEnumerable<SignatureColors> Signatures { get; }

        public ThemeCatalog() {
            Themes = from t in MahApps.Metro.ThemeManager.AppThemes select new ThemeColors( t );
            Accents = from a in MahApps.Metro.ThemeManager.Accents select  new AccentColors( a );

            Signatures = new List<SignatureColors> {
                                    new SignatureColors( "Orange", HexToColor( "#FFEA6500" ), "pack://application:,,,/Noise.UI.Style;component/Themes/Signature_Orange.xaml" ),
                                    new SignatureColors( "Cobalt", HexToColor( "#FF0047AB" ), "pack://application:,,,/Noise.UI.Style;component/Themes/Signature_Cobalt.xaml" )
                                };
        }

        private static Color HexToColor( string hexValue ) {
            var retValue = Colors.Black;
            var color = ColorConverter.ConvertFromString( hexValue );

            if( color != null ) {
                retValue = (Color)color;
            }

            return retValue;
        }
    }
}
