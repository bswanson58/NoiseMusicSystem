using Caliburn.Micro;
using ReusableBits.Mvvm.ViewModelSupport;

namespace MilkBottle.ViewModels {
    class SyncStatusViewModel : PropertyChangeBase {
        private readonly IEventAggregator   mEventAggregator;

        public SyncStatusViewModel( IEventAggregator eventAggregator ) {
            mEventAggregator = eventAggregator;
        }
    }
}
