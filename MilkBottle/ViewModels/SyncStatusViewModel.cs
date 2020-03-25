using System;
using System.IO;
using Caliburn.Micro;
using MilkBottle.Entities;
using MilkBottle.Interfaces;
using MilkBottle.Types;
using ReusableBits.Mvvm.ViewModelSupport;

namespace MilkBottle.ViewModels {
    class SyncStatusViewModel : PropertyChangeBase, IDisposable, 
                                IHandle<Events.ModeChanged>, IHandle<Events.PlaybackNotification> {
        private readonly IEventAggregator       mEventAggregator;
        private readonly IPresetController      mPresetController;
        private readonly IPresetListProvider    mListProvider;
        private readonly ISyncManager           mSyncManager;
        private IDisposable                     mPresetSubscription;

        public  string                          SceneName { get; private set; }
        public  string                          TrackName {  get; private set; }
        public  string                          PresetName { get; private set; }

        public SyncStatusViewModel( IPresetController presetController, ISyncManager syncManager, IPresetListProvider listProvider, IEventAggregator eventAggregator ) {
            mEventAggregator = eventAggregator;
            mSyncManager = syncManager;
            mListProvider = listProvider;
            mPresetController = presetController;

            mPresetSubscription = mPresetController.CurrentPreset.Subscribe( OnPresetChanged );
        }

        public void Handle( Events.ModeChanged args ) {
            if( args.ToView != ShellView.Sync ) {
                mEventAggregator.Unsubscribe( this );
            }
        }

        public void Handle( Events.PlaybackNotification args ) {
            var scene = mSyncManager.SelectScene( args.PlaybackEvent );

            if( scene != null ) {
                var list = mListProvider.GetPresets( scene.SourceListType, scene.SourceId );

                mPresetController.LoadPresets( list );

                if( scene.SceneSource != SceneSource.SinglePreset ) {
                    switch( scene.PresetCycle ) {
                        case PresetCycling.Duration:
                            mPresetController.PresetDuration = PresetDuration.Create( scene.PresetDuration );
                            break;

                        case PresetCycling.CountPerScene:
                            mPresetController.PresetDuration = PresetDuration.Create((int)((double)args.PlaybackEvent.TrackLength / scene.PresetDuration ));
                            break;
                    }
                }

                SceneName = scene.Name;
                TrackName = $"{args.PlaybackEvent.ArtistName}/{args.PlaybackEvent.TrackName}";

                RaisePropertyChanged( () => SceneName );
                RaisePropertyChanged( () => TrackName );
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
