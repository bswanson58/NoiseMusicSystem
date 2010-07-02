using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Adapters;

namespace Noise.UI.ViewModels {
	public class LibraryExplorerViewModel : ViewModelBase {
		private IUnityContainer			mContainer;
		private IEventAggregator		mEventAggregator;
		private	INoiseManager			mNoiseManager;
		private IExplorerViewStrategy	mViewStrategy;

		private readonly ObservableCollection<ExplorerTreeNode>	mTreeItems;

		public LibraryExplorerViewModel() {
			mTreeItems = new ObservableCollection<ExplorerTreeNode>();

			mViewStrategy = new ExplorerStrategyArtistAlbum( this );
		}

		[Dependency]
		public IUnityContainer Container {
			get { return( mContainer ); }
			set {
				mContainer = value;

				mEventAggregator = mContainer.Resolve<IEventAggregator>();
				mNoiseManager = mContainer.Resolve<INoiseManager>();

				PopulateTreeData();
			}
		}

		private void PopulateTreeData() {
			mTreeItems.Clear();

			var artistList = from artist in mNoiseManager.DataProvider.GetArtistList() orderby artist.Name select artist;
			foreach( DbArtist artist in artistList ) {
				var parent = new ExplorerTreeNode( mEventAggregator, artist );
				parent.SetChildren( from album in mNoiseManager.DataProvider.GetAlbumList( artist ) orderby album.Name select new ExplorerTreeNode( mEventAggregator, parent, album ));
				mTreeItems.Add( parent );
			}
		}

		public IEnumerable<ExplorerTreeNode> TreeData {
			get{ return( mTreeItems ); }
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
