using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Support;
using Noise.UI.Adapters;

namespace Noise.UI.ViewModels {
	public class LibraryExplorerViewModel : ViewModelBase {
		private IUnityContainer			mContainer;
		private IEventAggregator		mEvents;
		private IExplorerViewStrategy	mViewStrategy;
		private ObservableCollection<ExplorerTreeNode>	mTreeItems;

		[Dependency]
		public IUnityContainer Container {
			get { return( mContainer ); }
			set {
				mContainer = value; 
				mViewStrategy = mContainer.Resolve<IExplorerViewStrategy>( "ArtistAlbum" );
				mViewStrategy.Initialize( this );

				mEvents = mContainer.Resolve<IEventAggregator>();
				mEvents.GetEvent<Events.ExplorerItemSelected>().Subscribe( OnExplorerItemSelected );
			}
		}

		public void OnExplorerItemSelected( object item ) {
			if( item is DbArtist ) {
				mEvents.GetEvent<Events.ArtistFocusRequested>().Publish( item as DbArtist );
			}
			else if( item is DbAlbum ) {
				mEvents.GetEvent<Events.AlbumFocusRequested>().Publish( item as DbAlbum );
			}
		}

		public IEnumerable<ExplorerTreeNode> TreeData {
			get {
				if(( mTreeItems == null ) &&
				   ( mViewStrategy != null )) {
					mTreeItems = new ObservableCollection<ExplorerTreeNode>();
					mViewStrategy.PopulateTree( mTreeItems );
				}

				return( mTreeItems );
			}
		}

		public DataTemplate TreeViewItemTemplate {
			get{ return( Get( () => TreeViewItemTemplate )); }
			set{ Set( () => TreeViewItemTemplate, value ); }
		}

		public string SearchText {
			get{ return( Get( () => SearchText )); }
			set {
				Set( () => SearchText, value );

				mViewStrategy.ClearCurrentSearch();
			}
		}

		public void Execute_Search() {
			mViewStrategy.Search( SearchText );
		}

		[DependsUpon( "SearchText" )]
		public bool CanExecute_Search() {
			return(!string.IsNullOrWhiteSpace( SearchText ));
		}
	}
}
