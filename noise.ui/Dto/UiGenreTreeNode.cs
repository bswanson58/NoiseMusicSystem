using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using Caliburn.Micro;
using Noise.Infrastructure.Dto;

namespace Noise.UI.Dto {
	public class UiGenreTreeNode : UiTreeNode {
		private readonly DbGenre						mGenre;
		private readonly Action<UiGenreTreeNode>		mSelectAction;
		private readonly Action<UiGenreTreeNode>		mExpandAction;
		private readonly Action<UiGenreTreeNode>		mChildFillAction;
		private readonly List<SortDescription>			mSortDescriptions; 
		private	CollectionViewSource					mChildrenView;
		private bool									mRequiresChildren;
		private readonly BindableCollection<UiArtistTreeNode>	mChildren;

		public UiGenreTreeNode( DbGenre genre, Action<UiGenreTreeNode> onSelect, Action<UiGenreTreeNode> onExpand, Action<UiGenreTreeNode> childFill,
											   ViewSortStrategy sortStrategy, IObservable<ViewSortStrategy> sortChanged ) {
			mGenre = genre;
			mSelectAction = onSelect;
			mExpandAction = onExpand;
			mChildFillAction = childFill;

			mSortDescriptions = new List<SortDescription>();
			mChildren = new BindableCollection<UiArtistTreeNode> {
					        new UiArtistTreeNode( new UiArtist { DisplayName = "Loading artist list..." }, null, null, null, null, null )};
			mRequiresChildren = true;

			if( sortStrategy != null ) {
				SetSortDescriptions( sortStrategy.SortDescriptions );
			}
			if( sortChanged != null ) {
				sortChanged.Subscribe( OnSortChanged );
			}
		}

		public DbGenre Genre {
			get{ return( mGenre ); }
		}

		public Collection<UiArtistTreeNode> Children {
			get{ return( mChildren ); }
		}

		public ICollectionView ChildrenView {
			get {
				if( mChildrenView == null ) {
					mChildrenView = new CollectionViewSource { Source = mChildren };
				}

				return( mChildrenView.View );
			} 
		}

		public void UpdateSort() {
			if( mChildrenView != null ) {
				mChildrenView.View.Refresh();
			}
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
			mChildren.IsNotifying = false;
			mChildren.Clear();
			mChildren.AddRange( children );
			mChildren.IsNotifying = true;
			mChildren.Refresh();

			mRequiresChildren = false;
		}

		protected override void Onselect() {
			if( mSelectAction != null ) {
				mSelectAction( this );
			}
		}

		public override string IndexString {
			get{ return( Genre != null ? Genre.Name : "" ); }
		}
	}
}
