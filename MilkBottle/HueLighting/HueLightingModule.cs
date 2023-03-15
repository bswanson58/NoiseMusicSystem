using HueLighting.Interfaces;
using HueLighting.Models;
using HueLighting.ViewModels;
using HueLighting.Views;
using Prism.Ioc;
using Prism.Modularity;

namespace HueLighting {
    public class HueLightingModule : IModule {
        public void RegisterTypes( IContainerRegistry containerRegistry ) {
            containerRegistry.RegisterSingleton<IHubManager, HubManager>();

            containerRegistry.RegisterDialog<HubSelectionView, HubSelectionViewModel>();
            containerRegistry.RegisterDialog<HubRegistrationView, HubRegistrationViewModel>();
            containerRegistry.RegisterDialog<LightColorSelectorView, LightColorSelectorViewModel>();
            containerRegistry.RegisterDialog<MessageDialog, MessageDialogViewModel>();
        }

        public void OnInitialized( IContainerProvider containerProvider ) {
        }
    }
}
