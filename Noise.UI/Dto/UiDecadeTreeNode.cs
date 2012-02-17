using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using Noise.Infrastructure.Dto;
using Noise.UI.Adapters;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.Dto {
	public class UiDecadeTreeNode : UiTreeNode {
		private readonly DbDecadeTag					mDecadeTag;
		private readonly Action<UiDecadeTreeNode>		mSelectAction;
		private readonly Action<UiDecadeTreeNode>		mExpandAction;
		private readonly Action<UiDecadeTreeNode>		mChildFillAction;
		private readonly Action<UiDecadeTreeNode>		mLinkAction;
		private readonly List<SortDescription>			mSortDescriptions; 
		private	CollectionViewSource					mChildrenView;
		private bool									mRequiresChildren;
		private readonly BindableCollection<UiArtistTreeNode>	mChildren;

		public	LinkNode								DecadeWebsite { get; private set; }

		public UiDecadeTreeNode( DbDecadeTag tag, Action<UiDecadeTreeNode> onSelect, Action<UiDecadeTreeNode> onExpand,
												  Action<UiDecadeTreeNode> childFill, Action<UiDecadeTreeNode> linkAction,
												  ViewSortStrategy sortStrategy, IObservable<ViewSortStrategy> sortChanged ) {
			mDecadeTag = tag;
			mSelectAction = onSelect;
			mExpandAction = onExpand;
			mLinkAction = linkAction;
			mChildFillAction = childFill;

			mSortDescriptions = new List<SortDescription>();
			mChildren = new BindableCollection<UiArtistTreeNode> {
					        new UiArtistTreeNode( null, new UiArtist { DisplayName = "Loading artist list..." }, null, null, null, null, null )};
			mRequiresChildren = true;

			if( sortStrategy != null ) {
				SetSortDescriptions( sortStrategy.SortDescriptions );
			}
			if( sortChanged != null ) {
				sortChanged.Subscribe( OnSortChanged );
			}

			DecadeWebsite = !string.IsNullOrWhiteSpace( tag.Website ) ? new LinkNode( "Decade Info", 0, OnLinkClick ) :
																		new LinkNode( string.Empty );
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
			mChildrenView.View.Refresh();
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
			mChildren.Clear();
			mChildren.AddRange( children );

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

		public override string IndexString {
			get{ return( Tag != null ? Tag.Name : "" ); }
		}
	}
}
