using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Caliburn.Micro;
using MilkBottle.Dto;
using MilkBottle.Interfaces;

namespace MilkBottle.Models {
    class PresetController : IPresetController, IHandle<Events.MilkInitialized> {
        private readonly IEventAggregator           mEventAggregator;
        private readonly ProjectMWrapper            mProjectM;
        private readonly Subject<MilkDropPreset>    mCurrentPreset;
        private bool                                mUseHardCuts;

        public  IObservable<MilkDropPreset>         CurrentPreset => mCurrentPreset.AsObservable();

        public PresetController( ProjectMWrapper projectM, IEventAggregator eventAggregator ) {
            mProjectM = projectM;
            mEventAggregator = eventAggregator;

            mCurrentPreset = new Subject<MilkDropPreset>();

            mEventAggregator.Subscribe( this );

            if( mProjectM.isInitialized()) {
                Initialize();
            }
        }

        public void Handle( Events.MilkInitialized args ) {
            Initialize();
        }

        private void Initialize() {
            mProjectM.setPresetCallback( OnPresetSwitched );

            LoadInitialPresets();
        }

        public void selectNextPreset() {
            mProjectM.selectNext( mUseHardCuts );
        }

        public void selectPreviousPreset() {
            mProjectM.selectPrevious( mUseHardCuts );
        }

        public void selectRandomPreset() {
            mProjectM.selectRandom( mUseHardCuts );
        }

        public void setPresetOverlap( bool state ) {
            mUseHardCuts = !state;
        }

        public void setPresetCycling( bool state ) {
            mProjectM.setPresetLock( !state );
        }

        private void LoadInitialPresets() {
            mProjectM.clearPresetlist();
            mProjectM.setShuffleEnabled( false );
            mProjectM.setPresetLock( true );
            mProjectM.addPresetURL( @"D:\projectM\presets\presets_stock\Fvese - Lifesavor Anyone.milk", "Fvese - Lifesavor Anyone" );
            mProjectM.selectNext( true );
        }

        private void OnPresetSwitched(bool isHardCut, ulong presetIndex ) {
            mCurrentPreset.OnNext( new MilkDropPreset( mProjectM.getPresetName( presetIndex ), mProjectM.getPresetURL( presetIndex )));
        }

        public void Dispose() {
            mCurrentPreset?.Dispose();

            mEventAggregator.Unsubscribe( this );
        }
    }
}
