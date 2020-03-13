using System;
using MilkBottle.Entities;
using MilkBottle.Interfaces;
using ReusableBits.Mvvm.ViewModelSupport;

namespace MilkBottle.ViewModels {
    class ReviewStatusViewModel : PropertyChangeBase, IDisposable {
        private IDisposable                 mPresetSubscription;

        public  string                      PresetName { get; private set; }

        public ReviewStatusViewModel( IPresetController presetController ) {
            mPresetSubscription = presetController.CurrentPreset.Subscribe( OnPresetChanged );
        }

        private void OnPresetChanged( Preset preset ) {
            PresetName = preset != null ? preset.Name : String.Empty;

            RaisePropertyChanged( () => PresetName );
        }

        public void Dispose() {
            mPresetSubscription?.Dispose();
            mPresetSubscription = null;
        }
    }
}
