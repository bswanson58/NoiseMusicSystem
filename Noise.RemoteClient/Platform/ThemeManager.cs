using System;
using System.Linq;
using System.Reflection;
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
        }

        public static void LoadInitialTheme() {
            var fontSize = Preferences.Get( PreferenceNames.ApplicationFont, ThemeCatalog.DefaultFont );
            var fontTheme = ThemeCatalog.FontThemes.FirstOrDefault( f => f.ResourceId.Equals( fontSize ));

            if( fontTheme != null ) {
                ChangeFontResource( fontTheme );
            }
        }
    }
}