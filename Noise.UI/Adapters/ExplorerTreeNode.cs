using System.Collections.Generic;
using System.Collections.ObjectModel;
using Noise.Infrastructure;

namespace Noise.UI.Adapters {
	class ExplorerTreeNode : BindableObject {
		public ExplorerTreeNode				Parent { get; private set; }
		public ObservableCollection<object> Children { get; private set; }
		public	object						Item { get; private set; }
		private bool						mIsSelected;
		private bool						mIsExpanded;

		public ExplorerTreeNode( object item, IEnumerable<object> children ) :
			this( null, item, children ) {
		}

		public ExplorerTreeNode( ExplorerTreeNode parent, object item, IEnumerable<object> children ) {
			Parent = parent;
			Item = item;
			Children = new ObservableCollection<object>( children );
		}

		public bool IsSelected {
			get { return mIsSelected; }
			set {
				if( value != mIsSelected ) {
					mIsSelected = value;

					NotifyOfPropertyChange( () => IsSelected );
				}
			}
		}

		public bool IsExpanded {
			get { return mIsExpanded; }
			set {
				if( value != mIsExpanded ) {
					mIsExpanded = value;

					NotifyOfPropertyChange( () => IsExpanded );
				}

				// Expand all the way up to the root.
				if( mIsExpanded && Parent != null ) {
					Parent.IsExpanded = true;
				}
			}
		}
	}
}
