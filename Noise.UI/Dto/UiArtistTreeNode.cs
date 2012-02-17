using System;
using System.Collections.Generic;
using System.ComponentModel;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.Dto {
	public class UiArtistTreeNode : UiTreeNode {
		private readonly Action<UiArtistTreeNode>	mOnSelect;
		private readonly Action<UiArtistTreeNode>	mOnExpand;
		private readonly Action<UiArtistTreeNode>	mChildFillAction;
		private	bool								mRequiresChildren;
		private bool								mImSorting;
		private readonly BindableCollection<UiAlbumTreeNode>	mChildren;
		private readonly BindableCollection<SortDescription>	mSortDescriptions; 

		public	UiArtist				Artist { get; private set; }
		public	UiDecadeTreeNode		Parent { get; private set; }

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

			mChildren = new BindableCollection<UiAlbumTreeNode> { new UiAlbumTreeNode( new UiAlbum { Name = "Loading album list..." }, null, null )};
			mSortDescriptions = new BindableCollection<SortDescription> { new SortDescription( "Album.Name", ListSortDirection.Ascending )};

			OnSortChanged( sortStrategy );
			if( sortChanged != null ) {
				sortChanged.Subscribe( OnSortChanged );
			}
		}

		public BindableCollection<UiAlbumTreeNode> Children {
			get{ return( mChildren ); }
		}

		public BindableCollection<SortDescription> SortDescriptions {
			get{ return( mSortDescriptions ); }
		} 

		public void SetChildren( IEnumerable<UiAlbumTreeNode> children ) {
			mChildren.Clear();
			mChildren.AddRange( children );

			mRequiresChildren = false;
		}

		private void OnSortChanged( ViewSortStrategy strategy ) {
			mSortDescriptions.Clear();
			mSortDescriptions.AddRange( strategy.SortDescriptions );
		}

		public void UpdateSort() {
			mImSorting = true;
			mSortDescriptions.Refresh();
			mImSorting = false;
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
			if(( mOnSelect != null ) &&
			   (!mImSorting )) {
				mOnSelect( this );
			}
		}

		public override string IndexString {
			get{ return( Artist != null ? Artist.SortName : "" ); }
		}
	}
}
