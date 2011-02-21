using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Data;
using Noise.Infrastructure.Support;

namespace Noise.UI.Dto {
	public class UiArtistTreeNode : UiTreeNode {
		private readonly Action<UiArtistTreeNode>	mOnSelect;
		private readonly Action<UiArtistTreeNode>	mOnExpand;
		private readonly Action<UiArtistTreeNode>	mChildFillAction;
		private	bool								mRequiresChildren;
		private readonly ObservableCollectionEx<UiAlbumTreeNode>	mChildren;

		public	UiArtist				Artist { get; private set; }
		public	UiDecadeTreeNode		Parent { get; private set; }
		public	CollectionViewSource	ChildrenView { get; private set; }

		public UiArtistTreeNode( UiArtist artist,
								 Action<UiArtistTreeNode> onSelect, Action<UiArtistTreeNode> onExpand, Action<UiArtistTreeNode> childFill,
								 ViewSortStrategy sortStrategy, IObservable<ViewSortStrategy> sortChanged ) :
			this( null, artist, onSelect, onExpand, childFill, sortStrategy, sortChanged ) { }

		public UiArtistTreeNode( UiDecadeTreeNode decadeNode, UiArtist artist,
								 Action<UiArtistTreeNode> onSelect, Action<UiArtistTreeNode> onExpand, Action<UiArtistTreeNode> childFill,
								 ViewSortStrategy sortStrategy, IObservable<ViewSortStrategy> sortChanged ) {
			Parent = decadeNode;
			Artist = artist;

			mOnExpand = onExpand;
			mOnSelect = onSelect;
			mChildFillAction = childFill;
			mRequiresChildren = true;

			mChildren = new ObservableCollectionEx<UiAlbumTreeNode>();
			mChildren.Add( new UiAlbumTreeNode( new UiAlbum { Name = "Loading album list..." }, null, null ));

			ChildrenView = new CollectionViewSource { Source = mChildren };
			ChildrenView.SortDescriptions.Add( new SortDescription( "Album.Name", ListSortDirection.Ascending ));

			OnSortChanged( sortStrategy );
			sortChanged.Subscribe( OnSortChanged );
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

		private void OnSortChanged( ViewSortStrategy strategy ) {
			ChildrenView.SortDescriptions.Clear();

			foreach( var sort in strategy.SortDescriptions ) {
				ChildrenView.SortDescriptions.Add( sort );
			}
		}

		protected override void OnExpand() {
			if( Parent != null ) {
				Parent.IsExpanded = true;
			}

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
