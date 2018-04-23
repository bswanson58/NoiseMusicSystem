using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using MahApps.Metro;

namespace Noise.UI.Models {
    public class ThemeColors {
        private readonly AppTheme   mTheme;

        public string   Name { get; }
        public string   Id => mTheme.Name;

        public ThemeColors( AppTheme theme ) {
            mTheme = theme;

            Name = mTheme.Name.Replace( "Base", String.Empty );
        }
    }

    public class AccentColors {
        private readonly Accent     mAccent;

        public string   Name { get; }
        public string   Id => mAccent.Name;
        public Color    Color { get; }

        public AccentColors( Accent accent ) {
            mAccent = accent;

            Name = mAccent.Name;

            try {
                var color = mAccent.Resources["AccentColor"];

                if( color != null ) {
                    Color = (Color)color;
                }
            }
            catch( Exception ) {
                Color = Colors.Black;
            }
        }
    }

    public class SignatureColors {
        public String   Name { get; }
        public String   Id { get; }
        public String   Location { get; }

        public SignatureColors( string name, string location ) {
            Name = name;
            Id = name;
            Location = location;
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
                                    new SignatureColors( "Orange", "pack://application:,,,/Noise.UI.Style;component/Themes/Signature_Orange.xaml" ),
                                    new SignatureColors( "Cobalt", "pack://application:,,,/Noise.UI.Style;component/Themes/Signature_Cobalt.xaml" )
                                };
        }
    }
}
