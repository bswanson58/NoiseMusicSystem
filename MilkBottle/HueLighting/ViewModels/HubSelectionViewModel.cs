using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using HueLighting.Dto;
using HueLighting.Interfaces;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReusableBits.Mvvm.ViewModelSupport;

namespace HueLighting.ViewModels {
    class HubSelectionViewModel : PropertyChangeBase, IDialogAware {
        private readonly IHubManager                mHubManager;
        private HubInformation                           mSelectedHub;

        public  ObservableCollection<HubInformation>     HubList { get; }

        public  string                              Title { get; }
        public  bool                                CanRegister => CanRegisterHub();

        public  DelegateCommand                     ConfigureHub { get; }
        public  DelegateCommand                     RegisterHub { get; }

        public  DelegateCommand                     Ok { get; }
        public  DelegateCommand                     Cancel { get; }

        public  event Action<IDialogResult>         RequestClose;

        public HubSelectionViewModel( IHubManager hubManager ) {
            mHubManager = hubManager;

            HubList = new ObservableCollection<HubInformation>();

            ConfigureHub = new DelegateCommand( OnConfigureHub, CanConfigureHub );
            RegisterHub = new DelegateCommand( OnRegisterHub, CanRegisterHub );

            Title = "Hub Selection";

            Ok = new DelegateCommand( OnOk );
            Cancel = new DelegateCommand( OnCancel );
        }

        public async void OnDialogOpened( IDialogParameters parameters ) {
            await ScanForHubs();
        }

        private async Task ScanForHubs() {
            HubList.Clear();
            HubList.AddRange( await mHubManager.LocateHubs());
        }

        public HubInformation SelectedHub {
            get => mSelectedHub;
            set {
                mSelectedHub = value;

                RaisePropertyChanged( () => SelectedHub );
                RegisterHub.RaiseCanExecuteChanged();
            }
        }

        private void OnConfigureHub() {
            if( SelectedHub != null ) {
                mHubManager.SetConfiguredHub( SelectedHub );
            }
        }

        private bool CanConfigureHub() {
            return SelectedHub?.IsAppRegistered == true && SelectedHub?.IsConfiguredHub == false;
        }

        private async void OnRegisterHub() {
            if( SelectedHub != null ) {
                await mHubManager.RegisterApp( SelectedHub );

                await ScanForHubs();
            }
        }

        private bool CanRegisterHub() {
            return SelectedHub?.IsAppRegistered == false;
        }

        private void OnOk() {
            RaiseRequestClose( new DialogResult( ButtonResult.OK ));
        }

        private void OnCancel() {
            RaiseRequestClose( new DialogResult( ButtonResult.Cancel ));
        }

        public bool CanCloseDialog() {
            return true;
        }

        public void OnDialogClosed() { }

        private void RaiseRequestClose( IDialogResult dialogResult ) {
            RequestClose?.Invoke( dialogResult );
        }
    }
}
