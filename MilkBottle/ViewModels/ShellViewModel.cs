using System.Windows;
using Caliburn.Micro;
using ReusableBits.Mvvm.ViewModelSupport;

namespace MilkBottle.ViewModels {
    public class ShellViewModel : PropertyChangeBase {
        private readonly IEventAggregator   mEventAggregator;
        private WindowState                 mCurrentWindowState;

        public  bool        DisplayStatus { get; private set; }

        public ShellViewModel( IEventAggregator eventAggregator ) {
            mEventAggregator = eventAggregator;

            DisplayStatus = true;
        }

        public WindowState CurrentWindowState {
            get => mCurrentWindowState;
            set {
                mCurrentWindowState = value;

                DisplayStatus = mCurrentWindowState != WindowState.Maximized;
                RaisePropertyChanged( () => DisplayStatus );

                mEventAggregator.PublishOnUIThread( new Events.WindowStateChanged( mCurrentWindowState ));
            }
        }   
    } 
}
