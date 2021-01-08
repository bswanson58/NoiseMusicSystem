using System;
using System.Linq;
using System.Reflection;
using Noise.RemoteClient.Interfaces;
using Noise.RemoteClient.Resources;
using Noise.RemoteClient.Support;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Noise.RemoteClient.Platform {
    class FontResources : ResourceDictionary { }
    class ThemeResources : ResourceDictionary { }

    class ThemeManager {
        public static void ChangeFontResource( ThemeResource toTheme ) {
            var dictionary = Application.Current.Resources.MergedDictionaries.FirstOrDefault( d => d is FontResources );

            if( dictionary != null ) {
                var source = new Uri( toTheme.Source, UriKind.RelativeOrAbsolute );

                dictionary.SetAndLoadSource( source, toTheme.Source, typeof( ThemeManager ).GetTypeInfo().Assembly, null );
            }
        }

        public static void ChangeThemeResource( ThemeResource toTheme ) {
            var dictionary = Application.Current.Resources.MergedDictionaries.FirstOrDefault( d => d is ThemeResources );

            if( dictionary != null ) {
                var source = new Uri( toTheme.Source, UriKind.RelativeOrAbsolute );

                dictionary.SetAndLoadSource( source, toTheme.Source, typeof( ThemeManager ).GetTypeInfo().Assembly, null );
            }

            Application.Current.Resources.TryGetValue( "NavigationBarColor", out var color );
            if( color is ColorReference barColor ) {
                var platform = DependencyService.Get<IPlatformSupport>();

                platform?.SetStatusBarColor( barColor.Color );
            }
        }

        public static void LoadInitialTheme() {
            var fontSize = Preferences.Get( PreferenceNames.ApplicationFont, ThemeCatalog.DefaultFont );
            var fontTheme = ThemeCatalog.FontThemes.FirstOrDefault( f => f.ResourceId.Equals( fontSize ));

            if( fontTheme != null ) {
                ChangeFontResource( fontTheme );
            }

            var theme = Preferences.Get( PreferenceNames.ApplicationTheme, ThemeCatalog.DefaultTheme );
            var themeResource = ThemeCatalog.ThemeResources.FirstOrDefault( t => t.ResourceId.Equals( theme ));

            if( themeResource != null ) {
                ChangeThemeResource( themeResource );
            }
        }
    }
}