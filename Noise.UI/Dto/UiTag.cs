using System;
using System.Diagnostics;
using Noise.Infrastructure.Dto;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.Dto {
    [DebuggerDisplay("Artist = {" + nameof( Name ) + "}")]
    public class UiTag : AutomaticCommandBase {
        private readonly Action<UiTag>  mEditAction;

        public  DbTag       Tag { get; }
        public  string      Name => Tag.Name;
        public  bool        IsChecked { get; set; }

        public UiTag( DbTag tag, Action<UiTag> onEdit ) {
            Tag = tag;
            IsChecked = false;

            mEditAction = onEdit;
        }

        public void Execute_OnEdit() {
            mEditAction?.Invoke( this );
        }
    }
}
