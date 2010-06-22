using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Practices.Composite.Events;
using Noise.Infrastructure;

namespace Noise.UI.Adapters {
	class ExplorerTreeNode : BindableObject {
		private readonly IEventAggregator	mEventAggregator;
		private bool						mIsSelected;
		private bool						mIsExpanded;

		public	ExplorerTreeNode			Parent { get; private set; }
		public	object						Item { get; private set; }
		public	ObservableCollection<ExplorerTreeNode>	Children { get; set; }

		public ExplorerTreeNode( IEventAggregator eventAggregator, object item ) :
			this( eventAggregator, item, null ) {
		}

		public ExplorerTreeNode( IEventAggregator eventAggregator, object item, IEnumerable<ExplorerTreeNode> children ) :
			this( eventAggregator, null, item, children ) {
		}

		public ExplorerTreeNode( IEventAggregator eventAggregator, ExplorerTreeNode parent, object item ) :
			this( eventAggregator, parent, item, null ) {
		}

		public ExplorerTreeNode( IEventAggregator eventAggregator, ExplorerTreeNode parent, object item, IEnumerable<ExplorerTreeNode> children ) {
			mEventAggregator = eventAggregator;
			Parent = parent;
			Item = item;

			if( children != null ) {
				Children = new ObservableCollection<ExplorerTreeNode>( children );
			}
		}

		public void SetChildren( IEnumerable<ExplorerTreeNode> children ) {
			Children = new ObservableCollection<ExplorerTreeNode>( children );
		}

		public bool IsSelected {
			get { return mIsSelected; }
			set {
				if( value != mIsSelected ) {
					mIsSelected = value;

					NotifyOfPropertyChange( () => IsSelected );

					if( mIsSelected ) {
						mEventAggregator.GetEvent<Events.ExplorerItemSelected>().Publish( Item );
					}
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
