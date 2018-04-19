using System;
using System.Collections.Generic;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Models;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
    public class ConfigurationViewModel : PropertyChangeBase {
        private readonly IPreferences   mPreferences;
        private readonly ThemeManager   mThemeManager;

        public bool EnableGlobalHotkeys { get; set; }
        public bool EnableRemoteAccess { get; set; }
        public bool EnableSpeechCommands { get; set; }
        public bool EnableSortPrefixes { get; set; }
        public bool HasNetworkAccess { get; set; }
        public bool LoadLastLibraryOnStartup { get; set; }
        public bool MinimizeToTray { get; set; }
        public string SortPrefixes { get; set; }

        public IEnumerable<string>  AvailableThemes => mThemeManager.AvailableThemes;
        public IEnumerable<string>  AvailableAccents => mThemeManager.AvailableAccents;

        public string   CurrentTheme { get; set; }
        public string   CurrentAccent { get; set; }

        public ConfigurationViewModel( IPreferences preferences ) {
            mPreferences = preferences;

            mThemeManager = new ThemeManager();
            CurrentTheme = mThemeManager.CurrentTheme;
            CurrentAccent = mThemeManager.CurrentAccent;

            var interfacePreferences = mPreferences.Load<UserInterfacePreferences>();
            var corePreferences = mPreferences.Load<NoiseCorePreferences>();

            EnableGlobalHotkeys = interfacePreferences.EnableGlobalHotkeys;
            EnableRemoteAccess = corePreferences.EnableRemoteAccess;
            EnableSpeechCommands = corePreferences.EnableSpeechCommands;
            EnableSortPrefixes = interfacePreferences.EnableSortPrefixes;
            HasNetworkAccess = corePreferences.HasNetworkAccess;
            LoadLastLibraryOnStartup = corePreferences.LoadLastLibraryOnStartup;
            MinimizeToTray = interfacePreferences.MinimizeToTray;
            SortPrefixes = interfacePreferences.SortPrefixes;

            if( String.IsNullOrWhiteSpace( CurrentTheme )) {
                CurrentTheme = interfacePreferences.ThemeName;
            }
            if( String.IsNullOrWhiteSpace( CurrentAccent )) {
                CurrentAccent = interfacePreferences.ThemeAccent;
            }
        }

        public void UpdatePreferences() {
            var interfacePreferences = mPreferences.Load<UserInterfacePreferences>();
            var corePreferences = mPreferences.Load<NoiseCorePreferences>();
            
            corePreferences.EnableRemoteAccess = EnableRemoteAccess;
            corePreferences.EnableSpeechCommands = EnableSpeechCommands;
            corePreferences.HasNetworkAccess = HasNetworkAccess;
            corePreferences.LoadLastLibraryOnStartup = LoadLastLibraryOnStartup;

            interfacePreferences.EnableGlobalHotkeys = EnableGlobalHotkeys;
            interfacePreferences.EnableSortPrefixes = EnableSortPrefixes;
            interfacePreferences.SortPrefixes = SortPrefixes;
            interfacePreferences.MinimizeToTray = MinimizeToTray;

            interfacePreferences.ThemeName = CurrentTheme;
            interfacePreferences.ThemeAccent = CurrentAccent;

            mPreferences.Save( corePreferences );
            mPreferences.Save( interfacePreferences );

            mThemeManager.UpdateApplicationTheme( CurrentTheme, CurrentAccent );
        }
    }
}

