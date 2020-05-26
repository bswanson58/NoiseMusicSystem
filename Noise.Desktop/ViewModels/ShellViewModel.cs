using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Caliburn.Micro;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Regions;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Support;
using Noise.UI.ViewModels;
using Noise.UI.Views;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.Desktop.ViewModels {
    enum ShellViews {
        Startup,
        LibrarySelection,
        LibraryCreation,
        Library,
        Listening,
        Timeline
    }

    class ShellViewModel : PropertyChangeBase, IHandle<Events.NoiseSystemReady>, IHandle<Events.LibraryConfigurationLoaded>, IHandle<Events.WindowLayoutRequest> {
        private readonly INoiseWindowManager            mWindowManager;
        private readonly IRegionManager                 mRegionManager;
        private readonly IIpcManager			        mIpcManager;
        private readonly IDialogService                 mDialogService;
        private readonly IPreferences		            mPreferences;
        private ILibraryConfiguration		            mLibraryConfiguration;

        public  ObservableCollection<UiCompanionApp>    CompanionApplications => mIpcManager.CompanionApplications;
        public  bool									HaveCompanionApplications => CompanionApplications.Any();

        public  DelegateCommand                         LibraryLayout { get; }
        public  DelegateCommand                         ListeningLayout { get; }
        public  DelegateCommand                         TimelineLayout { get; }
        public  DelegateCommand                         Options { get; }

        public ShellViewModel( INoiseWindowManager windowManager, IRegionManager regionManager, IIpcManager ipcManager, IDialogService dialogService,
                               IPreferences preferences, IEventAggregator eventAggregator ) {
            mDialogService = dialogService;
            mPreferences = preferences;
            mWindowManager = windowManager;
            mRegionManager = regionManager;
            mIpcManager = ipcManager;

            LibraryLayout = new DelegateCommand( OnLibraryLayout );
            ListeningLayout = new DelegateCommand( OnListeningLayout );
            TimelineLayout = new DelegateCommand( OnTimelineLayout );
            Options = new DelegateCommand( OnOptions );

            CompanionApplications.CollectionChanged += OnCollectionChanged;

//            SetShellView( ShellViews.Startup );
            SetShellView( nameof( StartupView ));

            eventAggregator.Subscribe( this );
        }

        private void OnCollectionChanged( object sender, NotifyCollectionChangedEventArgs args ) {
            RaisePropertyChanged( () => HaveCompanionApplications );
        }

        public void Handle( Events.NoiseSystemReady args ) {
            if(( args.WasInitialized ) &&
               ( mLibraryConfiguration != null )) {
                var preferences = mPreferences.Load<NoiseCorePreferences>();

                var lastLibraryUsed = preferences.LastLibraryUsed;
                var loadLastLibraryOnStartup = preferences.LoadLastLibraryOnStartup;

                if(( loadLastLibraryOnStartup ) &&
                   ( lastLibraryUsed != Constants.cDatabaseNullOid )) {
                    mLibraryConfiguration.Open( lastLibraryUsed );

                    SetShellView( nameof( LibraryView ));
                }
                else {
                    SetShellView( mLibraryConfiguration.Libraries.Any() ? nameof( StartupLibrarySelectionView ) : nameof( StartupLibraryCreationView ));
                }
            }
        }

        public void Handle( Events.WindowLayoutRequest eventArgs ) {
            switch( eventArgs.LayoutName ) {
                case Constants.LibraryCreationLayout:
                    SetShellView( nameof( StartupLibraryCreationView ));
                    break;

                case Constants.ExploreLayout:
                    SetShellView( nameof( LibraryView ));
                    break;

                case Constants.SmallPlayerViewToggle:
                    mWindowManager.ToggleSmallPlayer();
                    break;
            }
        }

        public void Handle( Events.LibraryConfigurationLoaded args ) {
            mLibraryConfiguration = args.LibraryConfiguration;
        }

        private void OnLibraryLayout() {
            SetShellView( nameof( LibraryView ));
        }

        private void OnListeningLayout() {
            SetShellView( nameof( ListeningView ));
        }

        private void OnTimelineLayout() {
            SetShellView( nameof( TimelineView ));
        }

        private void SetShellView( string regionName ) {
            var region = mRegionManager.Regions.FirstOrDefault( r => r.Name == RegionNames.ShellView );

            if( region != null ) {
                Execute.OnUIThread( () => region.RequestNavigate( new Uri( regionName, UriKind.Relative)));
            }
        }

        private void OnOptions() {
            var dialogModel = new ConfigurationViewModel( mPreferences );

            if( mDialogService.ShowDialog( DialogNames.NoiseOptions, dialogModel ) == true ) {
                dialogModel.UpdatePreferences();
            }
        }
    }
}
