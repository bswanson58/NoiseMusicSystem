using System;

namespace Noise.UI.Dto {
	public class UiAlbumTreeNode : UiTreeNode {
		private readonly Action<UiAlbumTreeNode>	mOnSelect;
		private readonly Action<UiAlbumTreeNode>	mOnPlay;

		public	UiAlbum		Album { get; private set; }

		public UiAlbumTreeNode( UiAlbum album, Action<UiAlbumTreeNode> onSelect, Action<UiAlbumTreeNode> onPlay ) {
			Album = album;

			mOnSelect = onSelect;
			mOnPlay = onPlay;
		}

		protected override void Onselect() {
			if( mOnSelect != null ) {
				mOnSelect( this );
			}
		}
		public void Execute_PlayAlbum() {
			if( mOnPlay != null ) {
				mOnPlay( this );
			}
		}
	}
}
