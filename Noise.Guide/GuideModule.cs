using Noise.Guide.ViewModels;
using Noise.Guide.Views;
using Prism.Ioc;
using Prism.Modularity;

namespace Noise.Guide {
    public class GuideModule : IModule {
        public void RegisterTypes( IContainerRegistry containerRegistry ) {
            containerRegistry.RegisterDialog<GuideView, GuideViewModel>();
        }

        public void OnInitialized( IContainerProvider containerProvider ) {
        }
    }
}
