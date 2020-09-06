using System;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using MilkBottle.Dto;
using MilkBottle.Infrastructure.Dto;
using MilkBottle.Infrastructure.Events;
using MilkBottle.Infrastructure.Interfaces;
using MilkBottle.Interfaces;
using MilkBottle.Views;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReusableBits.Mvvm.ViewModelSupport;

namespace MilkBottle.ViewModels {
    class LightPipeControlViewModel : PropertyChangeBase, IDisposable,
                                      IHandle<CurrentGroupChanged>, IHandle<CurrentZoneChanged> {
        private readonly ILightPipePump                 mLightPipePump;
        private readonly IZoneManager                   mZoneManager;
        private readonly IPairingManager                mPairingManager;
        private readonly IDialogService                 mDialogService;
        private readonly IPreferences                   mPreferences;
        private readonly IEventAggregator               mEventAggregator;
        private bool                                    mLightPipeState;
        private LightPipePairing                        mCurrentPairing;
        private bool                                    mLoadingPairings;

        public  ObservableCollection<LightPipePairing>  Pairs { get; }

        public  DelegateCommand                         Configuration { get; }
        public  DelegateCommand                         Close { get; }

        public LightPipeControlViewModel( ILightPipePump pump, IPairingManager pairingManager, IZoneManager zoneManager,
                                          IDialogService dialogService, IPreferences preferences, IEventAggregator eventAggregator ) {
            mLightPipePump = pump;
            mPairingManager = pairingManager;
            mZoneManager = zoneManager;
            mDialogService = dialogService;
            mPreferences = preferences;
            mEventAggregator = eventAggregator;

            Pairs = new ObservableCollection<LightPipePairing>();
            LoadPairings();

            Close = new DelegateCommand( OnClose );
            Configuration = new DelegateCommand( OnConfiguration );

            mLightPipeState = mLightPipePump.IsEnabled;

            mEventAggregator.Subscribe( this );
        }

        public LightPipePairing CurrentPairing {
            get => mCurrentPairing;
            set {
                if(!mLoadingPairings ) {
                    mCurrentPairing = value;

                    OnPairingChanged();
                    RaisePropertyChanged( () => CurrentPairing );
                }
            }
        }

        private async void OnPairingChanged() {
            var currentState = LightPipeState;

            mPairingManager.SetCurrentPairing( CurrentPairing );
            if( CurrentPairing != null ) {
                var huePreferences = mPreferences.Load<HueConfiguration>();

                huePreferences.EntertainmentGroupId = CurrentPairing.EntertainmentGroupId;
                mPreferences.Save( huePreferences );

                mZoneManager.SetCurrentGroup( CurrentPairing.ZoneGroupId );
            }

            if( currentState ) {
                mLightPipeState = await mLightPipePump.EnableLightPipe( false, true );
                RaisePropertyChanged( () => LightPipeState );

                mLightPipeState = await mLightPipePump.EnableLightPipe( true, true );
                RaisePropertyChanged( () => LightPipeState );
                RaisePropertyChanged( () => OverallBrightness );
            }
        }

        public void Handle( CurrentGroupChanged message ) {
            UpdatePairing();
        }

        public void Handle( CurrentZoneChanged message ) {
            UpdatePairing();
        }

        private void UpdatePairing() {
            var currentZone = mZoneManager.GetCurrentGroup();
            var huePreferences = mPreferences.Load<HueConfiguration>();

            if(( currentZone != null ) &&
               (!String.IsNullOrEmpty( huePreferences.EntertainmentGroupId ))) {
                mPairingManager.SetCurrentPairing( mPairingManager.GetPairings().FirstOrDefault( p => p.EntertainmentGroupId.Equals( huePreferences.EntertainmentGroupId ) &&
                                                                                                      p.ZoneGroupId.Equals( currentZone.GroupId )));
            }

            LoadPairings();
        }

        private void LoadPairings() {
            mLoadingPairings = true;

            Pairs.Clear();
            Pairs.AddRange( mPairingManager.GetPairings());

            var currentPair = mPairingManager.GetCurrentPairing();

            mCurrentPairing = null;
            RaisePropertyChanged( () => CurrentPairing );

            if( currentPair != null ) {
                mCurrentPairing = Pairs.FirstOrDefault( p => p.PairingId.Equals( currentPair.PairingId ));
            }

            mLoadingPairings = false;
            RaisePropertyChanged( () => CurrentPairing );
        }

        public int OverallBrightnessMinimum => 0;
        public int OverallBrightnessMaximum => 100;
        public int OverallBrightness {
            get => (int)( mLightPipePump.OverallBrightness * 100.0 );
            set {
                mLightPipePump.OverallBrightness = value / 100.0;

                RaisePropertyChanged( () => OverallBrightness );
            }
        }

        public int CaptureFrequencyMinimum => 30;
        public int CaptureFrequencyMaximum => 1000;
        public int CaptureFrequency {
            get => mLightPipePump.CaptureFrequency;
            set {
                mLightPipePump.CaptureFrequency = value;

                RaisePropertyChanged( () => CaptureFrequency );
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

            UpdatePairing();
        }

        private void OnClose() {
            mEventAggregator.PublishOnUIThread( new Events.CloseLightPipeController());
        }

        public void Dispose() {
            mEventAggregator.Unsubscribe( this );
        }
    }
}
