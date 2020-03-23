using System;
using System.Collections.ObjectModel;
using System.Linq;
using LiteDB;
using MilkBottle.Dto;
using MilkBottle.Entities;
using MilkBottle.Interfaces;
using MilkBottle.Types;
using MilkBottle.Views;
using Prism;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReusableBits.Mvvm.ViewModelSupport;

namespace MilkBottle.ViewModels {
    class UiSource {
        public  SceneSource Source { get; }
        public  string      Title { get; }

        public UiSource( string title, SceneSource source ) {
            Source = source;
            Title = title;
        }
    }

    class UiCycling {
        public  PresetCycling   Cycling { get; }
        public  string          Title { get; }

        public UiCycling( string title, PresetCycling cycling ) {
            Cycling = cycling;
            Title = title;
        }
    }

    class SceneEditViewModel : PropertyChangeBase, IActiveAware {
        private readonly ISceneProvider             mSceneProvider;
        private readonly IPresetProvider            mPresetProvider;
        private readonly IPresetListProvider        mListProvider;
        private readonly IDialogService             mDialogService;
        private readonly IPlatformLog               mLog;
        private UiScene                             mCurrentScene;
        private UiSource                            mCurrentSource;
        private UiCycling                           mCurrentCycling;
        private Preset                              mCurrentPreset;
        private PresetList                          mCurrentList;
        private int                                 mCurrentCycleDuration;
        private int                                 mCurrentPresetOverlap;
        private bool                                mIsActive;

        public  ObservableCollection<UiScene>       Scenes { get; }
        public  ObservableCollection<UiSource>      SceneSources { get; }
        public  ObservableCollection<UiCycling>     PresetCycling { get; }
        public  ObservableCollection<PresetList>    PresetLists { get; }

        public  DelegateCommand                     NewScene { get; }
        public  DelegateCommand                     SelectPreset { get; }

        public  string                              Title => "Scenes";

        public  bool                                ArePropertiesValid => mCurrentScene != null;

        public  string                              CurrentPresetName => mCurrentPreset?.Name;
        public  bool                                IsPresetSource => mCurrentSource?.Source == SceneSource.SinglePreset;
        public  bool                                IsListSource => mCurrentSource?.Source == SceneSource.PresetList;

        public  int                                 MinimumCycleDuration { get; private set; }
        public  int                                 MaximumCycleDuration { get; private set; }
        public  string                              CycleDurationLegend => mCurrentCycling?.Cycling == Entities.PresetCycling.Duration ? 
                                                                                    $"{mCurrentCycleDuration} seconds per preset" : 
                                                                                    $"{mCurrentCycleDuration} presets per track";
        public  bool                                CanCycle => IsListSource;

        public  int                                 MinimumPresetOverlap => 0;
        public  int                                 MaximumPresetOverlap => 5;
        public  string                              PresetOverlapLegend => mCurrentPresetOverlap > 0 ? $"{mCurrentPresetOverlap} seconds" : "No Overlap";
        public  bool                                CanOverlap => IsListSource;

        public  event EventHandler                  IsActiveChanged = delegate { };

        public SceneEditViewModel( ISceneProvider sceneProvider, IPresetListProvider listProvider, IPresetProvider presetProvider,
                                   IStateManager stateManager, IDialogService dialogService, IPlatformLog log ) {
            mSceneProvider = sceneProvider;
            mPresetProvider = presetProvider;
            mListProvider = listProvider;
            mDialogService = dialogService;
            mLog = log;

            Scenes = new ObservableCollection<UiScene>();
            PresetLists = new ObservableCollection<PresetList>();
            SceneSources = new ObservableCollection<UiSource> {
                new UiSource( "Preset List", SceneSource.PresetList ),
                new UiSource( "Single Preset", SceneSource.SinglePreset )
            };
            PresetCycling = new ObservableCollection<UiCycling> {
                new UiCycling( "Count", Entities.PresetCycling.CountPerScene ),
                new UiCycling( "Duration", Entities.PresetCycling.Duration )
            };

            NewScene = new DelegateCommand( OnNewScene );
            SelectPreset = new DelegateCommand( OnSelectPreset );

            stateManager.EnterState( eStateTriggers.Stop );
        }

        public bool IsActive {
            get => mIsActive;
            set {
                mIsActive = value;

                if( mIsActive ) {
                    LoadLists();
                    LoadScenes();
                }
            }
        }

        public UiScene CurrentScene {
            get => mCurrentScene;
            set {
                mCurrentScene = value;

                OnSceneSelected();
                RaisePropertyChanged( () => CurrentScene );
            }
        }

