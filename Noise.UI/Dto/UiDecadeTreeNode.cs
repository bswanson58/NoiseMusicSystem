using System;
using System.Collections.Generic;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Support;

namespace Noise.UI.Dto {
	public class UiDecadeTreeNode : UiTreeNode {
		private readonly DbDecadeTag					mDecadeTag;
		private readonly Action<UiDecadeTreeNode>		mSelectAction;
		private readonly Action<UiDecadeTreeNode>		mExpandAction;
		private readonly Action<UiDecadeTreeNode>		mChildFillAction;
		private bool									mRequiresChildren;

		private readonly ObservableCollectionEx<UiArtistTreeNode>	mChildren;

		public UiDecadeTreeNode( DbDecadeTag tag, Action<UiDecadeTreeNode> onSelect, Action<UiDecadeTreeNode> onExpand, Action<UiDecadeTreeNode> childFill ) {
			mDecadeTag = tag;
			mSelectAction = onSelect;
			mExpandAction = onExpand;
			mChildFillAction = childFill;

			mChildren = new ObservableCollectionEx<UiArtistTreeNode>();
			mChildren.Add( new UiArtistTreeNode( null, new UiArtist( null ) { DisplayName = "Loading artist list..." }, null, null, null ));
			mRequiresChildren = true;
		}

		protected override void OnExpand() {
			if( mExpandAction != null ) {
				mExpandAction( this );
			}

			if(( IsExpanded ) &&
			   ( mChildFillAction != null )) {
				if( mRequiresChildren ) {
					mChildFillAction( this );
				}
			}
		}

		public void SetChildren( IEnumerable<UiArtistTreeNode> children ) {
			mChildren.SuspendNotification();
			mChildren.Clear();
			mChildren.AddRange( children );
			mChildren.ResumeNotification();

			mRequiresChildren = false;
		}

		protected override void Onselect() {
			if( mSelectAction != null ) {
				mSelectAction( this );
			}
		}

		public DbDecadeTag Tag {
			get{ return( mDecadeTag ); }
		}

		public ObservableCollectionEx<UiArtistTreeNode> Children {
			get{ return( mChildren ); }
		}
	}
}
