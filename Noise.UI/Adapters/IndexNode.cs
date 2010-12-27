namespace Noise.UI.Adapters {
	public class IndexNode {
		public	string		DisplayText { get; set; }
		private	readonly	ArtistTreeNode	mNode;

		public IndexNode( string text, ArtistTreeNode node ) {
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
