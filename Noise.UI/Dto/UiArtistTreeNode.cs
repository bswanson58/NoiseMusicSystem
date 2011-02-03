﻿using System;
using System.Collections.Generic;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Support;

namespace Noise.UI.Dto {
	public class UiArtistTreeNode : UiTreeNode {
		private readonly Action<UiArtistTreeNode>	mOnSelect;
		private readonly Action<UiArtistTreeNode>	mOnExpand;
		private readonly Action<UiArtistTreeNode>	mChildFillAction;
		private	bool								mRequiresChildren;
		private readonly ObservableCollectionEx<UiAlbumTreeNode>	mChildren;

		public	UiArtist		Artist { get; private set; }
		public	DbDecadeTag		DecadeTag { get; private set; }

		public UiArtistTreeNode( DbDecadeTag tag, UiArtist artist,
								 Action<UiArtistTreeNode> onSelect, Action<UiArtistTreeNode> onExpand, Action<UiArtistTreeNode> childFill ) {
			DecadeTag = tag;
			Artist = artist;

			mOnExpand = onExpand;
			mOnSelect = onSelect;
			mChildFillAction = childFill;
			mRequiresChildren = true;

			mChildren = new ObservableCollectionEx<UiAlbumTreeNode>();
			mChildren.Add( new UiAlbumTreeNode( new UiAlbum( null, null ) { Name = "Loading album list..." }, null, null ));
		}

		public ObservableCollectionEx<UiAlbumTreeNode> Children {
			get{ return( mChildren ); }
		}

		public void SetChildren( IEnumerable<UiAlbumTreeNode> children ) {
			mChildren.SuspendNotification();
			mChildren.Clear();
			mChildren.AddRange( children );
			mChildren.ResumeNotification();

			mRequiresChildren = false;
		}

		protected override void OnExpand() {
			if( mOnExpand != null ) {
				mOnExpand( this );
			}

			if(( IsExpanded ) &&
			   ( mChildFillAction != null )) {
				if( mRequiresChildren ) {
					mChildFillAction( this );
				}
			}
		}

		protected override void Onselect() {
			if( mOnSelect != null ) {
				mOnSelect( this );
			}
		}
	}
}
