using System;
using Caliburn.Micro;
using MilkBottle.Dto;
using MilkBottle.Interfaces;
using OpenTK;

namespace MilkBottle.ViewModels {
    class MilkViewModel : IDisposable {
        private readonly IEventAggregator   mEventAggregator;
        private readonly IMilkController    mMilkController;
        private readonly IPresetController  mPresetController;
        private IDisposable                 mPresetSubscription;


        public MilkViewModel( IMilkController milkController, IPresetController presetController, IEventAggregator eventAggregator ) {
            mEventAggregator = eventAggregator;
            mMilkController = milkController;
            mPresetController = presetController;
        }

        public void Initialize( GLControl glControl ) {
            mMilkController.Initialize( glControl );

            mPresetSubscription = mPresetController.CurrentPreset.Subscribe( OnPresetSwitched );
        }

        public void StartVisualization() {
            mMilkController.StartVisualization();
        }

        public void OnSizeChanged( int width, int height ) {
            mMilkController.OnSizeChanged( width, height );
        }

        private void OnPresetSwitched( MilkDropPreset preset ) {
            mEventAggregator.PublishOnUIThread( new Events.StatusEvent( preset.PresetName ));
        }

        public void Dispose() {
            mEventAggregator.Unsubscribe( this );

            mPresetSubscription?.Dispose();
        }
    }
}
