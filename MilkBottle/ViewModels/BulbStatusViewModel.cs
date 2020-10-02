using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Data;
using Caliburn.Micro;
using MilkBottle.Dto;
using MilkBottle.Infrastructure.Events;
using MilkBottle.Interfaces;
using ReusableBits.Mvvm.ViewModelSupport;

namespace MilkBottle.ViewModels {
    class BulbStatusViewModel : PropertyChangeBase, IDisposable, IHandle<CurrentGroupChanged> {
        private const int           cProcessingLimit = 10;

        private readonly BindableCollection<ZoneBulbState>  mBulbStates;
        private IDisposable         mBulbStateSubscription;
        private bool                mClearStates;

        public  ICollectionView     BulbStates { get; }

        public  string              ElapsedTime { get; private set; }
        public  bool                HighProcessingTime { get; private set; }

        public BulbStatusViewModel( IZoneUpdater zoneUpdater ) {
            mBulbStateSubscription = zoneUpdater.BulbStates.ObserveOnDispatcher().Subscribe( OnBulbStates );

            mBulbStates = new BindableCollection<ZoneBulbState>();

            BulbStates = CollectionViewSource.GetDefaultView( mBulbStates );
            BulbStates.SortDescriptions.Add( new SortDescription( nameof( ZoneBulbState.ZoneName ), ListSortDirection.Ascending ));
        }

        public void Handle( CurrentGroupChanged args ) {
            mClearStates = true;
        }

        private void OnBulbStates( ZoneBulbState state ) {
            if( mClearStates ) {
                mBulbStates.Clear();
                mClearStates = false;
            }
            else {
                var existing = mBulbStates.FirstOrDefault( b => b.ZoneName.Equals( state.ZoneName ));

                mBulbStates.IsNotifying = false;
                if( existing != null ) {
                    mBulbStates.Remove( existing );
                }

                mBulbStates.Add( state );
                mBulbStates.IsNotifying = true;
                mBulbStates.Refresh();

                ElapsedTime = $"{state.ProcessingTime}";
                HighProcessingTime = state.ProcessingTime > cProcessingLimit;

                RaisePropertyChanged( () => ElapsedTime );
                RaisePropertyChanged( () => HighProcessingTime );
            }
        }

        public void Dispose() {
            mBulbStateSubscription?.Dispose();
            mBulbStateSubscription = null;
        }
    }
}
