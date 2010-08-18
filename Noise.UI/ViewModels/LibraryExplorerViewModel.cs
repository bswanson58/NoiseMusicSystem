using System.Collections.Generic;
using System.Windows;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Support;
using Noise.UI.Adapters;
using Noise.UI.Behaviours.EventCommandTriggers;
using Noise.UI.Controls;

namespace Noise.UI.ViewModels {
	public class LibraryExplorerViewModel : ViewModelBase {
		private IUnityContainer			mContainer;
		private IEventAggregator		mEvents;
		private IExplorerViewStrategy	mViewStrategy;
		private ObservableCollectionEx<ExplorerTreeNode>	mTreeItems;
		private List<string>			mSearchOptions;

		[Dependency]
		public IUnityContainer Container {
			get { return( mContainer ); }
			set {
				mContainer = value; 
				mSearchOptions = new List<string>();

				mViewStrategy = mContainer.Resolve<IExplorerViewStrategy>( "ArtistAlbum" );
				mViewStrategy.Initialize( this );

				mEvents = mContainer.Resolve<IEventAggregator>();
				mEvents.GetEvent<Events.ExplorerItemSelected>().Subscribe( OnExplorerItemSelected );
				mEvents.GetEvent<Events.DatabaseChanged>().Subscribe( OnDatabaseUpdate );
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

		public void OnDatabaseUpdate( DatabaseChangeSummary summary ) {
			if( summary.ArtistChanges || summary.AlbumChanges ) {
				if(( mViewStrategy != null ) &&
				   ( mTreeItems != null )) {
					Execute.OnUiThread( UpdateTree );
				}
			}
		}

		private void UpdateTree() {
			mTreeItems.SuspendNotification();
			mTreeItems.Clear();
			mViewStrategy.PopulateTree( mTreeItems );
			mTreeItems.ResumeNotification();
		}

		public IEnumerable<ExplorerTreeNode> TreeData {
			get {
				if(( mTreeItems == null ) &&
				   ( mViewStrategy != null )) {
					mTreeItems = new ObservableCollectionEx<ExplorerTreeNode>();

					UpdateTree();
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

		public void Execute_Search( EventCommandParameter<object, RoutedEventArgs> args ) {
			var searchArgs = args.EventArgs as SearchEventArgs;

			if( searchArgs != null ) {
				mViewStrategy.Search( searchArgs.Keyword, searchArgs.Sections );
			}
		}

		[DependsUpon( "SearchText" )]
		public bool CanExecute_Search() {
			return(!string.IsNullOrWhiteSpace( SearchText ));
		}

		public List<string> SearchOptions {
			get{ return( mSearchOptions ); }
		}

		public bool HaveSearchOptions {
			get{ return(( mSearchOptions != null ) && ( mSearchOptions.Count > 0 )); }
		}
	}
}
