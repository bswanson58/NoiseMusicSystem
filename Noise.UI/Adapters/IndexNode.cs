using Noise.UI.Dto;

namespace Noise.UI.Adapters {
	public class IndexNode {
		public	string		DisplayText { get; set; }
		private	readonly	UiArtistTreeNode	mNode;

		public IndexNode( string text, UiArtistTreeNode node ) {
			DisplayText = text;
			mNode = node;
		}

		public override string ToString() {
			return( DisplayText );
		}

		public void DisplayNode() {
			mNode.IsSelected = true;
		}
	}
}
