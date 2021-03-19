using System;
using Prism.Commands;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.Adapters {
	public class LinkNode : PropertyChangeBase {
        private readonly Action<long>	mLinkAction;

        public	string					DisplayText { get; }
		public	long					LinkId { get; }
		public	bool					IsLinked { get; }
		public	DelegateCommand			LinkClicked { get; }

		public LinkNode( string displayText ) {
			DisplayText = displayText;

			IsLinked = false;
			LinkClicked = new DelegateCommand( OnLinkClicked );
		}

		public LinkNode( string displayText, long linkId, Action<long> linkAction ) : 
            this( displayText ) {
			LinkId = linkId;
			mLinkAction = linkAction;

			IsLinked = true;
		}

		private void OnLinkClicked() {
            mLinkAction?.Invoke( LinkId );
        }
	}
}
