﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Caliburn.Micro;
using MilkBottle.Dto;
using MilkBottle.Entities;
using MilkBottle.Interfaces;
using MilkBottle.Support;
using MilkBottle.Types;

namespace MilkBottle.Models {
    class PresetController : IPresetController, IHandle<Events.ModeChanged> {
        private readonly IEventAggregator           mEventAggregator;
        private readonly IPlatformLog               mLog;
        private readonly IPresetProvider            mPresetProvider;
        private readonly ProjectMWrapper            mProjectM;
        private readonly IPreferences               mPreferences;
        private readonly Subject<Preset>            mCurrentPreset;
        private readonly LimitedStack<ulong>        mPresetHistory;
        private readonly IPresetTimerFactory        mTimerFactory;
        private readonly IPresetSequencerFactory    mSequencerFactory;
        private readonly List<Preset>               mLoadedPresets;
        private IPresetTimer                        mPresetTimer;
        private IPresetSequencer                    mPresetSequencer;
        private int                                 mCurrentPresetIndex;
        private PresetDuration                      mPresetDuration;
        public  bool                                IsInitialized { get; private set; }
        public  bool                                BlendPresetTransition { get; set; }
        public  string                              CurrentPresetLibrary { get; private set; }
        public  bool                                IsRunning { get; private set; }

        public  IObservable<Preset>                 CurrentPreset => mCurrentPreset.AsObservable();

        public PresetController( ProjectMWrapper projectM, IPresetProvider presetProvider, 
                                 IPresetTimerFactory timerFactory, IPresetSequencerFactory sequencerFactory,
                                 IPreferences preferences, IEventAggregator eventAggregator, IPlatformLog log ) {
            mProjectM = projectM;
            mPresetProvider = presetProvider;
            mPreferences = preferences;
            mTimerFactory = timerFactory;
            mSequencerFactory = sequencerFactory;
            mEventAggregator = eventAggregator;
            mLog = log;

            mPresetHistory = new LimitedStack<ulong>( 100 );
            mCurrentPreset = new Subject<Preset>();
            mPresetDuration = PresetDuration.Create( PresetDuration.MaximumValue );
            mLoadedPresets = new List<Preset>();
            mCurrentPresetIndex = -1;
            IsRunning = false;
            IsInitialized = false;

            mEventAggregator.Subscribe( this );
        }

        public void Initialize() {
            if( mProjectM.isInitialized()) {
                InitializeMilk();
            
                ConfigurePresetTimer( PresetTimer.Infinite );
                ConfigurePresetSequencer( PresetSequence.Random );
            }
        }

        public void Handle( Events.ModeChanged args ) {
            IsInitialized = false;
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

        public void ConfigurePresetSequencer( PresetSequence forSequence ) {
            mPresetSequencer = mSequencerFactory.CreateSequencer( forSequence );
        }

        private void InitializeMilk() {
            mProjectM.setPresetCallback( OnPresetSwitched );
            mProjectM.setShuffleEnabled( false );
            mProjectM.setPresetLock( true );

            var preferences = mPreferences.Load<MilkPreferences>();

            mPresetDuration = preferences.PresetPlayDurationInSeconds;

            mProjectM.showFrameRate( preferences.DisplayFps );

            IsInitialized = true;
        }

        public Preset GetPlayingPreset() {
            var retValue = default( Preset );

            if(( mProjectM.isInitialized()) &&
               ( mProjectM.getPresetListSize() > 0 )) {
               var presetIndex = mProjectM.selectedPresetIndex();

                retValue = new Preset( mProjectM.getPresetName(presetIndex ), mProjectM.getPresetURL( presetIndex ), PresetLibrary.Default());
            }

            return retValue;
        }

        public void LoadLibrary( PresetLibrary library ) {
            if( SwitchLibrary( library )) {
                var preferences = mPreferences.Load<MilkPreferences>();

                preferences.CurrentPresetLibrary = library.Name;
                mPreferences.Save( preferences );
            }
        }

        public void StopPresetCycling() {
            mPresetTimer?.StopTimer();

            IsRunning = false;
        }

        public void StartPresetCycling() {
            mPresetTimer.ReloadTimer();
            mPresetTimer.StartTimer();

            IsRunning = true;
        }

        private bool SwitchLibrary( PresetLibrary forLibrary ) {
            var retValue = false;

            mLoadedPresets.Clear();
            mPresetProvider.SelectPresets( forLibrary, list => mLoadedPresets.AddRange( list ));

            if( mLoadedPresets.Any()) {
                LoadPresets( mLoadedPresets );

                CurrentPresetLibrary = forLibrary.Name;
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
            var currentIndex = mProjectM.selectedPresetIndex();

            PlayPreset( mPresetSequencer.SelectNextPreset( presetCount, (uint)currentIndex ));
        }

        public void SelectPreviousPreset() {
            if( mPresetHistory.Count > 0 ) {
                mCurrentPresetIndex = -1;

                PlayPreset( mPresetHistory.Pop());
            }
        }

        public PresetDuration PresetDuration {
            get => mPresetDuration;
            set {
                mPresetDuration = value;
                mPresetTimer.SetDuration( mPresetDuration );

                var preferences = mPreferences.Load<MilkPreferences>();

                preferences.PresetPlayDurationInSeconds = mPresetDuration;
                mPreferences.Save( preferences );
            }
        }

        public void PlayPreset( Preset preset ) {
            var index = mProjectM.addPresetURL( preset.Location, preset.Id.ToString());

            PlayPreset( index );

            mLoadedPresets.Add( preset );
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

        private void LoadPresets( IEnumerable<Preset> presetList ) {
            try {
                mProjectM.clearPresetlist();
                mPresetHistory.Clear();
                mCurrentPresetIndex = -1;

                foreach( var preset in presetList ) {
                    mProjectM.addPresetURL( preset.Location, preset.Id.ToString());
                }

                SelectNextPreset();
            }
            catch( Exception ex ) {
                mLog.LogException( "PresetController:LoadPresets", ex );
            }
        }

        private void OnPresetSwitched(bool isHardCut, ulong presetIndex ) {
            var id = mProjectM.getPresetName( presetIndex );

            if(!String.IsNullOrWhiteSpace( id )) {
                var preset = mLoadedPresets.FirstOrDefault( p => p.Id.ToString().Equals( id ));

                if( preset != null ) {
                    mCurrentPreset.OnNext( preset );
                }
            }
        }

        public void Dispose() {
            StopPresetCycling();

            mPresetTimer?.StopTimer();
            mCurrentPreset?.Dispose();
            mEventAggregator.Unsubscribe( this );
        }
    }
}
