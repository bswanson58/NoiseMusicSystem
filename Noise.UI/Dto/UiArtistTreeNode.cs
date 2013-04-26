using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using Caliburn.Micro;
using Noise.Infrastructure;

namespace Noise.UI.Dto {
	public class UiArtistTreeNode : UiTreeNode {
		private readonly Action<UiArtistTreeNode>	mOnSelect;
		private readonly Action<UiArtistTreeNode>	mOnExpand;
		private readonly Action<UiArtistTreeNode>	mChildFillAction;
		private readonly UiTreeNode					mParent;
		private readonly List<SortDescription>		mSortDescriptions; 
		private	bool								mRequiresChildren;
		private bool								mImSorting;
		private CollectionViewSource				mChildrenView;
		private readonly BindableCollection<UiAlbumTreeNode>	mChildren;

		public	UiArtist				Artist { get; private set; }
		public	long					ParentCategoryId { get; private set; }

		public UiArtistTreeNode( UiArtist artist,
								 Action<UiArtistTreeNode> onSelect, Action<UiArtistTreeNode> onExpand, Action<UiArtistTreeNode> childFill,
								 ViewSortStrategy sortStrategy, IObservable<ViewSortStrategy> sortChanged ) :
			this( artist, null, Constants.cDatabaseNullOid, onSelect, onExpand, childFill, sortStrategy, sortChanged ) { }

		public UiArtistTreeNode( UiArtist artist, UiTreeNode parent, long parentCategoryId,
								 Action<UiArtistTreeNode> onSelect, Action<UiArtistTreeNode> onExpand, Action<UiArtistTreeNode> childFill,
								 ViewSortStrategy sortStrategy, IObservable<ViewSortStrategy> sortChanged ) {
			ParentCategoryId = parentCategoryId;
			Artist = artist;

			mParent = parent;
			mOnExpand = onExpand;
			mOnSelect = onSelect;
			mChildFillAction = childFill;
			mRequiresChildren = true;

			mChildren = new BindableCollection<UiAlbumTreeNode> { new UiAlbumTreeNode( new UiAlbum { Name = "Loading album list..." }, null, null )};
			mSortDescriptions = new List<SortDescription> { new SortDescription( "Album.Name", ListSortDirection.Ascending )};

			if( sortStrategy != null ) {
				SetSortDescriptions( sortStrategy.SortDescriptions );
			}
			if( sortChanged != null ) {
				sortChanged.Subscribe( OnSortChanged );
			}
		}

		public Collection<UiAlbumTreeNode> Children {
			get{ return( mChildren ); }
		}

		public ICollectionView ChildrenView {
			get {
				if( mChildrenView == null ) {
					mChildrenView = new CollectionViewSource { Source = mChildren };

					SetSortDescriptions( mSortDescriptions );
				}

				return( mChildrenView.View );
			}
		}

		public void SetChildren( IEnumerable<UiAlbumTreeNode> children ) {
			mChildren.IsNotifying = false;
			mChildren.Clear();
			mChildren.AddRange( children );
			mChildren.IsNotifying = true;
			mChildren.Refresh();

			mRequiresChildren = false;
		}

		private void OnSortChanged( ViewSortStrategy strategy ) {
			if( strategy != null ) {
				SetSortDescriptions( strategy.SortDescriptions );
			}
		}

		private void SetSortDescriptions( IEnumerable<SortDescription> sortDescriptions ) {
			if( sortDescriptions != null ) {
				if( mChildrenView != null ) {
					mChildrenView.SortDescriptions.Clear();

					foreach( var sortDescription in sortDescriptions ) {
						mChildrenView.SortDescriptions.Add( sortDescription );
					}
				}
				else {
					mSortDescriptions.Clear();
					mSortDescriptions.AddRange( sortDescriptions );
				}
			}
		}

		public void UpdateSort() {
			mImSorting = true;
			
			if( mChildrenView != null ) {
				Execute.OnUIThread( () => mChildrenView.View.Refresh());
			}

			mImSorting = false;
		}

		protected override void OnExpand() {
			if( mParent != null ) {
				mParent.IsExpanded = true;
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
