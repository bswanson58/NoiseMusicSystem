using System;
using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Models;
using Prism.Commands;
using Prism.Services.Dialogs;

namespace Noise.UI.ViewModels {
    public class ConfigurationViewModel : PropertyChangedBase, IDialogAware {
        private readonly IPreferences   mPreferences;
        private readonly ThemeManager   mThemeManager;
        private ThemeColors             mCurrentTheme;
        private SignatureColors         mCurrentSignature;

        public  bool                    EnableGlobalHotkeys { get; set; }
        public  bool                    EnableRemoteAccess { get; set; }
        public  bool                    EnableSortPrefixes { get; set; }
        public  bool                    HasNetworkAccess { get; set; }
        public  bool                    LoadLastLibraryOnStartup { get; set; }
        public  bool                    MinimizeToTray { get; set; }
        public  bool                    MinimizeOnSwitchToCompanionApp { get; set; }
        public  string                  SortPrefixes { get; set; }

        public  BindableCollection<ThemeColors>     AvailableThemes { get; }
        public  BindableCollection<SignatureColors> AvailableSignatures { get; }

        public  string                              Title { get; }
        public  DelegateCommand                     Ok { get; }
        public  DelegateCommand                     Cancel { get; }
        public  event Action<IDialogResult>         RequestClose;

        public ThemeColors CurrentTheme {
            get => mCurrentTheme;
            set {
                mCurrentTheme = value;

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

            AvailableThemes = new BindableCollection<ThemeColors>( from theme in themeCatalog.Themes orderby theme.Name select theme );
            AvailableSignatures = new BindableCollection<SignatureColors>( themeCatalog.Signatures );

            var interfacePreferences = mPreferences.Load<UserInterfacePreferences>();
            var corePreferences = mPreferences.Load<NoiseCorePreferences>();

            EnableGlobalHotkeys = interfacePreferences.EnableGlobalHotkeys;
            EnableRemoteAccess = corePreferences.EnableRemoteAccess;
            EnableSortPrefixes = interfacePreferences.EnableSortPrefixes;
            HasNetworkAccess = corePreferences.HasNetworkAccess;
            LoadLastLibraryOnStartup = corePreferences.LoadLastLibraryOnStartup;
            MinimizeToTray = interfacePreferences.MinimizeToTray;
            MinimizeOnSwitchToCompanionApp = interfacePreferences.MinimizeOnSwitchToCompanionApp;
            SortPrefixes = interfacePreferences.SortPrefixes;

            CurrentTheme = AvailableThemes.FirstOrDefault( t => t.Id.Equals( mThemeManager.CurrentTheme )) ??
                           AvailableThemes.FirstOrDefault( t => t.Id.Equals( interfacePreferences.ThemeName ));

            CurrentSignature = AvailableSignatures.FirstOrDefault( s => s.Location.Equals( interfacePreferences.ThemeSignature ));

            Title ="Noise Options";

            Ok = new DelegateCommand( OnOk );
            Cancel = new DelegateCommand( OnCancel );
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
            interfacePreferences.MinimizeOnSwitchToCompanionApp = MinimizeOnSwitchToCompanionApp;

            if( CurrentTheme != null ) {
                interfacePreferences.ThemeName = CurrentTheme.Id;
            }
            if( CurrentSignature != null ) {
                interfacePreferences.ThemeSignature = CurrentSignature.Location;
            }

            mPreferences.Save( corePreferences );
            mPreferences.Save( interfacePreferences );

            UpdateTheme();
        }

        private void UpdateTheme() {
            if(( CurrentSignature != null ) &&
               ( CurrentTheme != null )) {
                mThemeManager.UpdateApplicationTheme( CurrentTheme.Id, CurrentSignature?.Location );
            }
        }

        public bool CanCloseDialog() {
            return true;
        }

        public void OnDialogOpened( IDialogParameters parameters ) { }
        public void OnDialogClosed() { }

        public void OnOk() {
            UpdatePreferences();

            RaiseRequestClose( new DialogResult( ButtonResult.OK ));
        }

        public void OnCancel() {
            RaiseRequestClose( new DialogResult( ButtonResult.Cancel ));
        }

        private void RaiseRequestClose( IDialogResult dialogResult ) {
            RequestClose?.Invoke( dialogResult );
        }
    }
}

