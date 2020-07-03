using System;
using MilkBottle.Entities;
using Prism.Commands;
using ReusableBits.Mvvm.ViewModelSupport;

namespace MilkBottle.Dto {
    class UiTag : PropertyChangeBase {
        private readonly Action<UiTag>  mOnTagSelected;
        private readonly Action<UiTag>  mOnTagEdit;
        private readonly Action<UiTag>  mOnTagDelete;

        private bool                    mIsSelected;

        public  PresetTag               Tag { get; }
        public  string                  Name => Tag.Name;

        public  DelegateCommand         Edit { get; }
        public  DelegateCommand         Delete { get; }

        public UiTag( PresetTag tag, Action<UiTag> onSelected, Action<UiTag> onTagEdit, Action<UiTag> onTagDelete ) {
            mOnTagSelected = onSelected;
            mOnTagDelete = onTagDelete;
            mOnTagEdit = onTagEdit;
            Tag = tag;

            Delete = new DelegateCommand( OnTagDelete );
            Edit = new DelegateCommand( OnTagEdit );
        }

        public bool IsSelected {
            get => mIsSelected;
            set {
                mIsSelected = value;

                mOnTagSelected?.Invoke( this );
                RaisePropertyChanged( () => IsSelected );
            }
        }

        public void SetSelectedState( bool state ) {
            mIsSelected = state;

            RaisePropertyChanged( () => IsSelected );
        }

        private void OnTagDelete() {
            mOnTagDelete?.Invoke( this );
        }

        private void OnTagEdit() {
            mOnTagEdit?.Invoke( this );
        }
    }
}
