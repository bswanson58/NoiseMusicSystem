using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Adapters;
using CuttingEdge.Conditions;
using Observal;
using Observal.Extensions;
using Condition = CuttingEdge.Conditions.Condition;

namespace Noise.UI.ViewModels {
	public class ExplorerStrategyArtistAlbum : IExplorerViewStrategy {
		private readonly IUnityContainer		mContainer;
		private readonly IEventAggregator		mEventAggregator;
		private readonly INoiseManager			mNoiseManager;
		private readonly Observer				mChangeObserver;
		private	LibraryExplorerViewModel		mViewModel;
		private IEnumerator<ExplorerTreeNode>	mTreeEnumerator;

		public ExplorerStrategyArtistAlbum( IUnityContainer container ) {
			mContainer = container;
			mEventAggregator = mContainer.Resolve<IEventAggregator>();
			mNoiseManager = mContainer.Resolve<INoiseManager>();

			mChangeObserver = new Observer();
			mChangeObserver.Extend( new PropertyChangedExtension()).WhenPropertyChanges( node => OnNodeChanged( node.Source ));
		}

		private void OnNodeChanged( object source ) {
			var notifier = source as UserSettingsNotifier;

			if( notifier != null ) {
				mNoiseManager.DataProvider.UpdateItem( notifier.TargetItem );
			}
		}

		public void Initialize( LibraryExplorerViewModel viewModel ) {
			mViewModel = viewModel;

			mViewModel.TreeViewItemTemplate = Application.Current.TryFindResource( "ArtistAlbumTemplate" ) as HierarchicalDataTemplate;
		}

		public void PopulateTree( ObservableCollection<ExplorerTreeNode> tree ) {
			Condition.Requires( mViewModel ).IsNotNull();

			var artistList = from artist in mNoiseManager.DataProvider.GetArtistList() orderby artist.Name select artist;

			foreach( DbArtist artist in artistList ) {
				var parent = artist.AlbumCount > 0 ? new ExplorerTreeNode( mEventAggregator, artist, FillChildren ) : new ExplorerTreeNode( mEventAggregator, artist );

				tree.Add( parent );
				mChangeObserver.Add( parent.SettingsNotifier );
			}
		}

		private IEnumerable<ExplorerTreeNode> FillChildren( ExplorerTreeNode parent ) {
			IEnumerable<ExplorerTreeNode>	retValue = null;
			var artist = parent.Item as DbArtist;

			if( artist != null ) {
				retValue = from album in mNoiseManager.DataProvider.GetAlbumList( artist ) orderby album.Name select new ExplorerTreeNode( mEventAggregator, parent, album );
			}

			return( retValue );
		}

		public bool Search( string searchText ) {
			var retValue = false;

			if(( mTreeEnumerator != null ) &&
			   ( mTreeEnumerator.Current != null )) {
				mTreeEnumerator.Current.IsSelected = false;
			}

			if(( mTreeEnumerator == null ) ||
			   ( !mTreeEnumerator.MoveNext())) {
				mTreeEnumerator = FindMatches( searchText, mViewModel.TreeData ).GetEnumerator();
				mTreeEnumerator.MoveNext();
			}

			var node = mTreeEnumerator.Current;

			if( node != null ) {
				node.IsSelected = true;

				// Ensure that this person is in view.
				while( node.Parent != null ) {
					node = node.Parent;

					node.IsExpanded = true;
				}

				retValue = true;
			}

			return ( retValue );
		}

		static IEnumerable<ExplorerTreeNode> FindMatches( string searchText, IEnumerable<ExplorerTreeNode> list ) {
			return( from node in list
					let artist = node.Item as DbArtist
					where artist != null where artist.Name.Contains( searchText ) 
					select node );

/*			foreach( var node in list ) {
				var artist = node.Item as DbArtist;

				if( artist != null ) {
					if( artist.Name.Contains( searchText ) ) {
						yield return node;
					}
				}
			}
*/		}

		public void ClearCurrentSearch() {
			mTreeEnumerator = null;
		}
	}
}
