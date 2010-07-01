using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Practices.Composite.Events;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Support;

namespace Noise.UI.Adapters {
	class ExplorerTreeNode : ViewModelBase {
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

					RaisePropertyChanged( () => IsSelected );

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

					RaisePropertyChanged( () => IsExpanded );
				}

				// Expand all the way up to the root.
				if( mIsExpanded && Parent != null ) {
					Parent.IsExpanded = true;
				}
			}
		}

		public void Execute_PlayAlbum() {
			var album = Item as DbAlbum;

			if( album != null ) {
				mEventAggregator.GetEvent<Events.AlbumPlayRequested>().Publish( album );
			}
		}

		public bool CanExecute_PlayAlbum() {
			var retValue = false;
			var album = Item as DbAlbum;

			if(( album != null ) &&
			   ( album.TrackCount > 0 )) {
				retValue = true;
			}

			return( retValue );
		}
	}
}
