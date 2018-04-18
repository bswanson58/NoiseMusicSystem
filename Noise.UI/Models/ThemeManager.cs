﻿using System.Collections.Generic;
using System.Linq;
using System.Windows;
using MahApps.Metro;

namespace Noise.UI.Models {
    public class ThemeManager {
        private AppTheme                        mCurrentTheme;
        private Accent                          mCurrentAccent;
        private readonly IEnumerable<AppTheme>  mAvailableThemes;
        private readonly IEnumerable<Accent>    mAvailableAccents;

        public ThemeManager() {
            var current = MahApps.Metro.ThemeManager.DetectAppStyle( Application.Current );

            mCurrentTheme = current.Item1;
            mCurrentAccent = current.Item2;

            mAvailableThemes = MahApps.Metro.ThemeManager.AppThemes;
            mAvailableAccents = MahApps.Metro.ThemeManager.Accents;
        }

        public IEnumerable<string> AvailableThemes => from t in mAvailableThemes select t.Name;
        public IEnumerable<string> AvailableAccents => from a in mAvailableAccents select a.Name;

        public string   CurrentTheme => mCurrentTheme.Name;
        public string   CurrentAccent => mCurrentAccent.Name;

        public void SetApplicationTheme( string themeName, string accentName ) {
            var theme = MahApps.Metro.ThemeManager.GetAppTheme( themeName );
            var accent = MahApps.Metro.ThemeManager.GetAccent( accentName );

            if(( theme != null ) &&
               ( accent != null )) {
                MahApps.Metro.ThemeManager.ChangeAppStyle( Application.Current, mCurrentAccent, mCurrentTheme );

                mCurrentTheme = theme;
                mCurrentAccent = accent;
            }
        }
     }
}
