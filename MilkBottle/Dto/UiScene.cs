using System;
using MilkBottle.Entities;
using Prism.Commands;
using ReusableBits.Mvvm.ViewModelSupport;

namespace MilkBottle.Dto {
    class UiScene : PropertyChangeBase {
        private readonly Action<UiScene>    mOnSceneEdit;
        private readonly Action<UiScene>    mOnSceneDelete;

        public PresetScene              Scene { get; private set; }
        public  string                  Name => Scene.Name;

        public  DelegateCommand         Edit { get; }
        public  DelegateCommand         Delete { get; }

        public UiScene( PresetScene scene, Action<UiScene> onEdit, Action<UiScene> onDelete ) {
            Scene = scene;

            mOnSceneEdit = onEdit;
            mOnSceneDelete = onDelete;

            Delete = new DelegateCommand( OnSceneDelete );
            Edit = new DelegateCommand( OnSceneEdit );
        }

        public void UpdateScene( PresetScene newScene ) {
            Scene = newScene;
        }

        private void OnSceneDelete() {
            mOnSceneDelete?.Invoke( this );
        }

        private void OnSceneEdit() {
            mOnSceneEdit?.Invoke( this );
        }
    }
}
