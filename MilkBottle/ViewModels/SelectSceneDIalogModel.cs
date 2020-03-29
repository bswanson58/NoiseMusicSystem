using System;
using System.Collections.ObjectModel;
using System.Linq;
using MilkBottle.Entities;
using MilkBottle.Interfaces;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReusableBits.Mvvm.ViewModelSupport;

namespace MilkBottle.ViewModels {
    class SelectSceneDialogModel : PropertyChangeBase, IDialogAware {
        public  const string                        cSceneParameter = "scene";

        private readonly ISceneProvider             mSceneProvider;
        private readonly IPlatformLog               mLog;

        public  ObservableCollection<PresetScene>   Scenes { get; }
        public  PresetScene                         SelectedScene { get; set; }

        public  DelegateCommand                     Ok { get; }
        public  DelegateCommand                     Cancel { get; }

        public  string                              Title { get; }
        public  event Action<IDialogResult>         RequestClose;

        public SelectSceneDialogModel( ISceneProvider sceneProvider, IPlatformLog log ) {
            mSceneProvider = sceneProvider;
            mLog = log;

            Scenes = new ObservableCollection<PresetScene>();

            Ok = new DelegateCommand( OnOk );
            Cancel = new DelegateCommand( OnCancel );

            Title = "Select Scene";
        }

        public void OnDialogOpened( IDialogParameters parameters ) {
            LoadScenes();

            SelectedScene = Scenes.FirstOrDefault();

            RaisePropertyChanged( () => SelectedScene );
        }

        private void LoadScenes() {
            Scenes.Clear();

            mSceneProvider.SelectScenes( list => Scenes.AddRange( from s in list orderby s.Name select s ))
                .IfLeft( ex => mLog.LogException( "SelectScenes", ex ));
        }

        public void OnOk() {
            RaiseRequestClose(
                SelectedScene != null
                    ? new DialogResult( ButtonResult.OK, new DialogParameters { { cSceneParameter, SelectedScene } } )
                    : new DialogResult( ButtonResult.Cancel ));
        }

        public void OnCancel() {
            RaiseRequestClose( new DialogResult( ButtonResult.Cancel ));
        }

        private void RaiseRequestClose( IDialogResult dialogResult ) {
            RequestClose?.Invoke( dialogResult );
        }

        public bool CanCloseDialog() {
            return true;
        }

        public void OnDialogClosed() { }
    }
}
