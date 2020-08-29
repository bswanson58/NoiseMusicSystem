using Caliburn.Micro;
using MilkBottle.Interfaces;
using MilkBottle.Views;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReusableBits.Mvvm.ViewModelSupport;

namespace MilkBottle.ViewModels {
    class LightPipeControlViewModel : PropertyChangeBase {
        private readonly ILightPipePump     mLightPipePump;
        private readonly IDialogService     mDialogService;
        private readonly IEventAggregator   mEventAggregator;

        public  DelegateCommand             Configuration { get; }
        public  DelegateCommand             Close { get; }

        public LightPipeControlViewModel( ILightPipePump pump, IDialogService dialogService, IEventAggregator eventAggregator ) {
            mLightPipePump = pump;
            mDialogService = dialogService;
            mEventAggregator = eventAggregator;

            Close = new DelegateCommand( OnClose );
            Configuration = new DelegateCommand( OnConfiguration );
        }

        private void OnConfiguration() {
            mDialogService.ShowDialog( nameof( LightPipeDialog ), new DialogParameters(), result => { });
        }

        private void OnClose() {
            mEventAggregator.PublishOnUIThread( new Events.CloseLightPipeController());
        }
    }
}
