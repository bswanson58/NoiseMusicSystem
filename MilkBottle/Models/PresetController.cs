using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Caliburn.Micro;
using MilkBottle.Dto;
using MilkBottle.Interfaces;

namespace MilkBottle.Models {
    class PresetController : IPresetController, IHandle<Events.MilkInitialized> {
        private readonly IEventAggregator           mEventAggregator;
        private readonly IPlatformLog               mLog;
        private readonly IPresetLibrarian           mLibrarian;
        private readonly ProjectMWrapper            mProjectM;
        private readonly Subject<MilkDropPreset>    mCurrentPreset;

        public  bool                                PresetOverlap { get; set; }

        public  IObservable<MilkDropPreset>         CurrentPreset => mCurrentPreset.AsObservable();

        public PresetController( ProjectMWrapper projectM, IPresetLibrarian librarian, IEventAggregator eventAggregator, IPlatformLog log ) {
            mProjectM = projectM;
            mLibrarian = librarian;
            mEventAggregator = eventAggregator;
            mLog = log;

            mCurrentPreset = new Subject<MilkDropPreset>();

            mEventAggregator.Subscribe( this );

            if( mProjectM.isInitialized()) {
                Initialize();
            }
        }

        public void Handle( Events.MilkInitialized args ) {
            Initialize();
        }

        public void LoadLibrary( string libraryName ) {
            var library = mLibrarian.GetLibrary( libraryName );

            if(( library != null ) &&
               ( library.Presets.Any())) {
                LoadPresets( library.Presets );
            }
        }

        private void Initialize() {
            mProjectM.setPresetCallback( OnPresetSwitched );
            mProjectM.setShuffleEnabled( false );

            LoadInitialPresets();
        }

        public void SelectNextPreset() {
            mProjectM.selectNext( PresetOverlap );
        }

        public void SelectPreviousPreset() {
            mProjectM.selectPrevious( PresetOverlap );
        }

        public void SelectRandomPreset() {
            mProjectM.selectRandom( PresetOverlap );
        }

        public bool PresetCycling {
            get => mProjectM.isPresetLocked();
            set => mProjectM.setPresetLock( value );
        }

        private void LoadInitialPresets() {
//            mProjectM.clearPresetlist();
//            mProjectM.setPresetLock( true );
//            mProjectM.addPresetURL( @"D:\projectM\presets\presets_stock\Fvese - Lifesavor Anyone.milk", "Fvese - Lifesavor Anyone" );
//            mProjectM.selectNext( true );
        }

        private void LoadPresets( IEnumerable<MilkDropPreset> presetList ) {
            try {
                mProjectM.clearPresetlist();

                foreach( var preset in presetList ) {
                    mProjectM.addPresetURL( preset.PresetLocation, preset.PresetName );
                }

                mProjectM.selectNext( PresetOverlap );
            }
            catch( Exception ex ) {
                mLog.LogException( "PresetController:LoadPresets", ex );
            }
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
