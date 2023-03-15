using System;
using Prism.Commands;
using Prism.Services.Dialogs;

namespace ReusableBits.Mvvm.ViewModelSupport {
    public class DialogAwareBase : PropertyChangeBase, IDialogAware {
        public  string                          Title { get; protected set; }
        public  event Action<IDialogResult>     RequestClose = delegate {  };

        public  DelegateCommand                 Ok { get; }
        public  DelegateCommand                 Cancel { get; }

        protected DialogAwareBase() {
            Title = "Dialog";

            Ok = new DelegateCommand( OnOk, CanAccept );
            Cancel = new DelegateCommand( OnCancel );
        }
        
        public virtual void OnDialogOpened( IDialogParameters parameters ) {
        }

        protected virtual DialogParameters CreateClosingParameters() => new DialogParameters();

        protected virtual void OnOk() {
            RaiseRequestClose( new DialogResult( ButtonResult.OK, CreateClosingParameters()));
        }

        protected virtual bool CanAccept() => true;

        protected virtual void OnCancel() {
            RaiseRequestClose( new DialogResult( ButtonResult.Cancel ));
        }

        public virtual bool CanCloseDialog() => true;
        public void OnDialogClosed() { }

        private void RaiseRequestClose( IDialogResult dialogResult ) {
            RequestClose?.Invoke( dialogResult );
        }
    }
}
