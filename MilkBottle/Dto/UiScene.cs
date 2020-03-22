using System;
using MilkBottle.Entities;
using Prism.Commands;
using ReusableBits.Mvvm.ViewModelSupport;

namespace MilkBottle.Dto {
    class UiScene : PropertyChangeBase {
        private readonly Action<UiScene>    mOnSceneEdit;
        private readonly Action<UiScene>    mOnSceneDelete;

        public PresetScene              Scene { get; }
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

        private void OnSceneDelete() {
            mOnSceneDelete?.Invoke( this );
        }

        private void OnSceneEdit() {
            mOnSceneEdit?.Invoke( this );
        }
    }
}
