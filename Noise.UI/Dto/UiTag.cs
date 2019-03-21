using System;
using System.Diagnostics;
using Noise.Infrastructure.Dto;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.Dto {
    [DebuggerDisplay("Artist = {" + nameof( Name ) + "}")]
    public class UiTag : AutomaticCommandBase {
        private readonly Action<UiTag>  mEditAction;
        private readonly Action<UiTag>  mPlayAction;

        public  DbTag       Tag { get; }
        public  string      Name => Tag.Name;
        public  bool        IsChecked { get; set; }

        public UiTag( DbTag tag ) : this( tag, null, null ) { }

        public UiTag( DbTag tag, Action<UiTag> onEdit, Action<UiTag> onPlay ) {
            Tag = tag;
            IsChecked = false;

            mEditAction = onEdit;
            mPlayAction = onPlay;
        }

        public void Execute_Edit() {
            mEditAction?.Invoke( this );
        }

        public void Execute_Play() {
            mPlayAction?.Invoke( this );
        }
    }
}
