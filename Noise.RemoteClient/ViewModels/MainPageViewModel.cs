﻿using Prism.Navigation;

namespace Noise.RemoteClient.ViewModels {
    public class MainPageViewModel : ViewModelBase {

        public MainPageViewModel( INavigationService navigationService )
            : base( navigationService ) {
            Title = "Main Page";
        }
    }
}
