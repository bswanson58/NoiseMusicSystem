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
        private bool                        mLightPipeState;

        public  string                      CaptureFrequencyTooltip => $"Capture Frequency: {CaptureFrequency} ms";

        public  DelegateCommand             Configuration { get; }
        public  DelegateCommand             Close { get; }

        public LightPipeControlViewModel( ILightPipePump pump, IDialogService dialogService, IEventAggregator eventAggregator ) {
            mLightPipePump = pump;
            mDialogService = dialogService;
            mEventAggregator = eventAggregator;

            Close = new DelegateCommand( OnClose );
            Configuration = new DelegateCommand( OnConfiguration );

            mLightPipeState = mLightPipePump.IsEnabled;
        }

        public int OverallBrightnessMinimum => 0;
        public int OverallBrightnessMaximum => 100;
        public int OverallBrightness {
            get => (int)( mLightPipePump.OverallBrightness * 100.0 );
            set => mLightPipePump.OverallBrightness = value / 100.0;
        }

        public int CaptureFrequencyMinimum => 0;
        public int CaptureFrequencyMaximum => 1000;
        public int CaptureFrequency {
            get => mLightPipePump.CaptureFrequency;
            set {
                mLightPipePump.CaptureFrequency = value;

                RaisePropertyChanged( () => CaptureFrequencyTooltip );
            }
        }

        public bool LightPipeState {
            get => mLightPipeState;
            set {
                mLightPipeState = value;

                OnLightPipeStateChanged();
            }
        }

        private async void OnLightPipeStateChanged() {
            mLightPipeState = await mLightPipePump.EnableLightPipe( mLightPipeState, true );

            RaisePropertyChanged( () => LightPipeState );
            RaisePropertyChanged( () => OverallBrightness );
        }

        private void OnConfiguration() {
            LightPipeState = false;

            mDialogService.ShowDialog( nameof( LightPipeDialog ), new DialogParameters(), result => { });
        }

        private void OnClose() {
            mEventAggregator.PublishOnUIThread( new Events.CloseLightPipeController());
        }
    }
}
