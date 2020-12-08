using Noise.RemoteClient.Interfaces;
using Prism.Navigation;

namespace Noise.RemoteClient.ViewModels {
    public class MainPageViewModel : ViewModelBase {
        private readonly IServiceLocator    mServiceLocator;

        public MainPageViewModel( IServiceLocator serviceLocator, INavigationService navigationService )
            : base( navigationService ) {
            mServiceLocator = serviceLocator;
            Title = "Main Page";

            mServiceLocator.StartServiceLocator();
        }
    }
}
