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
using ReusableBits.Ui.Platform;

namespace Noise.Desktop.ViewModels {
    class ShellViewModel : PropertyChangeBase, IHandle<Events.WindowLayoutRequest> {
        private readonly INoiseWindowManager            mWindowManager;
        private readonly IRegionManager                 mRegionManager;
        private readonly IIpcManager			        mIpcManager;
        private readonly IDialogService                 mDialogService;
        private readonly IPlatformDialogService         mPlatformDialogs;
        private readonly IDataExchangeManager	        mDataExchangeMgr;

        public  ObservableCollection<UiCompanionApp>    CompanionApplications => mIpcManager.CompanionApplications;
        public  bool									HaveCompanionApplications => CompanionApplications.Any();

        public  bool                                    IsLibraryView { get; private set; }
        public  bool                                    IsTimelineView { get; private set; }
        public  bool                                    IsListeningView { get; private set; }

        public  DelegateCommand                         LibraryLayout { get; }
        public  DelegateCommand                         ListeningLayout { get; }
        public  DelegateCommand                         TimelineLayout { get; }
        public  DelegateCommand                         Options { get; }
        public  DelegateCommand                         Guide { get; }

        public ShellViewModel( INoiseWindowManager windowManager, IRegionManager regionManager, IIpcManager ipcManager, IDataExchangeManager dataExchangeManager, 
                               IDialogService dialogService, IPlatformDialogService platformDialogService, IEventAggregator eventAggregator ) {
            mDialogService = dialogService;
            mWindowManager = windowManager;
            mRegionManager = regionManager;
            mIpcManager = ipcManager;
            mDataExchangeMgr = dataExchangeManager;
            mPlatformDialogs = platformDialogService;

            LibraryLayout = new DelegateCommand( OnLibraryLayout );
            ListeningLayout = new DelegateCommand( OnListeningLayout );
            TimelineLayout = new DelegateCommand( OnTimelineLayout );
            Options = new DelegateCommand( OnOptions );
            Guide = new DelegateCommand( OnGuide );

            CompanionApplications.CollectionChanged += OnCollectionChanged;

            var importCommand = new DelegateCommand( OnImport );
            GlobalCommands.ImportFavorites.RegisterCommand( importCommand );
            GlobalCommands.ImportRadioStreams.RegisterCommand( importCommand );
            GlobalCommands.ImportUserTags.RegisterCommand( importCommand );

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

                    IsLibraryView = true;
                    RaisePropertyChanged( () => IsLibraryView );
                    break;

                case Constants.SmallPlayerViewToggle:
                    mWindowManager.ToggleSmallPlayer();
                    break;
            }
        }

        private void OnLibraryLayout() {
            SetShellView( nameof( LibraryView ));

            IsLibraryView = true;
            RaisePropertyChanged( () => IsLibraryView );
        }

        private void OnListeningLayout() {
            SetShellView( nameof( ListeningView ));

            IsListeningView = true;
            RaisePropertyChanged( () => IsListeningView );
        }

        private void OnTimelineLayout() {
            SetShellView( nameof( TimelineView ));

            IsTimelineView = true;
            RaisePropertyChanged( () => IsTimelineView );
        }

        private void SetShellView( string regionName ) {
            var region = mRegionManager.Regions.FirstOrDefault( r => r.Name == RegionNames.ShellView );

            if( region != null ) {
                Execute.OnUIThread( () => region.RequestNavigate( new Uri( regionName, UriKind.Relative)));
            }

            IsLibraryView = false;
            IsListeningView = false;
            IsTimelineView = false;

            RaisePropertyChanged( () => IsLibraryView );
            RaisePropertyChanged( () => IsListeningView );
            RaisePropertyChanged( () => IsTimelineView );
        }

        private void OnOptions() {
            mDialogService.ShowDialog( nameof( ConfigurationDialog ), new DialogParameters(), result => { });
        }

        private void OnGuide() {
            mDialogService.ShowDialog( nameof( Noise.Guide.Views.GuideView ), new DialogParameters(), result => { });
        }

        private void OnImport() {
            if( mPlatformDialogs.OpenFileDialog( "Import", Constants.ExportFileExtension, "Export Files|*" + Constants.ExportFileExtension, out var fileName ) == true ) {

                var	importCount = mDataExchangeMgr.Import( fileName, true );
                var	importMessage = importCount > 0 ? $"{importCount} item(s) were imported." : "No items were imported.";

                mPlatformDialogs.MessageDialog( "Import Results", importMessage );
            }
        }
    }
}