        private void OnSceneSelected() {
            if( mCurrentScene != null ) {
                mCurrentSource = SceneSources.FirstOrDefault( s => s.Source.Equals( mCurrentScene.Scene.SceneSource ));

                if( mCurrentScene.Scene.SourceId != ObjectId.Empty ) {
                    if( mCurrentScene.Scene.SceneSource == SceneSource.SinglePreset ) {
                        mPresetProvider.GetPresetById( mCurrentScene.Scene.SourceId )
                            .Match( 
                                p => p.Do( preset => mCurrentPreset = preset ),
                                ex => LogException( "", ex )
                                );
                    }
                    if( mCurrentScene.Scene.SceneSource == SceneSource.PresetList ) {
                        SelectedList = PresetLists.FirstOrDefault( l => l.ListIdentifier.Equals( mCurrentScene.Scene.SourceId ));
                    }
                }

                mCurrentCycling = PresetCycling.FirstOrDefault( c => c.Cycling.Equals( mCurrentScene.Scene.PresetCycle ));
                mCurrentCycleDuration = mCurrentScene.Scene.PresetDuration;
                UpdateCycling();

                mCurrentPresetOverlap = mCurrentScene.Scene.OverlapDuration;
            }

            RaisePropertyChanged( () => ArePropertiesValid );
            RaisePropertyChanged( () => IsListSource );
            RaisePropertyChanged( () => IsPresetSource );
            RaisePropertyChanged( () => CurrentPresetName );
            RaisePropertyChanged( () => SelectedSource );
            RaisePropertyChanged( () => CurrentCycling );
            RaisePropertyChanged( () => CurrentCycleDuration );
            RaisePropertyChanged( () => CanCycle );
            RaisePropertyChanged( () => CycleDurationLegend );
            RaisePropertyChanged( () => CurrentPresetOverlap );
            RaisePropertyChanged( () => PresetOverlapLegend );
            RaisePropertyChanged( () => CanOverlap );
        }

        private void LoadLists() {
            PresetLists.Clear();

            PresetLists.AddRange( mListProvider.GetLists());
            SelectedList = PresetLists.FirstOrDefault();
        }

        private void LoadScenes() {
            var previousScene = mCurrentScene;

            Scenes.Clear();
            mSceneProvider.SelectScenes( list => Scenes.AddRange( from s in list orderby s.Name select new UiScene( s, OnEditScene, OnDeleteScene )))
                .IfLeft( ex => LogException( "SelectScenes", ex ));

            if( previousScene != null ) {
                CurrentScene = Scenes.FirstOrDefault( s => s.Scene.Id.Equals( previousScene.Scene.Id ));
            }

            if( CurrentScene == null ) {
                CurrentScene = Scenes.FirstOrDefault();
            }
        }

        private void OnNewScene() {
            mDialogService.ShowDialog( nameof( NewSceneDialog ), new DialogParameters(), OnNewSceneResult );
        }

        private void OnNewSceneResult( IDialogResult result ) {
            if( result.Result == ButtonResult.OK ) {
                var newScene = new PresetScene( result.Parameters.GetValue<string>( NewSceneDialogModel.cSceneNameParameter ));

                mSceneProvider.Insert( newScene ).IfLeft( ex => LogException( "OnNewSceneResult", ex ));

                LoadScenes();
                CurrentScene = Scenes.FirstOrDefault( t => t.Scene.Id.Equals( newScene.Id ));
            }
        }

        private void OnEditScene( UiScene scene ) {
            if( scene != null ) {
                mCurrentScene = scene;

                mDialogService.ShowDialog( nameof( NewSceneDialog), new DialogParameters {{ NewSceneDialogModel.cSceneNameParameter, scene.Name }}, OnEditSceneResult );
            }
        }

        private void OnEditSceneResult( IDialogResult result ) {
            if(( result.Result == ButtonResult.OK ) &&
               ( mCurrentScene != null )) {
                var newName = result.Parameters.GetValue<string>( NewSceneDialogModel.cSceneNameParameter );

                if(!String.IsNullOrWhiteSpace( newName )) {
                    var tag = mCurrentScene.Scene.WithName( newName );

                    mSceneProvider.Update( tag ).IfLeft( ex => LogException( "OnEditSceneResult", ex ));

                    LoadScenes();
                }
            }
        }

        private void OnDeleteScene( UiScene scene ) {
            if( scene != null ) {
                mCurrentScene = scene;

                SceneDelete( scene.Scene );
            }
        }

        private void SceneDelete( PresetScene scene ) {
            if( scene != null ) {
                mDialogService.ShowDialog( nameof( ConfirmDeleteDialog ), 
                    new DialogParameters {{ ConfirmDeleteDialogModel.cEntityNameParameter, scene.Name }}, 
                    OnSceneDeleteResult );
            }
        }

        private void OnSceneDeleteResult( IDialogResult result ) {
            if(( result.Result == ButtonResult.OK ) &&
               ( mCurrentScene != null )) {
                mSceneProvider.Delete( mCurrentScene.Scene )
                    .Match( 
                        unit => LoadScenes(), 
                        ex => LogException( "OnSceneDeleteResult", ex ));
            }
        }

        public UiSource SelectedSource {
            get => mCurrentSource;
            set {
                mCurrentSource = value;

                OnSceneSourceChanged();
                RaisePropertyChanged( () => SelectedSource );
                RaisePropertyChanged( () => IsListSource );
                RaisePropertyChanged( () => IsPresetSource );
            }
        }

