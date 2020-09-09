using System;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.Desktop.ViewModels {
    class ExitApplicationDialogViewModel : PropertyChangeBase, IDialogAware {
        public const string     cReasonParameter = "reason";

        public  DelegateCommand             Ok { get; }
        public  DelegateCommand             Cancel { get; }

        public  string                      Title { get; }
        public  string                      ExitReason { get; private set; }

        public  event Action<IDialogResult> RequestClose;

        public ExitApplicationDialogViewModel() {
            Ok = new DelegateCommand( OnOk );
            Cancel = new DelegateCommand( OnCancel );

            Title = "Exit Application";
        }

        public bool CanCloseDialog() {
            return true;
        }

        public void OnDialogClosed() { }

        public void OnDialogOpened( IDialogParameters parameters ) {
            ExitReason = parameters.GetValue<string>( cReasonParameter );

            RaisePropertyChanged( () => ExitReason );
        }

        public void OnOk() {
            RaiseRequestClose( new DialogResult( ButtonResult.OK, new DialogParameters()));
        }

        public void OnCancel() {
            RaiseRequestClose( new DialogResult( ButtonResult.Cancel ));
        }

        private void RaiseRequestClose( IDialogResult dialogResult ) {
            RequestClose?.Invoke( dialogResult );
        }
    }
}
