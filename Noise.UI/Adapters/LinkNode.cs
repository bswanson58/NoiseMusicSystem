using Microsoft.Practices.Composite.Events;
using Noise.Infrastructure.Support;

namespace Noise.UI.Adapters {
	public class LinkNode : ViewModelBase {
		private readonly IEventAggregator	mEvents;

		public	string		DisplayText { get; private set; }
		public	string		LinkText { get; private set; }
		public	bool		IsLinked { get; private set; }

		public LinkNode( string displayText ) {
			DisplayText = displayText;

			IsLinked = false;
		}

		public LinkNode( IEventAggregator eventAggregator, string displayText, string linkText ) {
			mEvents = eventAggregator;

			DisplayText = displayText;
			LinkText = linkText;

			IsLinked = true;
		}

		public void Execute_LinkClicked() {
			if( mEvents != null ) {
				
			}
		}
	}
}
