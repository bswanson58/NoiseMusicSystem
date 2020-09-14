using System;
using System.Linq;
using Caliburn.Micro;
using MilkBottle.Dto;
using MilkBottle.Infrastructure.Dto;
using MilkBottle.Infrastructure.Interfaces;
using MilkBottle.Interfaces;
using Prism.Commands;
using ReusableBits.Mvvm.ViewModelSupport;

namespace MilkBottle.ViewModels {
    class LightPipePairingViewModel : PropertyChangeBase, IDisposable,
                                      IHandle<Infrastructure.Events.CurrentGroupChanged>, IHandle<Infrastructure.Events.CurrentZoneChanged> {
        private readonly IEventAggregator   mEventAggregator;
        private readonly IPairingManager    mPairingManager;
        private readonly IZoneManager       mZoneManager;
        private readonly IPreferences       mPreferences;
        private LightPipePairing            mCurrentPairing;
        private string                      mPairName;

        public  bool                        HaveCurrentPair { get; private set; }

        public  DelegateCommand             AddPair { get; }
        public  DelegateCommand             DeletePair { get; }

        public LightPipePairingViewModel( IPairingManager pairingManager, IZoneManager zoneManager, IEventAggregator eventAggregator, IPreferences preferences ) {
            mPairingManager = pairingManager;
            mZoneManager = zoneManager;
            mEventAggregator = eventAggregator;
            mPreferences = preferences;

            AddPair = new DelegateCommand( OnAddPair, CanAddPair );
            DeletePair = new DelegateCommand( OnDeletePair, CanDeletePair );

            DeterminePairing();

            mEventAggregator.Subscribe( this );
        }

        public string PairName {
            get => mPairName;
            set {
                mPairName = value;

                OnPairNameChanged();
                AddPair.RaiseCanExecuteChanged();
            }
        }

        private void OnPairNameChanged() {
            RaisePropertyChanged( () => PairName );
        }

        public void Handle( Infrastructure.Events.CurrentGroupChanged args ) {
            DeterminePairing();
        }

        public void Handle( Infrastructure.Events.CurrentZoneChanged args ) {
            DeterminePairing();
        }

        private void DeterminePairing() {
            var currentZone = mZoneManager.GetCurrentGroup();
            var huePreferences = mPreferences.Load<HueConfiguration>();

            if(( currentZone != null ) &&
               (!String.IsNullOrEmpty( huePreferences.EntertainmentGroupId ))) {
                mCurrentPairing = mPairingManager.GetPairings().FirstOrDefault( p => p.EntertainmentGroupId.Equals( huePreferences.EntertainmentGroupId ) &&
                                                                                     p.ZoneGroupId.Equals( currentZone.GroupId ));
                HaveCurrentPair = mCurrentPairing != null;
                PairName = mCurrentPairing?.PairingName;
            }

            RaisePropertyChanged( () => HaveCurrentPair );
            RaisePropertyChanged( () => PairName );

            AddPair.RaiseCanExecuteChanged();
            DeletePair.RaiseCanExecuteChanged();
        }

        private void OnAddPair() {
            var currentZone = mZoneManager.GetCurrentGroup();
            var huePreferences = mPreferences.Load<HueConfiguration>();

            if((!HaveCurrentPair ) &&
               (!String.IsNullOrEmpty( PairName )) &&
               ( currentZone != null ) &&
               (!String.IsNullOrWhiteSpace( huePreferences.EntertainmentGroupId ))) {
                mPairingManager.AddPairing( PairName, huePreferences.EntertainmentGroupId, currentZone.GroupId );
            }

            DeterminePairing();
        }

        private bool CanAddPair() {
            return !HaveCurrentPair && !String.IsNullOrWhiteSpace( PairName );
        }

        private void OnDeletePair() {
            if(( HaveCurrentPair ) &&
               ( mCurrentPairing != null )) {
                mPairingManager.DeletePairing( mCurrentPairing );

                DeterminePairing();
            }
        }

        private bool CanDeletePair() {
            return HaveCurrentPair;
        }

        public void Dispose() {
            mEventAggregator.Unsubscribe( this );
        }
    }
}
