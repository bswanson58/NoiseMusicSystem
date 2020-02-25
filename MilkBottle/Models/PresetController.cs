using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Caliburn.Micro;
using MilkBottle.Dto;
using MilkBottle.Interfaces;
using MilkBottle.Support;

namespace MilkBottle.Models {
    class PresetController : IPresetController {
        private readonly IEventAggregator           mEventAggregator;
        private readonly IPlatformLog               mLog;
        private readonly IPresetLibrarian           mLibrarian;
        private readonly ProjectMWrapper            mProjectM;
        private readonly IPreferences               mPreferences;
        private readonly Subject<MilkDropPreset>    mCurrentPreset;
        private readonly LimitedStack<ulong>        mPresetHistory;
        private readonly LimitedRepeatingRandom     mRandom;
        private readonly IPresetTimerFactory        mTimerFactory;
        private IPresetTimer                        mPresetTimer;
        private int                                 mCurrentPresetIndex;
        private int                                 mPresetDuration;
        private bool                                mPlayRandom;
        public  bool                                IsInitialized { get; private set; }
        public  bool                                BlendPresetTransition { get; set; }
        public  string                              CurrentPresetLibrary { get; private set; }
        public  bool                                IsRunning { get; private set; }

        public  IObservable<MilkDropPreset>         CurrentPreset => mCurrentPreset.AsObservable();

        public PresetController( ProjectMWrapper projectM, IPresetLibrarian librarian, IPresetTimerFactory timerFactory,
                                 IPreferences preferences, IEventAggregator eventAggregator, IPlatformLog log ) {
            mProjectM = projectM;
            mLibrarian = librarian;
            mPreferences = preferences;
            mTimerFactory = timerFactory;
            mEventAggregator = eventAggregator;
            mLog = log;

            mPresetHistory = new LimitedStack<ulong>( 100 );
            mRandom = new LimitedRepeatingRandom( 0.6 );
            mCurrentPreset = new Subject<MilkDropPreset>();
            mCurrentPresetIndex = -1;
            IsRunning = false;
            IsInitialized = false;

            mEventAggregator.Subscribe( this );
        }

        public void Initialize() {
            if(( mProjectM.isInitialized()) &&
               ( mLibrarian.IsInitialized )) {
                InitializeMilk();
            
                ConfigurePresetTimer( PresetTimer.Infinite );
            }
        }

        public void MilkConfigurationUpdated() {
            var wasRunning = IsRunning;

            if( wasRunning ) {
                StopPresetCycling();
            }

            InitializeMilk();

            if( wasRunning ) {
                StartPresetCycling();
            }
        }

        public void ConfigurePresetTimer( PresetTimer timerType ) {
            if( mPresetTimer != null ) {
                mPresetTimer.StopTimer();
                mPresetTimer.PresetTimeElapsed -= OnPresetTimer;
            }

            mPresetTimer = mTimerFactory.CreateTimer( timerType );
            mPresetTimer.SetDuration( mPresetDuration );
            mPresetTimer.PresetTimeElapsed += OnPresetTimer;
        }

        private void InitializeMilk() {
            mProjectM.setPresetCallback( OnPresetSwitched );
            mProjectM.setShuffleEnabled( false );
            mProjectM.setPresetLock( true );

            var preferences = mPreferences.Load<MilkPreferences>();

            mPresetDuration = preferences.PresetPlayDurationInSeconds;
            mPlayRandom = preferences.PlayPresetsRandomly;
            BlendPresetTransition = preferences.BlendPresetTransition;

            mProjectM.showFrameRate( preferences.DisplayFps );

            if((!String.IsNullOrWhiteSpace( preferences.CurrentPresetLibrary )) &&
               ( mLibrarian.ContainsLibrary( preferences.CurrentPresetLibrary ))) {
                SwitchLibrary( preferences.CurrentPresetLibrary );
            }
            else {
                SwitchLibrary( mLibrarian.AvailableLibraries.FirstOrDefault());
            }

            IsInitialized = true;
        }

