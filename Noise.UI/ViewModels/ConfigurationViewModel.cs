using System;
using System.Collections.Generic;
using System.Linq;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Models;
using Noise.UI.Support;

namespace Noise.UI.ViewModels {
    public class ConfigurationViewModel : DialogModelBase {
        private readonly IPreferences   mPreferences;
        private readonly ThemeManager   mThemeManager;
        private readonly ThemeCatalog   mThemeCatalog;

        public bool EnableGlobalHotkeys { get; set; }
        public bool EnableRemoteAccess { get; set; }
        public bool EnableSpeechCommands { get; set; }
        public bool EnableSortPrefixes { get; set; }
        public bool HasNetworkAccess { get; set; }
        public bool LoadLastLibraryOnStartup { get; set; }
        public bool MinimizeToTray { get; set; }
        public string SortPrefixes { get; set; }

        public IEnumerable<ThemeColors>     AvailableThemes => mThemeCatalog.Themes;
        public IEnumerable<AccentColors>    AvailableAccents => mThemeCatalog.Accents;
        public IEnumerable<SignatureColors> AvailableSignatures => mThemeCatalog.Signatures;

        public string           CurrentThemeId { get; set; }
        public string           CurrentAccentId { get; set; }
        public string           CurrentSignatureId { get; set; }

        public ConfigurationViewModel( IPreferences preferences ) {
            mPreferences = preferences;

            mThemeManager = new ThemeManager();
            mThemeCatalog = new ThemeCatalog();
            CurrentThemeId = mThemeManager.CurrentTheme;
            CurrentAccentId = mThemeManager.CurrentAccent;

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

            if( String.IsNullOrWhiteSpace( CurrentThemeId )) {
                CurrentThemeId = interfacePreferences.ThemeName;
            }

            if( String.IsNullOrWhiteSpace( CurrentAccentId )) {
                CurrentAccentId = interfacePreferences.ThemeAccent;
            }

            var signature = AvailableSignatures.FirstOrDefault( s => s.Location.Equals( interfacePreferences.ThemeSignature ));
            CurrentSignatureId = signature?.Id;
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

            interfacePreferences.ThemeName = CurrentThemeId;
            interfacePreferences.ThemeAccent = CurrentAccentId;

            var signature = AvailableSignatures.FirstOrDefault( s => s.Id.Equals( CurrentSignatureId ));
            interfacePreferences.ThemeSignature = signature?.Location;

            mPreferences.Save( corePreferences );
            mPreferences.Save( interfacePreferences );

            mThemeManager.UpdateApplicationTheme( CurrentThemeId, CurrentAccentId, signature?.Location );
        }
    }
}

