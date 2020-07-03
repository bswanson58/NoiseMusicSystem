using System;
using MilkBottle.Entities;
using Prism.Commands;
using ReusableBits.Mvvm.ViewModelSupport;

namespace MilkBottle.Dto {
    class UiMood : PropertyChangeBase {
        private readonly Action<UiMood> mOnDelete;
        private readonly Action<UiMood> mOnEdit;
        private readonly Action<UiMood> mOnChecked;
        private bool                    mChecked;

        public  Mood                    Mood { get; private set; }
        public  string                  Name => Mood.Name;

        public  DelegateCommand         Edit { get; }
        public  DelegateCommand         Delete {  get; }

        public UiMood( Mood mood, Action<UiMood> onChecked, Action<UiMood> onEdit, Action<UiMood> onDelete ) {
            Mood = mood;
            mOnDelete = onDelete;
            mOnEdit = onEdit;
            mOnChecked = onChecked;

            Delete = new DelegateCommand( OnDelete );
            Edit = new DelegateCommand( OnEdit );
        }

        public void SetMood( Mood mood ) {
            Mood = mood;
        }

        public void SetChecked( bool state ) {
            mChecked = state;

            RaisePropertyChanged( () => IsChecked );
        }

        public bool IsChecked {
            get => mChecked;
            set {
                mChecked = value;

                RaisePropertyChanged( () => IsChecked );
                mOnChecked?.Invoke( this );
            }
        }
        private void OnEdit() {
            mOnEdit?.Invoke( this );
        }

        private void OnDelete() {
            mOnDelete?.Invoke( this );
        }
    }
}
