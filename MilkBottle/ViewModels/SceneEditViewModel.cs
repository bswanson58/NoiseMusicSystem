using System;
using System.Collections.ObjectModel;
using System.Linq;
using MilkBottle.Dto;
using MilkBottle.Entities;
using MilkBottle.Interfaces;
using MilkBottle.Views;
using Prism;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReusableBits.Mvvm.ViewModelSupport;

namespace MilkBottle.ViewModels {
    class SceneEditViewModel : PropertyChangeBase, IActiveAware {
        private readonly ISceneProvider mSceneProvider;
        private readonly IDialogService mDialogService;
        private readonly IPlatformLog   mLog;
        private UiScene                 mCurrentScene;
        private bool                    mIsActive;

        public  ObservableCollection<UiScene>   Scenes { get; }

        public  DelegateCommand                 NewScene { get; }

        public  string                          Title => "Scenes";
        public  event EventHandler              IsActiveChanged = delegate { };

        public SceneEditViewModel( ISceneProvider sceneProvider, IStateManager stateManager, IDialogService dialogService, IPlatformLog log ) {
            mSceneProvider = sceneProvider;
            mDialogService = dialogService;
            mLog = log;

            Scenes = new ObservableCollection<UiScene>();
            NewScene = new DelegateCommand( OnNewScene );

            stateManager.EnterState( eStateTriggers.Stop );
        }

        public bool IsActive {
            get => mIsActive;
            set {
                mIsActive = value;

                if( mIsActive ) {
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

        private void OnSceneSelected() { }

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

        private void LogException( string message, Exception ex ) {
            mLog.LogException( message, ex );
        }
    }
}
