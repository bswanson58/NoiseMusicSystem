using System.Windows;
using Album4Matter.Dto;
using Caliburn.Micro;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Album4Matter.ViewModels {
    public class ShellViewModel :AutomaticCommandBase {
        private readonly IEventAggregator   mEventAggregator;

        public ShellViewModel( IEventAggregator eventAggregator ) {
            mEventAggregator = eventAggregator;
        }

        public void Execute_WindowStateChanged( WindowState state ) {
            mEventAggregator.PublishOnUIThread( new Events.WindowStateEvent( state ));
        }
    }
}
