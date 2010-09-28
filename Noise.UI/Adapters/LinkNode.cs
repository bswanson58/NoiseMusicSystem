using System;
using Noise.Infrastructure.Support;

namespace Noise.UI.Adapters {
	public class LinkNode : ViewModelBase {
		public	string					DisplayText { get; private set; }
		public	long					LinkId { get; private set; }
		public	bool					IsLinked { get; private set; }
		private readonly Action<long>	mLinkAction;

		public LinkNode( string displayText ) {
			DisplayText = displayText;

			IsLinked = false;
		}

		public LinkNode( string displayText, long linkId, Action<long> linkAction ) {
			DisplayText = displayText;
			LinkId = linkId;
			mLinkAction = linkAction;

			IsLinked = true;
		}

		public void Execute_LinkClicked() {
			if( mLinkAction != null ) {
				mLinkAction( LinkId );
			}
		}
	}
}
