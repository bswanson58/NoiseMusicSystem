using System;
using System.Linq;
using System.Windows;

// from: http://svetoslavsavov.blogspot.com/2009/07/switching-wpf-interface-themes-at.html

//
// Add a property to the root element of the application such as:
//      Themes:ThemeSelector.CurrentThemeDictionary="{Binding CurrentTheme}"
// where CurrentTheme might be: "/ThemeSelector;component/Themes/ShinyBlue.xaml"
//
namespace ReusableBits.Ui.Themes {
    /// <summary>
    /// Inherits ResourceDictionary, used to identify a theme resource dictionary in the merged dictionaries collection
    /// </summary>
    public class ThemeResourceDictionary : ResourceDictionary { }

    public class ThemeSelector : DependencyObject {
        public static readonly DependencyProperty CurrentThemeDictionaryProperty =
            DependencyProperty.RegisterAttached("CurrentThemeDictionary", typeof( Uri ),
            typeof( ThemeSelector ),
            new UIPropertyMetadata( null, CurrentThemeDictionaryChanged ));

        public static Uri GetCurrentThemeDictionary( DependencyObject obj ) {
            return (Uri)obj.GetValue(CurrentThemeDictionaryProperty);
        }

        public static void SetCurrentThemeDictionary( DependencyObject obj, Uri value ) {
            obj.SetValue( CurrentThemeDictionaryProperty, value );
        }

        private static void CurrentThemeDictionaryChanged( DependencyObject obj, DependencyPropertyChangedEventArgs e ) {
            // works only on FrameworkElement objects
            if( obj is FrameworkElement ) {
                ApplyTheme((FrameworkElement)obj, GetCurrentThemeDictionary( obj ));
            }
        }

        private static void ApplyTheme( FrameworkElement targetElement, Uri dictionaryUri ) {
            if( targetElement != null ) {
                try {
                    var existingDictionaries = ( from dictionary in targetElement.Resources.MergedDictionaries.OfType<ThemeResourceDictionary>() select dictionary ).ToList();

                    if (( dictionaryUri != null ) &&
                        ( dictionaryUri.IsWellFormedOriginalString()) &&
                        (!String.IsNullOrWhiteSpace( dictionaryUri.OriginalString )) &&
                        (!existingDictionaries.Any( td => td.Source.Equals( dictionaryUri )))) {
                        var themeDictionary = new ThemeResourceDictionary { Source = dictionaryUri };

                        // add the new dictionary to the collection of merged dictionaries of the target object
                        targetElement.Resources.MergedDictionaries.Insert(0, themeDictionary);
                    }

                    // remove any other existing theme dictionaries 
                    foreach( var thDictionary in existingDictionaries.Where( td => !td.Source.Equals( dictionaryUri ))) {
                        targetElement.Resources.MergedDictionaries.Remove( thDictionary );
                    }
                }
                finally { }
            }
        }
    }
}
