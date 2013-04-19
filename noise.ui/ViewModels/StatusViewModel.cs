using Caliburn.Micro;
using Noise.Infrastructure;
using ReusableBits.Mvvm.ViewModelSupport;
using ReusableBits.Ui.Controls;

namespace Noise.UI.ViewModels {
	public class StatusViewModel : AutomaticPropertyBase, IHandle<Events.StatusEvent> {
		private readonly IEventAggregator	mEventAggregator;

		public	string						Version { get; private set; }

		public StatusViewModel( IEventAggregator eventAggregator ) {
			mEventAggregator = eventAggregator;

			Version = "Desktop v0.9.0";

			mEventAggregator.Subscribe( this );
		}

		public StatusMessage StatusMessage {
			get{ return( Get( () => StatusMessage )); }
			set{ Set( () => StatusMessage, value ); }
		}

		public void Handle( Events.StatusEvent status ) {
			StatusMessage = new StatusMessage( status.Message ) { ExtendActiveDisplay = status.ExtendDisplay };
		}
	}
}
