using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Practices.Composite.Events;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Support;

namespace Noise.UI.Adapters {
	public class ExplorerTreeNode : ViewModelBase {
		private readonly IEventAggregator	mEventAggregator;
		private readonly Func<ExplorerTreeNode,IEnumerable<ExplorerTreeNode>>	mChildFillFunction;

		public	ExplorerTreeNode			Parent { get; private set; }
		public	object						Item { get; private set; }
		public	bool						RequiresChildren{ get; private set; }
		public	UserSettingsNotifier		SettingsNotifier { get; private set; }
		public	ObservableCollection<ExplorerTreeNode>	Children { get; set; }

		public ExplorerTreeNode( IEventAggregator eventAggregator, object item ) :
			this( eventAggregator, null, item, null ) {
		}

		public ExplorerTreeNode( IEventAggregator eventAggregator, object item, IEnumerable<ExplorerTreeNode> children ) :
			this( eventAggregator, null, item, children ) {
		}

		public ExplorerTreeNode( IEventAggregator eventAggregator, ExplorerTreeNode parent, object item ) :
			this( eventAggregator, parent, item, null ) {
		}

		public ExplorerTreeNode( IEventAggregator eventAggregator, object item, Func<ExplorerTreeNode,IEnumerable<ExplorerTreeNode>> fillFunc ) :
			this( eventAggregator, null, item, null ) {
			var dummyChild = new DbArtist { Name = "Albums are being loaded" };
			var dummyNode = new ExplorerTreeNode( eventAggregator, this, dummyChild );

			mChildFillFunction = fillFunc;
			Children = new ObservableCollection<ExplorerTreeNode> { dummyNode };
			RequiresChildren = true;
		}

		public ExplorerTreeNode( IEventAggregator eventAggregator, ExplorerTreeNode parent, object item, IEnumerable<ExplorerTreeNode> children ) {
			mEventAggregator = eventAggregator;
			Parent = parent;
			Item = item;

			SettingsNotifier = new UserSettingsNotifier( Item as IUserSettings );

			if( children != null ) {
				Children = new ObservableCollection<ExplorerTreeNode>( children );
			}
		}

		public void SetItem( object item ) {
			Item = item;

			RaisePropertyChanged( () => Item );
		}

		public void SetChildren( IEnumerable<ExplorerTreeNode> children ) {
			Children = new ObservableCollection<ExplorerTreeNode>( children );

			RequiresChildren = false;
			RaisePropertyChanged( () => Children );
		}

		public bool IsSelected {
			get { return( Get( () => IsSelected )); }
			set {
				Set( () => IsSelected, value  );

				if( value ) {
					mEventAggregator.GetEvent<Events.ExplorerItemSelected>().Publish( Item );
				}
			}
		}

		public bool IsExpanded {
			get { return( Get( () => IsExpanded )); }
			set { 
				Set( () => IsExpanded, value  );

				// Expand all the way up to the root.
				if( value && Parent != null ) {
					Parent.IsExpanded = true;
				}

				if(( value ) &&
				   ( RequiresChildren ) &&
				   ( mChildFillFunction != null )) {
					SetChildren( mChildFillFunction( this ));
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

		public void Execute_Rename() {
			var artist = Item as DbArtist;

			if( artist != null ) {
				
			}
		}
		public bool CanExecute_Rename() {
			var retValue = false;
			var artist = Item as DbArtist;

			if( artist != null ) {
				retValue = true;
			}

			return( retValue );
		}
	}
}
