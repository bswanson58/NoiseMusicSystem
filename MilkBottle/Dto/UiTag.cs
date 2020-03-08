using System;
using MilkBottle.Entities;
using ReusableBits.Mvvm.ViewModelSupport;

namespace MilkBottle.Dto {
    class UiTag : PropertyChangeBase {
        private readonly Action<UiTag>  mOnTagSelected;
        private bool            mIsSelected;

        public PresetTag        Tag { get; }

        public UiTag( PresetTag tag, Action<UiTag> onSelected ) {
            mOnTagSelected = onSelected;
            Tag = tag;
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
    }
}
