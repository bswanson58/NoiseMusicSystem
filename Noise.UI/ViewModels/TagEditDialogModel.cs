using System;
using Noise.UI.Dto;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
    public class TagEditDialogModel : PropertyChangeBase, IDialogAware {
        public  const string                cTagParameter = "tag";
        public  const string                cDeleteRequested = "deleteRequested";

        private bool                        IsValid => mTag?.Tag != null && !string.IsNullOrWhiteSpace( TagName );
        private UiTag                       mTag;

        public  string                      TagName {  get; set; }
        public  string                      Title { get; }
        public  event Action<IDialogResult> RequestClose;

        public  DelegateCommand             Ok { get; }
        public  DelegateCommand             Cancel { get; }
        public  DelegateCommand             DeleteTag { get; }

        public TagEditDialogModel() {
            Ok = new DelegateCommand( OnOk );
            Cancel = new DelegateCommand( OnCancel );
            DeleteTag = new DelegateCommand( OnDeleteTag );

            Title = "Tag Properties";
        }

        public void OnDialogOpened( IDialogParameters parameters ) {
            mTag = parameters.GetValue<UiTag>( cTagParameter );

            if( mTag != null ) {
                TagName = mTag.Name;

                RaisePropertyChanged( () => TagName );
                RaisePropertyChanged( () => IsValid );
            }
        }

        private void OnDeleteTag() {
            var parameters = new DialogParameters{{ cDeleteRequested, true }, { cTagParameter, mTag }};

            RaiseRequestClose( new DialogResult( ButtonResult.OK, parameters ));
        }

        public bool CanCloseDialog() {
            return true;
        }

        public void OnDialogClosed() { }

        public void OnOk() {
            mTag.Tag.Name = TagName;

            var parameters = new DialogParameters{{ cDeleteRequested, false }, { cTagParameter, mTag }};

            RaiseRequestClose( new DialogResult( IsValid ? ButtonResult.OK : ButtonResult.Cancel, parameters ));
        }

        public void OnCancel() {
            RaiseRequestClose( new DialogResult( ButtonResult.Cancel ));
        }

        private void RaiseRequestClose( IDialogResult dialogResult ) {
            RequestClose?.Invoke( dialogResult );
        }
    }
}
