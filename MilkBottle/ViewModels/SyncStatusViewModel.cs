using System;
using System.IO;
using Caliburn.Micro;
using MilkBottle.Entities;
using MilkBottle.Interfaces;
using MilkBottle.Types;
using ReusableBits.Mvvm.ViewModelSupport;
using ReusableBits.Platform;

namespace MilkBottle.ViewModels {
    class SyncStatusViewModel : PropertyChangeBase, IDisposable, 
                                IHandle<Events.InitializationComplete>, IHandle<Events.ModeChanged>, IHandle<Events.PlaybackNotification> {
        private readonly IEventAggregator       mEventAggregator;
        private readonly IPresetController      mPresetController;
        private readonly IPresetListProvider    mListProvider;
        private readonly ISyncManager           mSyncManager;
        private readonly IStateManager          mStateManager;
        private IDisposable                     mPresetSubscription;
        private PlaybackEvent                   mCurrentPlayback;

        public  string                          SceneName { get; private set; }
        public  string                          TrackName {  get; private set; }
        public  string                          PresetName { get; private set; }

        public SyncStatusViewModel( IPresetController presetController, ISyncManager syncManager, IPresetListProvider listProvider,
                                    IStateManager stateManger, IEventAggregator eventAggregator ) {
            mEventAggregator = eventAggregator;
            mSyncManager = syncManager;
            mListProvider = listProvider;
            mPresetController = presetController;
            mStateManager = stateManger;

            mPresetSubscription = mPresetController.CurrentPreset.Subscribe( OnPresetChanged );

            if( mPresetController.IsInitialized ) {
                Initialize();
            }

            mEventAggregator.Subscribe( this );
        }

        public void Handle( Events.InitializationComplete args ) {
            Initialize();
        }

        private void Initialize() {
            mEventAggregator.Subscribe( this );

            if( mCurrentPlayback != null ) {
                StartPlayback( mCurrentPlayback );
            }
            else {
                StartScene( mSyncManager.GetDefaultScene());

                TrackName = String.Empty;
                RaisePropertyChanged( () => TrackName );
            }

            mStateManager.EnterState( eStateTriggers.Run );
        }

        public void Handle( Events.ModeChanged args ) {
            if( args.ToView != ShellView.Sync ) {
                mEventAggregator.Unsubscribe( this );
            }

            mPresetSubscription?.Dispose();
            mPresetSubscription = null;
        }

        public void Handle( Events.PlaybackNotification args ) {
            mCurrentPlayback = args.PlaybackEvent;

            StartPlayback( mCurrentPlayback );
        }

        public void StartPlayback( PlaybackEvent playbackEvent ) {
            if( playbackEvent != null ) {
                StartScene( mSyncManager.SelectScene( playbackEvent ));

                TrackName = $"{playbackEvent.ArtistName}/{playbackEvent.TrackName}";
                RaisePropertyChanged( () => TrackName );
            }
        }

        public void StartScene( PresetScene scene ) {
            if( scene != null ) {
                var list = mListProvider.GetPresets( scene.SourceListType, scene.SourceId );

                mPresetController.LoadPresets( list );

                if( scene.SceneSource != SceneSource.SinglePreset ) {
                    var trackDuration = mCurrentPlayback?.TrackLength ?? 600;

                    switch( scene.PresetCycle ) {
                        case PresetCycling.Duration:
                            mPresetController.ConfigurePresetTimer( PresetTimer.Infinite );
                            mPresetController.PresetDuration = PresetDuration.Create( scene.PresetDuration );
                            break;

                        case PresetCycling.CountPerScene:
                            mPresetController.ConfigurePresetTimer( PresetTimer.FixedDuration );
                            mPresetController.PresetDuration = PresetDuration.Create((int)((double)trackDuration / scene.PresetDuration ));
                            break;
                    }

                    mPresetController.BlendPresetTransition = scene.OverlapPresets;
                }

                SceneName = scene.Name;
                RaisePropertyChanged( () => SceneName );
            }
        }

        private void OnPresetChanged( Preset preset ) {
            if( preset != null ) {
                PresetName = Path.GetFileNameWithoutExtension( preset.Name );

                RaisePropertyChanged( () => PresetName );
            }
        }

        public void Dispose() {
            mPresetSubscription?.Dispose();
            mPresetSubscription = null;

            mEventAggregator.Unsubscribe( this );
        }
    }
}
