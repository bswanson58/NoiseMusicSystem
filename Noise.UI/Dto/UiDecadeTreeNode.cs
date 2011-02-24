using System;
using System.Collections.Generic;
using System.Windows.Data;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Support;
using Noise.UI.Adapters;

namespace Noise.UI.Dto {
	public class UiDecadeTreeNode : UiTreeNode {
		private readonly DbDecadeTag					mDecadeTag;
		private readonly Action<UiDecadeTreeNode>		mSelectAction;
		private readonly Action<UiDecadeTreeNode>		mExpandAction;
		private readonly Action<UiDecadeTreeNode>		mChildFillAction;
		private readonly Action<UiDecadeTreeNode>		mLinkAction;
		private bool									mRequiresChildren;
		private readonly ObservableCollectionEx<UiArtistTreeNode>	mChildren;

		public	CollectionViewSource					ChildrenView { get; private set; }
		public	LinkNode								DecadeWebsite { get; private set; }

		public UiDecadeTreeNode( DbDecadeTag tag, Action<UiDecadeTreeNode> onSelect, Action<UiDecadeTreeNode> onExpand,
												  Action<UiDecadeTreeNode> childFill, Action<UiDecadeTreeNode> linkAction,
												  ViewSortStrategy sortStrategy, IObservable<ViewSortStrategy> sortChanged ) {
			mDecadeTag = tag;
			mSelectAction = onSelect;
			mExpandAction = onExpand;
			mLinkAction = linkAction;
			mChildFillAction = childFill;

			mChildren = new ObservableCollectionEx<UiArtistTreeNode>();
			ChildrenView = new CollectionViewSource { Source = mChildren };
			OnSortChanged( sortStrategy );
			if( sortChanged != null ) {
				sortChanged.Subscribe( OnSortChanged );
			}
			mChildren.Add( new UiArtistTreeNode( null, new UiArtist { DisplayName = "Loading artist list..." }, null, null, null, null, null ));
			mRequiresChildren = true;

			if(!string.IsNullOrWhiteSpace( tag.Website )) {
				DecadeWebsite = new LinkNode( "Decade Info", 0, OnLinkClick );
			}
		}

		public void UpdateSort() {
			ChildrenView.View.Refresh();
		}

		private void OnSortChanged( ViewSortStrategy strategy ) {
			ChildrenView.SortDescriptions.Clear();

			if( strategy != null ) {
				foreach( var sort in strategy.SortDescriptions ) {
					ChildrenView.SortDescriptions.Add( sort );
				}
			}
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

		public bool RequiresChildren {
			get{ return( mRequiresChildren ); }
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

		private void OnLinkClick( long item ) {
			if( mLinkAction != null ) {
				mLinkAction( this );
			}
		}

		public DbDecadeTag Tag {
			get{ return( mDecadeTag ); }
		}

		public ObservableCollectionEx<UiArtistTreeNode> Children {
			get{ return( mChildren ); }
		}

		public override string IndexString {
			get{ return( Tag != null ? Tag.Name : "" ); }
		}
	}
}
