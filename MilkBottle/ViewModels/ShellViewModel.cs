using System.Windows;
using ReusableBits.Mvvm.ViewModelSupport;

namespace MilkBottle.ViewModels {
    public class ShellViewModel : PropertyChangeBase {
        private WindowState mCurrentWindowState;

        public  bool        DisplayStatus { get; private set; }

        public ShellViewModel() {
            DisplayStatus = true;
        }

        public WindowState CurrentWindowState {
            get => mCurrentWindowState;
            set {
                mCurrentWindowState = value;

                DisplayStatus = mCurrentWindowState != WindowState.Maximized;
                RaisePropertyChanged( () => DisplayStatus );
            }
        }   
    } 
}
