using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Models;
using Noise.UI.Support;

namespace Noise.UI.ViewModels {
    public class ConfigurationViewModel : DialogModelBase {
        private readonly IPreferences   mPreferences;
        private readonly ThemeManager   mThemeManager;
        private ThemeColors             mCurrentTheme;
        private AccentColors            mCurrentAccent;
        private SignatureColors         mCurrentSignature;

        public bool EnableGlobalHotkeys { get; set; }
        public bool EnableRemoteAccess { get; set; }
        public bool EnableSpeechCommands { get; set; }
        public bool EnableSortPrefixes { get; set; }
        public bool HasNetworkAccess { get; set; }
        public bool LoadLastLibraryOnStartup { get; set; }
        public bool MinimizeToTray { get; set; }
        public string SortPrefixes { get; set; }

        public BindableCollection<ThemeColors>      AvailableThemes { get; }
        public BindableCollection<AccentColors>     AvailableAccents { get; }
        public BindableCollection<SignatureColors>  AvailableSignatures { get; }

        public ThemeColors CurrentTheme {
            get => mCurrentTheme;
            set {
                mCurrentTheme = value;

                UpdateTheme();
            }
        }

        public AccentColors CurrentAccent {
            get => mCurrentAccent;
            set {
                mCurrentAccent = value;

                UpdateTheme();
            }
            
        }

        public SignatureColors CurrentSignature {
            get => mCurrentSignature;
            set {
                mCurrentSignature = value;

                UpdateTheme();
            }
        }

        public ConfigurationViewModel( IPreferences preferences ) {
            mPreferences = preferences;

            mThemeManager = new ThemeManager();
            var themeCatalog = new ThemeCatalog();

            AvailableThemes = new BindableCollection<ThemeColors>( themeCatalog.Themes );
            AvailableAccents = new BindableCollection<AccentColors>( themeCatalog.Accents );
            AvailableSignatures = new BindableCollection<SignatureColors>( themeCatalog.Signatures );

            var interfacePreferences = mPreferences.Load<UserInterfacePreferences>();
            var corePreferences = mPreferences.Load<NoiseCorePreferences>();

            EnableGlobalHotkeys = interfacePreferences.EnableGlobalHotkeys;
            EnableRemoteAccess = corePreferences.EnableRemoteAccess;
            EnableSortPrefixes = interfacePreferences.EnableSortPrefixes;
            HasNetworkAccess = corePreferences.HasNetworkAccess;
            LoadLastLibraryOnStartup = corePreferences.LoadLastLibraryOnStartup;
            MinimizeToTray = interfacePreferences.MinimizeToTray;
            SortPrefixes = interfacePreferences.SortPrefixes;

            CurrentTheme = AvailableThemes.FirstOrDefault( t => t.Id.Equals( mThemeManager.CurrentTheme )) ??
                           AvailableThemes.FirstOrDefault( t => t.Id.Equals( interfacePreferences.ThemeName ));

            CurrentAccent = AvailableAccents.FirstOrDefault( a => a.Id.Equals( mThemeManager.CurrentAccent )) ??
                            AvailableAccents.FirstOrDefault( a => a.Id.Equals( interfacePreferences.ThemeAccent ));

            CurrentSignature = AvailableSignatures.FirstOrDefault( s => s.Location.Equals( interfacePreferences.ThemeSignature ));
        }

        public void UpdatePreferences() {
            var interfacePreferences = mPreferences.Load<UserInterfacePreferences>();
            var corePreferences = mPreferences.Load<NoiseCorePreferences>();
            
            corePreferences.EnableRemoteAccess = EnableRemoteAccess;
            corePreferences.HasNetworkAccess = HasNetworkAccess;
            corePreferences.LoadLastLibraryOnStartup = LoadLastLibraryOnStartup;

            interfacePreferences.EnableGlobalHotkeys = EnableGlobalHotkeys;
            interfacePreferences.EnableSortPrefixes = EnableSortPrefixes;
            interfacePreferences.SortPrefixes = SortPrefixes;
            interfacePreferences.MinimizeToTray = MinimizeToTray;

            if( CurrentTheme != null ) {
                interfacePreferences.ThemeName = CurrentTheme.Id;
            }
            if( CurrentAccent != null ) {
                interfacePreferences.ThemeAccent = CurrentAccent.Id;
            }
            if( CurrentSignature != null ) {
                interfacePreferences.ThemeSignature = CurrentSignature.Location;
            }

            mPreferences.Save( corePreferences );
            mPreferences.Save( interfacePreferences );

            UpdateTheme();
        }

        private void UpdateTheme() {
            if(( CurrentAccent != null ) &&
               ( CurrentSignature != null ) &&
               ( CurrentTheme != null )) {
                mThemeManager.UpdateApplicationTheme( CurrentTheme.Id, CurrentAccent.Id, CurrentSignature?.Location );
            }
        }
    }
}

