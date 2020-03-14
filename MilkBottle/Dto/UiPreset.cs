using System;
using System.Linq;
using MilkBottle.Entities;
using MilkBottle.Types;
using Prism.Commands;

namespace MilkBottle.Dto {
    class UiPreset {
        private readonly Action<Preset> mOnDelete;
        private readonly Action<Preset> mOnEdit;

        public  Preset                  Preset { get; }
        public  string                  Name => Preset.Name;

        public  bool                    IsFavorite => Preset.IsFavorite;
        public  bool                    DoNotPlay => Preset.Rating == PresetRating.DoNotPlayValue;
        public  bool                    HasTags => Preset.Tags.Any();

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
