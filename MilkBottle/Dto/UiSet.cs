using System;
using MilkBottle.Entities;
using Prism.Commands;

namespace MilkBottle.Dto {
    class UiSet {
        private readonly Action<UiSet>  mOnSetEdit;
        private readonly Action<UiSet>  mOnSetDelete;

        public  PresetSet   Set { get; }
        public  string      Name => Set.Name;

        public  DelegateCommand         Edit { get; }
        public  DelegateCommand         Delete { get; }

        public UiSet( PresetSet set, Action<UiSet> onSetEdit, Action<UiSet> onSetDelete ) {
            mOnSetDelete = onSetDelete;
            mOnSetEdit = onSetEdit;

            Set = set;

            Delete = new DelegateCommand( OnSetDelete );
            Edit = new DelegateCommand( OnSetEdit );
        }

        private void OnSetDelete() {
            mOnSetDelete?.Invoke( this );
        }

        private void OnSetEdit() {
            mOnSetEdit?.Invoke( this );
        }
    }
}
