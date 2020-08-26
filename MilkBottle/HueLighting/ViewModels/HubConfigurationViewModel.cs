using HueLighting.Dto;
using HueLighting.Interfaces;
using HueLighting.Views;
using MilkBottle.Infrastructure.Interfaces;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReusableBits.Mvvm.ViewModelSupport;

namespace HueLighting.ViewModels {
    class HubConfigurationViewModel : PropertyChangeBase {
        private readonly IHubManager    mHubManager;
        private readonly IPreferences   mPreferences;
        private readonly IDialogService mDialogService;

        public  string                  BridgeName { get; private set; }

        public  DelegateCommand         ConfigureBridge { get; }

        public HubConfigurationViewModel( IHubManager hubManager, IPreferences preferences, IDialogService dialogService ) {
            mHubManager = hubManager;
            mDialogService = dialogService;
            mPreferences = preferences;

            ConfigureBridge = new DelegateCommand( OnConfigureBridge );

            LoadHubInfo();
        }

        private void OnConfigureBridge() {
            mDialogService.ShowDialog( nameof( HubSelectionView ), new DialogParameters(), result => { });
        }

        private void LoadHubInfo() {
            var configuration = mPreferences.Load<InstallationInfo>();

            BridgeName = configuration.BridgeUserName;
            RaisePropertyChanged( () => BridgeName );
        }
    }
}
