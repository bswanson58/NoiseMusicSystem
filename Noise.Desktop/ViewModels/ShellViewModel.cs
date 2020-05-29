using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Views;
using Prism.Commands;
using Prism.Regions;
using Prism.Services.Dialogs;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.Desktop.ViewModels {
    class ShellViewModel : PropertyChangeBase, IHandle<Events.WindowLayoutRequest> {
        private readonly INoiseWindowManager            mWindowManager;
        private readonly IRegionManager                 mRegionManager;
        private readonly IIpcManager			        mIpcManager;
        private readonly IDialogService                 mDialogService;

        public  ObservableCollection<UiCompanionApp>    CompanionApplications => mIpcManager.CompanionApplications;
        public  bool									HaveCompanionApplications => CompanionApplications.Any();

        public  DelegateCommand                         LibraryLayout { get; }
        public  DelegateCommand                         ListeningLayout { get; }
        public  DelegateCommand                         TimelineLayout { get; }
        public  DelegateCommand                         Options { get; }

        public ShellViewModel( INoiseWindowManager windowManager, IRegionManager regionManager, IIpcManager ipcManager, IDialogService dialogService, IEventAggregator eventAggregator ) {
            mDialogService = dialogService;
            mWindowManager = windowManager;
            mRegionManager = regionManager;
            mIpcManager = ipcManager;

            LibraryLayout = new DelegateCommand( OnLibraryLayout );
            ListeningLayout = new DelegateCommand( OnListeningLayout );
            TimelineLayout = new DelegateCommand( OnTimelineLayout );
            Options = new DelegateCommand( OnOptions );

            CompanionApplications.CollectionChanged += OnCollectionChanged;

            eventAggregator.Subscribe( this );
        }

        private void OnCollectionChanged( object sender, NotifyCollectionChangedEventArgs args ) {
            RaisePropertyChanged( () => HaveCompanionApplications );
        }

        public void Handle( Events.WindowLayoutRequest eventArgs ) {
            switch( eventArgs.LayoutName ) {
                case Constants.StartupLayout:
                    SetShellView( nameof( StartupView ));
                    break;

                case Constants.LibraryCreationLayout:
                    SetShellView( nameof( StartupLibraryCreationView ));
                    break;

                case Constants.LibrarySelectionLayout:
                    SetShellView( nameof( StartupLibrarySelectionView ));
                    break;

                case Constants.ExploreLayout:
                    SetShellView( nameof( LibraryView ));
                    break;

                case Constants.SmallPlayerViewToggle:
                    mWindowManager.ToggleSmallPlayer();
                    break;
            }
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
            mDialogService.ShowDialog( nameof( ConfigurationDialog ), new DialogParameters(), result => { });
        }
    }
}
