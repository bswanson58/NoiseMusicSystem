using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using HueLighting.Interfaces;
using Prism.Commands;
using Prism.Services.Dialogs;
using Q42.HueApi.Models.Bridge;

namespace HueLighting.ViewModels {
    class HubSelectionViewModel : IDialogAware {
        private readonly IHubManager                mHubManager;

        public  ObservableCollection<LocatedBridge> BridgeList { get; }

        public  string                              Title { get; }
        public  DelegateCommand                     Ok { get; }
        public  DelegateCommand                     Cancel { get; }

        public  event Action<IDialogResult>         RequestClose;

        public HubSelectionViewModel( IHubManager hubManager ) {
            mHubManager = hubManager;

            BridgeList = new ObservableCollection<LocatedBridge>();

            Title = "Hub Selection";

            Ok = new DelegateCommand( OnOk );
            Cancel = new DelegateCommand( OnCancel );
        }

        public async void OnDialogOpened( IDialogParameters parameters ) {
            await ScanForHubs();
        }

        private async Task ScanForHubs() {
            BridgeList.AddRange( await mHubManager.LocateHubs());
        }

        private void OnOk() { }

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