        public MilkDropPreset GetPlayingPreset() {
            var retValue = default( MilkDropPreset );

            if(( mProjectM.isInitialized()) &&
               ( mProjectM.getPresetListSize() > 0 )) {
               var presetIndex = mProjectM.selectedPresetIndex();

                retValue = new MilkDropPreset( mProjectM.getPresetName(presetIndex ), mProjectM.getPresetURL( presetIndex ));
            }

            return retValue;
        }

        public void LoadLibrary( string libraryName ) {
            if( SwitchLibrary( libraryName )) {
                var preferences = mPreferences.Load<MilkPreferences>();

                preferences.CurrentPresetLibrary = libraryName;
                mPreferences.Save( preferences );
            }
        }

        public void StopPresetCycling() {
            mPresetTimer.StopTimer();

            IsRunning = false;
        }

        public void StartPresetCycling() {
            mPresetTimer.ReloadTimer();
            mPresetTimer.StartTimer();

            IsRunning = true;
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
/*
        private void TestPresets() {
            var presetCount = mProjectM.getPresetListSize();

            for( uint preset = 0; preset < presetCount; preset++ ) {
                mProjectM.selectPreset( preset, true );

                if( mProjectM.getErrorLoadingCurrentPreset()) {
                    var presetName = mProjectM.getPresetName( preset );
                    var presetLocation = mProjectM.getPresetURL( preset );

                    mLog.LogMessage( $"Error loading preset named '{presetName}' at '{presetLocation}'" );
                }
            }
        }
*/
        public void SelectNextPreset() {
            var presetCount = mProjectM.getPresetListSize();

            if( mPlayRandom ) {
                var index = (ulong)mRandom.Next( 0, (int)presetCount );

                PlayPreset( index );
            }
            else {
                var currentIndex = mProjectM.selectedPresetIndex() + 1;

                if( currentIndex >= presetCount ) {
                    currentIndex = 0;
                }

                PlayPreset( currentIndex );
            }
        }

        public void SelectPreviousPreset() {
            if( mPresetHistory.Count > 0 ) {
                mCurrentPresetIndex = -1;

                PlayPreset( mPresetHistory.Pop());
            }
        }

        public int PresetDuration {
            get => mPresetDuration;
            set {
                mPresetDuration = Math.Min( 60, Math.Max( 5, value ));
                mPresetTimer.SetDuration( mPresetDuration );

                var preferences = mPreferences.Load<MilkPreferences>();

                preferences.PresetPlayDurationInSeconds = mPresetDuration;
                mPreferences.Save( preferences );
            }
        }

        public void PlayPreset( MilkDropPreset preset ) {
            var index = mProjectM.addPresetURL( preset.PresetLocation, preset.PresetName );

            PlayPreset( index );
        }

        private void OnPresetTimer( object sender, EventArgs args ) {
            SelectNextPreset();
        }

        private void PlayPreset( ulong index ) {
            if( index < mProjectM.getPresetListSize()) {
                if( mCurrentPresetIndex != -1 ) {
                    mPresetHistory.Push((ulong)mCurrentPresetIndex );
                }

                mProjectM.selectPreset((uint)index, !BlendPresetTransition );

                mCurrentPresetIndex = (int)index;
                mPresetTimer.ReloadTimer();
            }
        }

        private void LoadPresets( IEnumerable<MilkDropPreset> presetList ) {
            try {
                mProjectM.clearPresetlist();
                mPresetHistory.Clear();
                mCurrentPresetIndex = -1;

                foreach( var preset in presetList ) {
                    mProjectM.addPresetURL( preset.PresetLocation, preset.PresetName );
                }

                SelectNextPreset();
            }
            catch( Exception ex ) {
                mLog.LogException( "PresetController:LoadPresets", ex );
            }
        }

        private void OnPresetSwitched(bool isHardCut, ulong presetIndex ) {
            mCurrentPreset.OnNext( new MilkDropPreset( mProjectM.getPresetName( presetIndex ), mProjectM.getPresetURL( presetIndex )));
        }

        public void Dispose() {
            StopPresetCycling();

            mPresetTimer?.StopTimer();
            mCurrentPreset?.Dispose();
            mEventAggregator.Unsubscribe( this );
        }
    }
}
