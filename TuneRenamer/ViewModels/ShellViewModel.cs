using System.Windows;
using Caliburn.Micro;
using Prism.Commands;
using ReusableBits.Mvvm.ViewModelSupport;
using TuneRenamer.Dto;

namespace TuneRenamer.ViewModels {
    public class ShellViewModel :AutomaticCommandBase {
        private readonly IEventAggregator       mEventAggregator;

        public  DelegateCommand<WindowState?>   WindowStateChanged { get; }

        public ShellViewModel( IEventAggregator eventAggregator ) {
            mEventAggregator = eventAggregator;

            WindowStateChanged = new DelegateCommand<WindowState?>( OnWindowStateChanged );
        }

        private void OnWindowStateChanged( WindowState? state ) {
            if( state != null ) {
                mEventAggregator.PublishOnUIThread( new Events.WindowStateEvent( state.Value ));
            }
        }
    }
}
