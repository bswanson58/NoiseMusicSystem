using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Threading;
using Caliburn.Micro;
using MilkBottle.Dto;
using MilkBottle.Interfaces;

namespace MilkBottle.Models {
    class PresetController : IPresetController, IHandle<Events.MilkInitialized> {
        private readonly IEventAggregator           mEventAggregator;
        private readonly IPlatformLog               mLog;
        private readonly IPresetLibrarian           mLibrarian;
        private readonly ProjectMWrapper            mProjectM;
        private readonly IPreferences               mPreferences;
        private readonly Subject<MilkDropPreset>    mCurrentPreset;
        private readonly DispatcherTimer            mPresetTimer;
        private readonly Random                     mRandom;
        private int                                 mPresetDuration;
        private bool                                mPlayRandom;

        public  bool                                BlendPresetTransition { get; set; }
        public  string                              CurrentPresetLibrary { get; private set; }

        public  IObservable<MilkDropPreset>         CurrentPreset => mCurrentPreset.AsObservable();

        public PresetController( ProjectMWrapper projectM, IPresetLibrarian librarian, IPreferences preferences, IEventAggregator eventAggregator, IPlatformLog log ) {
            mProjectM = projectM;
            mLibrarian = librarian;
            mPreferences = preferences;
            mEventAggregator = eventAggregator;
            mLog = log;

            mRandom = new Random( DateTime.Now.Millisecond );
            mCurrentPreset = new Subject<MilkDropPreset>();
            mPresetTimer = new DispatcherTimer();
            mPresetTimer.Tick += OnPresetTimer;

            mEventAggregator.Subscribe( this );

            if( mProjectM.isInitialized()) {
                Initialize();
            }
        }

        public void Handle( Events.MilkInitialized args ) {
            Initialize();
        }

        public void LoadLibrary( string libraryName ) {
            if( SwitchLibrary( libraryName )) {
                var preferences = mPreferences.Load<MilkPreferences>();

                preferences.CurrentPresetLibrary = libraryName;
                mPreferences.Save( preferences );
            }
        }

        private bool SwitchLibrary( string libraryName ) {
            var retValue = false;
            var library = mLibrarian.GetLibrary( libraryName );

            if(( library != null ) &&
               ( library.Presets.Any())) {
                LoadPresets( library.Presets );

                CurrentPresetLibrary = libraryName;
                mEventAggregator.PublishOnUIThread( new Events.PresetLibrarySwitched( CurrentPresetLibrary ));

                retValue = true;
            }

            return retValue;
        }

        private void Initialize() {
            mProjectM.setPresetCallback( OnPresetSwitched );
            mProjectM.setShuffleEnabled( false );
            mProjectM.setPresetLock( true );

            var preferences = mPreferences.Load<MilkPreferences>();

            mPresetDuration = preferences.PresetPlayDurationInSeconds;
            mPlayRandom = preferences.PlayPresetsRandomly;
            BlendPresetTransition = preferences.BlendPresetTransition;
            
            if(!String.IsNullOrWhiteSpace( preferences.CurrentPresetLibrary )) {
                SwitchLibrary( preferences.CurrentPresetLibrary );
            }
            else {
                SwitchLibrary( mLibrarian.AvailableLibraries.FirstOrDefault());
            }

            mPresetTimer.Interval = TimeSpan.FromSeconds( mPresetDuration );

            if( mPlayRandom ) {
                mPresetTimer.Start();
            }
        }

        public void SelectNextPreset() {
            var presetCount = mProjectM.getPlaylistSize();

            if( mPlayRandom ) {
                var index = mRandom.Next( 0, (int)presetCount );

                PlayPreset( index );
            }
            else {
                var currentIndex = (int)mProjectM.selectedPresetIndex() + 1;

                if( currentIndex >= presetCount ) {
                    currentIndex = 0;
                }

                PlayPreset( currentIndex );
            }
        }

        public void SelectPreviousPreset() {
            mProjectM.selectPrevious( !BlendPresetTransition );
        }

        public void SelectRandomPreset() {
            mProjectM.selectRandom( !BlendPresetTransition );
        }

        public bool PresetCycling {
            get => mPlayRandom;
            set {
                mPlayRandom = value;

                if( mPlayRandom ) {
                    mPresetTimer.Start();
                }
                else {
                    mPresetTimer.Stop();
                }

                var preferences = mPreferences.Load<MilkPreferences>();

                preferences.PlayPresetsRandomly = mPlayRandom;
                mPreferences.Save( preferences );
            }
        }

        private void OnPresetTimer( object sender, EventArgs args ) {
            SelectNextPreset();
        }

        private void PlayPreset( int index ) {
            if( index < mProjectM.getPlaylistSize()) {
                mProjectM.selectPreset((uint)index, !BlendPresetTransition );
            }
        }

        private void LoadPresets( IEnumerable<MilkDropPreset> presetList ) {
            try {
                mProjectM.clearPresetlist();

                foreach( var preset in presetList ) {
                    mProjectM.addPresetURL( preset.PresetLocation, preset.PresetName );
                }

                mProjectM.selectNext( !BlendPresetTransition );
            }
            catch( Exception ex ) {
                mLog.LogException( "PresetController:LoadPresets", ex );
            }
        }

        private void OnPresetSwitched(bool isHardCut, ulong presetIndex ) {
            mCurrentPreset.OnNext( new MilkDropPreset( mProjectM.getPresetName( presetIndex ), mProjectM.getPresetURL( presetIndex )));
        }

        public void Dispose() {
            mPresetTimer.Stop();
            mCurrentPreset?.Dispose();

            mEventAggregator.Unsubscribe( this );
        }
    }
}
