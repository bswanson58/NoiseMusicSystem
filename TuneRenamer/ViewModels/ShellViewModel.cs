using System.Windows;
using Caliburn.Micro;
using ReusableBits.Mvvm.ViewModelSupport;
using TuneRenamer.Dto;

namespace TuneRenamer.ViewModels {
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
