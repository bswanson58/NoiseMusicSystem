using HueLighting.Views;
using MilkBottle.Infrastructure.Dto;
using MilkBottle.Infrastructure.Interfaces;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReusableBits.Mvvm.ViewModelSupport;

namespace HueLighting.ViewModels {
    class HubConfigurationViewModel : PropertyChangeBase {
        private readonly IPreferences   mPreferences;
        private readonly IDialogService mDialogService;

        public  string                  BridgeName { get; private set; }

        public  DelegateCommand         ConfigureBridge { get; }

        public HubConfigurationViewModel( IPreferences preferences, IDialogService dialogService ) {
            mDialogService = dialogService;
            mPreferences = preferences;

            ConfigureBridge = new DelegateCommand( OnConfigureBridge );

            LoadHubInfo();
        }

        private void OnConfigureBridge() {
            mDialogService.ShowDialog( nameof( HubSelectionView ), new DialogParameters(), result => {
                if( result.Result == ButtonResult.OK ) {
                    LoadHubInfo();
                }
            });
        }

        private void LoadHubInfo() {
            var configuration = mPreferences.Load<HueConfiguration>();

            BridgeName = configuration.BridgeId + " (" + configuration.BridgeIp + ")";
            RaisePropertyChanged( () => BridgeName );
        }
    }
}