        public PresetList SelectedList {
            get => mCurrentList;
            set {
                mCurrentList = value;

                OnSceneSourceChanged();
                RaisePropertyChanged( () => SelectedList );
            }
        }

        private void OnSceneSourceChanged() {
            if( mCurrentScene != null ) {
                if(( mCurrentSource?.Source == SceneSource.PresetList ) &&
                   ( mCurrentList != null )) {
                    var newScene = mCurrentScene.Scene.WithSource( SceneSource.PresetList, mCurrentList.ListType, mCurrentList.ListIdentifier );

                    mSceneProvider.Update( newScene ).IfLeft( ex => LogException( "OnSceneSourceChanged", ex ));
                }

                if(( mCurrentSource?.Source == SceneSource.SinglePreset ) &&
                   ( mCurrentPreset != null )) {
                    var newScene = mCurrentScene.Scene.WithSource( SceneSource.SinglePreset, PresetListType.Preset, mCurrentPreset.Id );

                    mSceneProvider.Update( newScene ).IfLeft( ex => LogException( "OnSceneSourceChanged", ex ));
                }

                RaisePropertyChanged( () => CanCycle );
                RaisePropertyChanged( () => CanOverlap );
            }
        }

        private void OnSelectPreset() {
            if( mCurrentScene != null ) {
                mDialogService.ShowDialog( "SelectPresetDialog", new DialogParameters(), OnPresetSelected );
            }
        }

        private void OnPresetSelected( IDialogResult result ) {
            if(( result.Result == ButtonResult.OK ) &&
               ( mCurrentScene != null )) {
                mCurrentPreset = result.Parameters.GetValue<Preset>( SelectPresetDialogModel.cPresetParameter );

                if( mCurrentPreset != null ) {
                    var newScene = mCurrentScene.Scene.WithSource( SceneSource.SinglePreset, PresetListType.Preset, mCurrentPreset.Id );

                    mSceneProvider.Update( newScene ).IfLeft( ex => LogException( "OnPresetSelected", ex ));

                    RaisePropertyChanged( () => CurrentPresetName );
                }
            }
        }

        public UiCycling CurrentCycling {
            get => mCurrentCycling;
            set {
                mCurrentCycling = value;

                OnPresetCyclingChanged();
                RaisePropertyChanged( () => PresetCycling );
            }
        }

        public int CurrentCycleDuration {
            get => mCurrentCycleDuration;
            set {
                mCurrentCycleDuration = value;

                OnPresetCyclingChanged();
                RaisePropertyChanged( () => CurrentCycleDuration );
            }
        }

        private void OnPresetCyclingChanged() {
            UpdateCycling();

            if( mCurrentScene != null ) {
                var newScene = mCurrentScene.Scene.WithCycle( mCurrentCycling.Cycling, mCurrentCycleDuration );

                mSceneProvider.Update( newScene ).IfLeft( ex => LogException( "OnPresetCyclingChanged-Update", ex ));
            }
        }

        private void UpdateCycling() {
            MinimumCycleDuration = mCurrentCycling.Cycling == Entities.PresetCycling.CountPerScene ? 1 : PresetDuration.MinimumValue;
            MaximumCycleDuration = mCurrentCycling.Cycling == Entities.PresetCycling.CountPerScene ? 10 : PresetDuration.MaximumValue;

            if( mCurrentCycling.Cycling.Equals( Entities.PresetCycling.CountPerScene  )) {
                mCurrentCycleDuration = Math.Min( mCurrentCycleDuration, 10 );
                mCurrentCycleDuration = Math.Max( mCurrentCycleDuration, 1 );
            }
            else {
                mCurrentCycleDuration = Math.Min( mCurrentCycleDuration, PresetDuration.MaximumValue );
                mCurrentCycleDuration = Math.Max( mCurrentCycleDuration, PresetDuration.MinimumValue );
            }

            RaisePropertyChanged( () => CycleDurationLegend );
            RaisePropertyChanged( () => CurrentCycleDuration );
            RaisePropertyChanged( () => MinimumCycleDuration );
            RaisePropertyChanged( () => MaximumCycleDuration );
        }

        public int CurrentPresetOverlap {
            get => mCurrentPresetOverlap;
            set {
                mCurrentPresetOverlap = value;

                OnPresetOverlapChanged();
                RaisePropertyChanged( () => CurrentPresetOverlap );
                RaisePropertyChanged( () => PresetOverlapLegend );
            }
        }

        private void OnPresetOverlapChanged() {
            if( mCurrentScene != null ) {
                var newScene = mCurrentScene.Scene.WithOverlap( mCurrentPresetOverlap != 0, mCurrentPresetOverlap );

                mSceneProvider.Update( newScene ).IfLeft( ex => LogException( "OnPresetOverlapChanged-Update", ex ));
            }
        }
        private void LogException( string message, Exception ex ) {
            mLog.LogException( message, ex );
        }
    }
}
