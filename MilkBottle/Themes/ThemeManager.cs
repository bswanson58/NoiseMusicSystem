using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using ControlzEx.Theming;
using ReusableBits.Ui.Themes;

namespace MilkBottle.Themes {
    public class ThemeManager {
        private Theme                           mCurrentTheme;
        private readonly IEnumerable<Theme>     mAvailableThemes;

        public ThemeManager() {
            var current = ControlzEx.Theming.ThemeManager.Current.DetectTheme( Application.Current );

            mCurrentTheme = current;

            mAvailableThemes = ControlzEx.Theming.ThemeManager.Current.Themes;
        }

        public IEnumerable<string>  AvailableThemes => from t in mAvailableThemes select t.Name;
        public string               CurrentTheme => mCurrentTheme?.Name;

        public void UpdateApplicationTheme( string themeName, string signatureName ) {
            if(!String.IsNullOrWhiteSpace( themeName )) {
                var theme = ControlzEx.Theming.ThemeManager.Current.GetTheme( themeName );

                if( theme != null ) {
                    SetApplicationTheme( theme );

                    mCurrentTheme = theme;
                }
            }

            if(!String.IsNullOrWhiteSpace( signatureName )) {
                SetApplicationResources( Application.Current.MainWindow, new Uri( signatureName ));
            }
        }

        public static void SetApplicationTheme( string themeName, string signatureUri ) {
            if(!String.IsNullOrWhiteSpace( themeName )) {
                SetApplicationTheme( ControlzEx.Theming.ThemeManager.Current.GetTheme( themeName ));
            }
            if(!String.IsNullOrWhiteSpace( signatureUri )) {
                SetApplicationResources( Application.Current.MainWindow, new Uri( signatureUri ));
            }
        }

        public static void SetApplicationTheme( Theme theme ) {
            if( theme != null ) {
             
                ControlzEx.Theming.ThemeManager.Current.ChangeTheme( Application.Current, theme );
            }
        }

        public static void SetApplicationResources( FrameworkElement element, Uri resources ) {
            ThemeSelector.ApplyTheme( element, resources );
        }
     }
}
