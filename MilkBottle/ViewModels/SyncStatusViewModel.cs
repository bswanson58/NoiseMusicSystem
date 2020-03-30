using System;
using System.IO;
using Caliburn.Micro;
using MilkBottle.Dto;
using MilkBottle.Entities;
using MilkBottle.Interfaces;
using MilkBottle.Types;
using MilkBottle.Views;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReusableBits.Mvvm.ViewModelSupport;
using ReusableBits.Platform;

namespace MilkBottle.ViewModels {
    class SyncStatusViewModel : PropertyChangeBase, IDisposable, 
                                IHandle<Events.InitializationComplete>, IHandle<Events.ModeChanged> {
        private readonly IEventAggregator       mEventAggregator;
        private readonly IPresetController      mPresetController;
        private readonly IPresetListProvider    mListProvider;
        private readonly ISceneProvider         mSceneProvider;
        private readonly IIpcManager            mIpcManager;
        private readonly ISyncManager           mSyncManager;
        private readonly IStateManager          mStateManager;
        private readonly IDialogService         mDialogService;
        private readonly IPreferences           mPreferences;
        private readonly IPlatformLog           mLog;
        private IDisposable                     mPresetSubscription;
        private IDisposable                     mPlaybackSubscription;
        private PlaybackEvent                   mCurrentPlayback;
        private PresetScene                     mCurrentScene;

        public  DelegateCommand                 SceneWizard { get; }

        public  string                          SceneName { get; private set; }
        public  string                          TrackName {  get; private set; }
        public  string                          PresetName { get; private set; }

        public SyncStatusViewModel( IPresetController presetController, ISyncManager syncManager, IPresetListProvider listProvider, ISceneProvider sceneProvider,
                                    IStateManager stateManger, IDialogService dialogService, IIpcManager ipcManager,
                                    IEventAggregator eventAggregator, IPreferences preferences, IPlatformLog log ) {
            mEventAggregator = eventAggregator;
            mSyncManager = syncManager;
            mListProvider = listProvider;
            mSceneProvider = sceneProvider;
            mPresetController = presetController;
            mStateManager = stateManger;
            mIpcManager = ipcManager;
            mDialogService = dialogService;
            mPreferences = preferences;
            mLog = log;

            SceneWizard = new DelegateCommand( OnSceneWizard, CanExecuteSceneWizard );

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

            mPresetSubscription = mPresetController.CurrentPreset.Subscribe( OnPresetChanged );
            mPlaybackSubscription = mIpcManager.OnPlaybackEvent.Subscribe( OnPlaybackEvent );

            if( mCurrentPlayback?.IsValidEvent == true ) {
                StartPlayback( mCurrentPlayback );
            }
            else {
                StartScene( mSyncManager.GetDefaultScene());

                TrackName = String.Empty;
                RaisePropertyChanged( () => TrackName );
            }

            mStateManager.EnterState( eStateTriggers.Run );

            SceneWizard.RaiseCanExecuteChanged();
        }

        public void Handle( Events.ModeChanged args ) {
            if( args.ToView != ShellView.Sync ) {
                mEventAggregator.Unsubscribe( this );

                mPresetSubscription?.Dispose();
                mPresetSubscription = null;

                mPlaybackSubscription?.Dispose();
                mPlaybackSubscription = null;
            }
        }

        private void OnPlaybackEvent( PlaybackEvent args ) {
            mCurrentPlayback = args;

            StartPlayback( mCurrentPlayback );
        }

        public void StartPlayback( PlaybackEvent playbackEvent ) {
            if( playbackEvent != null ) {
                StartScene( mSyncManager.SelectScene( playbackEvent ));

                TrackName = $"{playbackEvent.ArtistName}/{playbackEvent.TrackName}";
                RaisePropertyChanged( () => TrackName );
            }

            SceneWizard.RaiseCanExecuteChanged();
        }

        public void StartScene( PresetScene scene ) {
            if( scene != null ) {
                var list = mListProvider.GetPresets( scene.SourceListType, scene.SourceId );

                mPresetController.LoadPresets( list );

                if( scene.SceneSource != SceneSource.SinglePreset ) {
                    var trackDuration = mCurrentPlayback?.TrackLength ?? 600;

                    switch( scene.PresetCycle ) {
                        case PresetCycling.Duration:
                            mPresetController.PresetDuration = PresetDuration.Create( scene.PresetDuration );
                            break;

                        case PresetCycling.CountPerScene:
                            mPresetController.PresetDuration = PresetDuration.Create((int)((double)trackDuration / scene.PresetDuration ));
                            break;
                    }

                    mPresetController.ConfigurePresetTimer( PresetTimer.FixedDuration );
                    mPresetController.BlendPresetTransition = scene.OverlapPresets;
                }
                else {
                    mPresetController.ConfigurePresetTimer( PresetTimer.Infinite );

                    OnPresetChanged( mPresetController.GetPlayingPreset());
                }

                mCurrentScene = scene;
                SceneName = scene.Name;

                var preferences = mPreferences.Load<MilkPreferences>();

                if( mCurrentScene.Id.ToString().Equals( preferences.DefaultScene )) {
                    SceneName += " (default)";
                }

                RaisePropertyChanged( () => SceneName );
                SceneWizard.RaiseCanExecuteChanged();
            }
        }

        private void OnSceneWizard() {
            if(( mCurrentScene != null ) &&
               ( mCurrentPlayback != null )) {
                var dialogParameters = new DialogParameters{{ SceneWizardDialogModel.cPlaybackParameter, mCurrentPlayback }, 
                                                            { SceneWizardDialogModel.cSceneParameter, mCurrentScene }};

                mDialogService.ShowDialog( nameof( SceneWizardDialog ), dialogParameters, OnSceneWizardResult );
            }
        }

        private void OnSceneWizardResult( IDialogResult result ) {
            if( result.Result == ButtonResult.OK ) {
                var newScene = result.Parameters.GetValue<bool>( SceneWizardDialogModel.cNewSceneCreatedParameter );
                var scene = result.Parameters.GetValue<PresetScene>( SceneWizardDialogModel.cSceneParameter );

                if( newScene ) {
                    mSceneProvider.Insert( scene ).IfLeft( ex => mLog.LogException( "OnSceneWizardResult.Insert", ex ));
                }
                else {
                    mSceneProvider.Update( scene ).IfLeft( ex => mLog.LogException( "OnSceneWizardResult.Update", ex ));

                    mCurrentScene = scene;
                }
            }
        }

        private bool CanExecuteSceneWizard() {
            return ( mCurrentScene != null ) && ( mCurrentPlayback != null );
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
