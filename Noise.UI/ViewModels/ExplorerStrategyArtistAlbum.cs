using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Adapters;
using CuttingEdge.Conditions;
using Observal;
using Observal.Extensions;
using Condition = CuttingEdge.Conditions.Condition;

namespace Noise.UI.ViewModels {
	public class ExplorerStrategyArtistAlbum : IExplorerViewStrategy {
		private const string					cSearchOptionDefault = "!";
		private const string					cSearchArtists = "Artists";
		private const string					cSearchAlbums = "Albums";
		private const string					cSearchIgnoreCase = "Ignore Case";

		private readonly IUnityContainer		mContainer;
		private readonly IEventAggregator		mEventAggregator;
		private readonly INoiseManager			mNoiseManager;
		private readonly Observer				mChangeObserver;
		private	LibraryExplorerViewModel		mViewModel;
		private IEnumerator<ExplorerTreeNode>	mTreeEnumerator;
		private string							mLastSearchOptions;

		public ExplorerStrategyArtistAlbum( IUnityContainer container ) {
			mContainer = container;
			mEventAggregator = mContainer.Resolve<IEventAggregator>();
			mNoiseManager = mContainer.Resolve<INoiseManager>();

			mChangeObserver = new Observer();
			mChangeObserver.Extend( new PropertyChangedExtension()).WhenPropertyChanges( node => OnNodeChanged( node.Source ));

			mEventAggregator.GetEvent<Events.ArtistContentUpdated>().Subscribe( OnArtistUpdated );
		}

		private void OnNodeChanged( object source ) {
			var notifier = source as UserSettingsNotifier;

			if( notifier != null ) {
				mNoiseManager.DataProvider.UpdateItem( notifier.TargetItem );
			}
		}

		public void OnArtistUpdated( DbArtist forArtist ) {
			foreach( var treeNode in mViewModel.TreeData ) {
				var artist = treeNode.Item as DbArtist;

				if(( artist != null ) &&
				   ( string.Compare( artist.Name, forArtist.Name, true ) == 0 )) {
					treeNode.SetItem( forArtist );

					break;
				}
			}
		}

		public void Initialize( LibraryExplorerViewModel viewModel ) {
			mViewModel = viewModel;

			mViewModel.TreeViewItemTemplate = Application.Current.TryFindResource( "ArtistAlbumTemplate" ) as HierarchicalDataTemplate;

			mViewModel.SearchOptions.Add( cSearchOptionDefault + cSearchArtists );
			mViewModel.SearchOptions.Add( cSearchAlbums );
			mViewModel.SearchOptions.Add( cSearchOptionDefault + cSearchIgnoreCase );
		}

		public void PopulateTree( ObservableCollection<ExplorerTreeNode> tree ) {
			Condition.Requires( mViewModel ).IsNotNull();

			if( mNoiseManager.IsInitialized ) {
				var artistList = from artist in mNoiseManager.DataProvider.GetArtistList() orderby artist.Name select artist;

				foreach( DbArtist artist in artistList ) {
					var parent = artist.AlbumCount > 0 ? new ExplorerTreeNode( mEventAggregator, artist, FillChildren ) :
														 new ExplorerTreeNode( mEventAggregator, artist );
					tree.Add( parent );
					mChangeObserver.Add( parent.SettingsNotifier );
				}
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

		public bool Search( string searchText, IEnumerable<string> searchOptions ) {
			var retValue = false;

			var theseOptions = String.Concat( searchOptions );
			if(!theseOptions.Equals( mLastSearchOptions )) {
				mLastSearchOptions = theseOptions;

				ClearCurrentSearch();
			}

			if(( mTreeEnumerator != null ) &&
			   ( mTreeEnumerator.Current != null )) {
				mTreeEnumerator.Current.IsSelected = false;
			}

			if(( mTreeEnumerator == null ) ||
			   ( !mTreeEnumerator.MoveNext())) {
				mTreeEnumerator = FindMatches( searchText, mViewModel.TreeData, searchOptions ).GetEnumerator();
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

		static IEnumerable<ExplorerTreeNode> FindMatches( string searchText, IEnumerable<ExplorerTreeNode> list, IEnumerable<string> options ) {
			IEnumerable<ExplorerTreeNode>	retValue;

			if( options.Contains( cSearchIgnoreCase )) {
				var matchText = searchText.ToUpper();

				retValue = from node in list
						   let artist = node.Item as DbArtist
						   where artist != null where artist.Name.ToUpper().Contains( matchText )
						   select node;
			}
			else {
				retValue = from node in list
						   let artist = node.Item as DbArtist
						   where artist != null where artist.Name.Contains( searchText )
						   select node;
			}

/*			foreach( var node in list ) {
				var artist = node.Item as DbArtist;

				if( artist != null ) {
					if( artist.Name.Contains( searchText ) ) {
						yield return node;
					}
				}
			} */

			return( retValue );
		}

		public void ClearCurrentSearch() {
			mTreeEnumerator = null;
		}
	}
}
