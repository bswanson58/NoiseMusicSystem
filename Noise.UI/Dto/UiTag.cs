using System;
using System.Diagnostics;
using Noise.Infrastructure.Dto;
using Prism.Commands;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.Dto {
    [DebuggerDisplay("Tag = {" + nameof( Name ) + "}")]
    public class UiTag : PropertyChangeBase {
        private readonly Action<UiTag>  mEditAction;
        private readonly Action<UiTag>  mPlayAction;

        public  DbTag       Tag { get; }
        public  string      Name => Tag.Name;
        public  bool        IsChecked { get; set; }
        public  bool        IsPlaying { get; private set; }

        public  DelegateCommand Edit { get; }
        public  DelegateCommand Play { get; }

        private UiTag() {
            Edit = new DelegateCommand( OnEdit );
            Play = new DelegateCommand( OnPlay );
        }

        public UiTag( DbTag tag ) : this( tag, null, null ) { }

        public UiTag( DbTag tag, Action<UiTag> onEdit, Action<UiTag> onPlay ) :
            this () {
            Tag = tag;
            IsChecked = false;

            mEditAction = onEdit;
            mPlayAction = onPlay;
        }

        public void SetIsPlaying( bool isPlaying ) {
            IsPlaying = isPlaying;

            RaisePropertyChanged( () => IsPlaying );
        }

        private void OnEdit() {
            mEditAction?.Invoke( this );
        }

        private void OnPlay() {
            mPlayAction?.Invoke( this );
        }
    }
}
