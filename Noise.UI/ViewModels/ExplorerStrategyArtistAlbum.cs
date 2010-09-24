﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Adapters;
using CuttingEdge.Conditions;
using Observal;
using Observal.Extensions;
using Condition = CuttingEdge.Conditions.Condition;

namespace Noise.UI.ViewModels {
	public class ExplorerStrategyArtistAlbum : ViewModelBase, IExplorerViewStrategy {
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
			mChangeObserver.Extend( new PropertyChangedExtension()).WhenPropertyChanges( OnNodeChanged );

			mEventAggregator.GetEvent<Events.DatabaseItemChanged>().Subscribe( OnDatabaseItemChanged );
		}

		private void OnNodeChanged( PropertyChangeNotification changeNotification ) {
			var notifier = changeNotification.Source as UserSettingsNotifier;

			if( notifier != null ) {
				if( changeNotification.PropertyName == "UiRating" ) {
					mNoiseManager.DataProvider.SetRating( notifier.TargetItem as DbArtist, notifier.UiRating );
				}
				if( changeNotification.PropertyName == "UiIsFavorite" ) {
					mNoiseManager.DataProvider.SetFavorite( notifier.TargetItem as DbArtist, notifier.UiIsFavorite );
				}
			}
		}

		private void OnDatabaseItemChanged( DbItemChangedArgs args ) {
			if( args.Item is DbArtist ) {
				BeginInvoke( () => {
					var artist = args.Item as DbArtist;

					if( artist != null ) {
						var treeNode = ( from ExplorerTreeNode node in mViewModel.TreeData
										 where artist.DbId == ( node.Item is DbArtist ? ( node.Item as DbArtist ).DbId : 0 )
										 select node ).FirstOrDefault();

						switch( args.Change ) {
							case DbItemChanged.Update:
								if( treeNode != null ) {
									treeNode.UiDisplay.UpdateObject( artist );
								}
								break;

							case DbItemChanged.Insert:
								mViewModel.TreeData.SuspendNotification();
								AddArtist( mViewModel.TreeData, artist );
								mViewModel.TreeData.Sort( node => node.Item is DbArtist ? ( node.Item as DbArtist ).Name : "", ListSortDirection.Ascending );
								mViewModel.TreeData.ResumeNotification();
								break;

							case DbItemChanged.Delete:
								if( treeNode != null ) {
									mViewModel.TreeData.Remove( treeNode );
								}
								break;
						}
					}
				} );
			}
		}

		public void Initialize( LibraryExplorerViewModel viewModel ) {
			mViewModel = viewModel;

			mViewModel.TreeViewItemTemplate = Application.Current.TryFindResource( "ArtistAlbumTemplate" ) as HierarchicalDataTemplate;

			mViewModel.SearchOptions.Add( cSearchOptionDefault + cSearchArtists );
			mViewModel.SearchOptions.Add( cSearchAlbums );
			mViewModel.SearchOptions.Add( cSearchOptionDefault + cSearchIgnoreCase );
		}

		public void PopulateTree( ObservableCollection<ExplorerTreeNode> tree, IDatabaseFilter filter ) {
			Condition.Requires( mViewModel ).IsNotNull();

			if( mNoiseManager.IsInitialized ) {
				using( var list = mNoiseManager.DataProvider.GetArtistList( filter )) {
					var artistList = from artist in list.List orderby artist.Name select artist;

					foreach( DbArtist artist in artistList ) {
						AddArtist( tree, artist );
					}
				}
			}
		}

		private void AddArtist( ICollection<ExplorerTreeNode> tree, DbArtist artist ) {
			var parent = artist.AlbumCount > 0 ? new ExplorerTreeNode( mEventAggregator, artist, FillChildren ) :
													new ExplorerTreeNode( mEventAggregator, artist );
			tree.Add( parent );
			mChangeObserver.Add( parent.UiEdit );
		}

		private List<ExplorerTreeNode> FillChildren( ExplorerTreeNode parent ) {
			List<ExplorerTreeNode>	retValue = null;
			var artist = parent.Item as DbArtist;

			if( artist != null ) {
				using( var albumList = mNoiseManager.DataProvider.GetAlbumList( artist )) {
					retValue = new List<ExplorerTreeNode>( from album in albumList.List orderby album.Name select new ExplorerTreeNode( mEventAggregator, parent, album ));
				}
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
