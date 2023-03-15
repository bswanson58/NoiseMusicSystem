using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using HueLighting.Dto;
using HueLighting.Interfaces;
using HueLighting.Support;
using HueLighting.Views;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReusableBits.Mvvm.ViewModelSupport;

namespace HueLighting.ViewModels {
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class HubRegistrationViewModel : DialogAwareBase {
        private readonly IHubManager                mHubManager;
        private readonly IDialogService             mDialogService;
        private CancellationTokenSource             mTokenSource;

        public  ObservableCollection<HubViewModel>  Hubs { get; }
        public  bool                                ScanningForHubs { get; private set; }
        public  bool                                WaitingForButton { get; private set; }

        public  DelegateCommand<HubInformation>     RegisterHub { get; }
        public  DelegateCommand                     HubScan { get; }
        public  ICommand                            CancelRegistration { get; }

        public HubRegistrationViewModel( IHubManager hubManager, IDialogService dialogService ) {
            mHubManager = hubManager;
            mDialogService = dialogService;

            mTokenSource = new CancellationTokenSource();

            HubScan = new DelegateCommand( OnHubScan, CanScanHubs );
            RegisterHub = new DelegateCommand<HubInformation>( OnRegisterHub );
            CancelRegistration = new DelegateCommand( OnCancelRegistration );

            Hubs = new ObservableCollection<HubViewModel>();
            ScanningForHubs = false;
            WaitingForButton = false;
        }

        public override async void OnDialogOpened( IDialogParameters parameters ) {
            await LocateHubs();
        }

        private async void OnHubScan() {
            await LocateHubs();
        }

        private bool CanScanHubs() => !ScanningForHubs;

        private async Task LocateHubs() {
            Hubs.Clear();

            ScanningForHubs = true;
            RaisePropertyChanged( () => ScanningForHubs );
            HubScan.RaiseCanExecuteChanged();

            var hubs = await mHubManager.LocateHubs();
            Hubs.AddRange( hubs.Select( h => new HubViewModel( h )));

            ScanningForHubs = false;
            RaisePropertyChanged( () => ScanningForHubs );
            HubScan.RaiseCanExecuteChanged();
        }

        private async void OnRegisterHub( HubInformation hub ) {
            if( hub != null ) {
                WaitingForButton = true;
                RaisePropertyChanged( () => WaitingForButton );

                var result = await RegisterWithHub( hub );

                WaitingForButton = false;
                RaisePropertyChanged( () => WaitingForButton );

                if( result ) {
                    var parameters = new DialogParameters {
                        { MessageDialogViewModel.cTitle, "Registration Successful" },
                        { MessageDialogViewModel.cMessage, "Registration was successful!" }
                    };

                    mDialogService.ShowDialog( nameof( MessageDialog ), parameters, r => { } );

                    await LoadRegisteredHubs();
                }
            }
        }

        private void OnCancelRegistration() {
            mTokenSource?.Cancel();
        }

        private async Task<bool> RegisterWithHub( HubInformation hub ) {
            HubInformation registeredHub;

            async Task<bool> RegisterApp() {
                registeredHub = await mHubManager.RegisterApp( hub, true );

                return registeredHub == null;
            }

            mTokenSource = new CancellationTokenSource( TimeSpan.FromSeconds( 90 ));

            return !( await Repeat.IntervalWhile( TimeSpan.FromSeconds( 1 ), RegisterApp, mTokenSource.Token ));
        }

        private async Task LoadRegisteredHubs() {
            Hubs.Clear();

            var registeredHubs = await mHubManager.GetRegisteredHubs();

            ScanningForHubs = true;
            RaisePropertyChanged( () => ScanningForHubs );

            Hubs.AddRange( registeredHubs.Select( h => new HubViewModel( h )));

            ScanningForHubs = false;
            RaisePropertyChanged( () => ScanningForHubs );
        }
    }
}
