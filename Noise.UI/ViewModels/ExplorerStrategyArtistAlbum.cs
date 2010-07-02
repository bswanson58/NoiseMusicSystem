using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Noise.Infrastructure.Dto;
using Noise.UI.Adapters;

namespace Noise.UI.ViewModels {
	internal class ExplorerStrategyArtistAlbum : IExplorerViewStrategy {
		private readonly LibraryExplorerViewModel	mViewModel;
		private IEnumerator<ExplorerTreeNode>		mTreeEnumerator;

		public ExplorerStrategyArtistAlbum( LibraryExplorerViewModel viewModel ) {
			mViewModel = viewModel;

			mViewModel.TreeViewItemTemplate = Application.Current.TryFindResource( "ArtistAlbumTemplate" ) as HierarchicalDataTemplate;
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
