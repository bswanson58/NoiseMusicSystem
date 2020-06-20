using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using ControlzEx.Theming;

namespace MilkBottle.Themes {
    public abstract class ColorBase {
        public String   Name { get; protected set; }
        public String   Id { get; protected set; }
        public Color    Color { get; protected set; }
    }

    public class ThemeColors : ColorBase {
        public ThemeColors( Theme theme ) {
            Id = theme.Name;
            Name = theme.DisplayName;
            Color = theme.PrimaryAccentColor;
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
        public IEnumerable<SignatureColors> Signatures { get; }

        public ThemeCatalog() {
            Themes = from t in ControlzEx.Theming.ThemeManager.Current.Themes select new ThemeColors( t );

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
