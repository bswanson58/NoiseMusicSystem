using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Threading;
using Caliburn.Micro;
using MilkBottle.Dto;
using MilkBottle.Interfaces;
using MilkBottle.Support;

namespace MilkBottle.Models {
    class PresetController : IPresetController, IHandle<Events.MilkInitialized>, IHandle<Events.MilkUpdated>, IHandle<Events.PresetLibraryInitialized> {
        private readonly IEventAggregator           mEventAggregator;
        private readonly IPlatformLog               mLog;
        private readonly IPresetLibrarian           mLibrarian;
        private readonly ProjectMWrapper            mProjectM;
        private readonly IPreferences               mPreferences;
        private readonly Subject<MilkDropPreset>    mCurrentPreset;
        private readonly LimitedStack<ulong>        mPresetHistory;
        private readonly DispatcherTimer            mPresetTimer;
        private readonly LimitedRepeatingRandom     mRandom;
        private int                                 mCurrentPresetIndex;
        private int                                 mPresetDuration;
        private bool                                mPlayRandom;
        public  bool                                IsInitialized { get; private set; }
        public  bool                                BlendPresetTransition { get; set; }
        public  string                              CurrentPresetLibrary { get; private set; }
        public  bool                                IsRunning { get; private set; }

        public  IObservable<MilkDropPreset>         CurrentPreset => mCurrentPreset.AsObservable();

        public PresetController( ProjectMWrapper projectM, IPresetLibrarian librarian, IPreferences preferences, IEventAggregator eventAggregator, IPlatformLog log ) {
            mProjectM = projectM;
            mLibrarian = librarian;
            mPreferences = preferences;
            mEventAggregator = eventAggregator;
            mLog = log;

            mPresetHistory = new LimitedStack<ulong>( 100 );
            mRandom = new LimitedRepeatingRandom( 0.6 );
            mCurrentPreset = new Subject<MilkDropPreset>();
            mPresetTimer = new DispatcherTimer();
            mPresetTimer.Tick += OnPresetTimer;
            mCurrentPresetIndex = -1;
            IsRunning = false;
            IsInitialized = false;

            mEventAggregator.Subscribe( this );

            Initialize();
        }

        private void Initialize() {
            if(( mProjectM.isInitialized()) &&
               ( mLibrarian.IsInitialized )) {
                InitializeMilk();

                StartPresetCycling();

                mEventAggregator.PublishOnUIThread( new Events.PresetControllerInitialized());
            }
        }

        public void Handle( Events.MilkInitialized args ) {
            Initialize();
        }

        public void Handle( Events.MilkUpdated args ) {
            var wasRunning = IsRunning;

            if( wasRunning ) {
                StopPresetCycling();
            }

            InitializeMilk();

            if( wasRunning ) {
                StartPresetCycling();
            }
        }

        public void Handle( Events.PresetLibraryInitialized args ) {
            Initialize();
        }
 
        private void InitializeMilk() {
            mProjectM.setPresetCallback( OnPresetSwitched );
            mProjectM.setShuffleEnabled( false );
            mProjectM.setPresetLock( true );

            var preferences = mPreferences.Load<MilkPreferences>();

            mPresetDuration = preferences.PresetPlayDurationInSeconds;
            mPlayRandom = preferences.PlayPresetsRandomly;
            BlendPresetTransition = preferences.BlendPresetTransition;

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

            if( mProjectM.isInitialized()) {
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
            mPresetTimer.Stop();

            IsRunning = false;
        }

        public void StartPresetCycling() {
            UpdateTimer();
            mPresetTimer.Start();

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

        public bool RandomPresetCycling {
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

        public int PresetDuration {
            get => mPresetDuration;
            set {
                mPresetDuration = Math.Min( 60, Math.Max( 5, value ));

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

        private void UpdateTimer() {
            mPresetTimer.Interval = TimeSpan.FromSeconds( mPresetDuration );
        }

        private void PlayPreset( ulong index ) {
            if( index < mProjectM.getPresetListSize()) {
                if( mCurrentPresetIndex != -1 ) {
                    mPresetHistory.Push((ulong)mCurrentPresetIndex );
                }

                mProjectM.selectPreset((uint)index, !BlendPresetTransition );

                mCurrentPresetIndex = (int)index;
                UpdateTimer();
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

            mPresetTimer.Tick -= OnPresetTimer;
            mCurrentPreset?.Dispose();
            mEventAggregator.Unsubscribe( this );
        }
    }
}
