using System;
using MilkBottle.Entities;
using Prism.Commands;

namespace MilkBottle.Dto {
    class UiPreset {
        private readonly Action<Preset> mOnDelete;
        private readonly Action<Preset> mOnEdit;

        public  Preset                  Preset { get; }
        public  string                  Name => Preset.Name;

        public  DelegateCommand         Delete { get; }
        public  DelegateCommand         Edit { get; }

        public UiPreset( Preset preset, Action<Preset> onEdit, Action<Preset> onDelete ) {
            mOnDelete = onDelete;
            mOnEdit = onEdit;

            Preset = preset;

            Delete = new DelegateCommand( OnDelete );
            Edit = new DelegateCommand( OnEdit );
        }

        private void OnDelete() {
            mOnDelete?.Invoke( Preset );
        }

        private void OnEdit() {
            mOnEdit?.Invoke( Preset );
        }
    }
}
