using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Noise.UI.Adapters {
	class ExplorerTreeNode {
		public object Parent { get; private set; }
		private bool	mIsSelected;
		private bool	mIsExpanded;
		public ObservableCollection<object> Children { get; private set; }

		public ExplorerTreeNode( object parent, IEnumerable<object> children ) {
			Parent = parent;
			Children = new ObservableCollection<object>( children );
		}

		public bool IsSelected {
			get { return mIsSelected; }
			set {
				if( value != mIsSelected ) {
					mIsSelected = value;
					//					this.OnPropertyChanged( "IsSelected" );
				}
			}
		}

		public bool IsExpanded {
			get { return mIsExpanded; }
			set {
				if( value != mIsExpanded ) {
					mIsExpanded = value;
//					this.OnPropertyChanged( "IsExpanded" );
				}

				// Expand all the way up to the root.
//				if( mIsExpanded && _parent != null )
//					_parent.IsExpanded = true;
			}
		}
	}
}
