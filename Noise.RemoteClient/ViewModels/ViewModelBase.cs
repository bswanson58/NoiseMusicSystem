using Prism.Mvvm;
using Prism.Navigation;

namespace Noise.RemoteClient.ViewModels {
    public class ViewModelBase : BindableBase, IInitialize, INavigationAware, IDestructible {
        private string mTitle;

        protected INavigationService NavigationService { get; }

        public string Title {
            get => mTitle;
            set => SetProperty( ref mTitle, value );
        }

        public ViewModelBase( INavigationService navigationService ) {
            NavigationService = navigationService;
        }

        public virtual void Initialize( INavigationParameters parameters ) { }

        public virtual void OnNavigatedFrom( INavigationParameters parameters ) { }
        public virtual void OnNavigatedTo( INavigationParameters parameters ) { }

        public virtual void Destroy() { }
    }
}
