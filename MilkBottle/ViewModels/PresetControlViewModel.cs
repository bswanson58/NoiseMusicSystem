using System;
using MilkBottle.Dto;
using MilkBottle.Interfaces;
using Prism.Commands;
using ReusableBits.Mvvm.ViewModelSupport;

namespace MilkBottle.ViewModels {
    class PresetControlViewModel : PropertyChangeBase, IDisposable {
        private readonly IPresetController  mController;
        private IDisposable                 mPresetSubscription;

        public  DelegateCommand     NextPreset { get; }
        public  DelegateCommand     PreviousPreset { get; }

        public  string              PresetName { get; set; }

        public PresetControlViewModel( IPresetController presetController ) {
            mController = presetController;

            NextPreset = new DelegateCommand( OnNextPreset );
            PreviousPreset = new DelegateCommand( OnPreviousPreset );

            mPresetSubscription = mController.CurrentPreset.Subscribe( OnPresetChanged );
        }

        public void OnPresetChanged( MilkDropPreset preset ) {
            PresetName = preset.PresetName;

            RaisePropertyChanged( () => PresetName );
        }

        public bool IsLocked {
            get => mController.PresetCycling;
            set => mController.PresetCycling = value;
        }

        public bool IsBlended {
            get => mController.PresetOverlap;
            set => mController.PresetOverlap = value;
        }

        private void OnNextPreset() {
            mController.SelectNextPreset();
        }

        private void OnPreviousPreset() {
            mController.SelectPreviousPreset();
        }

        public void Dispose() {
            mPresetSubscription?.Dispose();
            mPresetSubscription = null;
        }
    }
}
