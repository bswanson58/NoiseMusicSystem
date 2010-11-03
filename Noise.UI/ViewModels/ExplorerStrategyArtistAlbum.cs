using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using AutoMapper;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Adapters;
using CuttingEdge.Conditions;
using Noise.UI.Dto;
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
		private IEnumerator<ArtistTreeNode>		mTreeEnumerator;
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
			var notifier = changeNotification.Source as UiArtist;

			if( notifier != null ) {
				if( changeNotification.PropertyName == "UiRating" ) {
					mNoiseManager.DataProvider.SetArtistRating( notifier.DbId, notifier.UiRating );
				}
				if( changeNotification.PropertyName == "UiIsFavorite" ) {
					mNoiseManager.DataProvider.SetArtistFavorite( notifier.DbId, notifier.UiIsFavorite );
				}
			}
		}

		private void OnDatabaseItemChanged( DbItemChangedArgs args ) {
			if( args.Item is DbArtist ) {
				BeginInvoke( () => {
					var artist = args.Item as DbArtist;

					if( artist != null ) {
						var treeNode = ( from ArtistTreeNode node in mViewModel.TreeData
										 where artist.DbId == node.Artist.DbId select node ).FirstOrDefault();

						switch( args.Change ) {
							case DbItemChanged.Update:
								if( treeNode != null ) {
									Mapper.DynamicMap( artist, treeNode.Artist );
								}
								break;

							case DbItemChanged.Insert:
								mViewModel.TreeData.SuspendNotification();
								AddArtist( mViewModel.TreeData, artist );
								mViewModel.TreeData.Sort( node => node.Artist.Name, ListSortDirection.Ascending );
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

		public IEnumerable<ArtistTreeNode> BuildTree( IDatabaseFilter filter ) {
			Condition.Requires( mViewModel ).IsNotNull();

			var retValue = new List<ArtistTreeNode>();

			if( mNoiseManager.IsInitialized ) {
				using( var list = mNoiseManager.DataProvider.GetArtistList( filter )) {
					var artistList = from artist in list.List orderby artist.Name select artist;

					foreach( DbArtist artist in artistList ) {
						AddArtist( retValue, artist );
					}
				}
			}

			return( retValue );
		}

		private void AddArtist( ICollection<ArtistTreeNode> tree, DbArtist artist ) {
			var uiArtist = new UiArtist( OnArtistSelect ) { DisplayGenre = mNoiseManager.TagManager.GetGenre( artist.Genre ) };
			Mapper.DynamicMap( artist, uiArtist );
			var parent = new ArtistTreeNode( uiArtist, FillChildren );

			tree.Add( parent );
			mChangeObserver.Add( parent.Artist );
		}

		private List<UiAlbum> FillChildren( ArtistTreeNode parent ) {
			var	retValue = new List<UiAlbum>();
			var artist = parent.Artist;

			if( artist != null ) {
				using( var albumList = mNoiseManager.DataProvider.GetAlbumList( artist.DbId )) {
					foreach( var dbAlbum in albumList.List ) {
						var uiAlbum = new UiAlbum( OnAlbumSelect, OnAlbumPlay ) { DisplayGenre = mNoiseManager.TagManager.GetGenre( dbAlbum.Genre ) };
						Mapper.DynamicMap( dbAlbum, uiAlbum );

						retValue.Add( uiAlbum );
					}
				}
			}

			return( retValue );
		}

		private void OnArtistSelect( long artistId ) {
			var artist = mNoiseManager.DataProvider.GetArtist( artistId );

			if( artist != null ) {
				mEventAggregator.GetEvent<Events.ArtistFocusRequested>().Publish( artist );
			}
		}

		private void OnAlbumSelect( long albumId ) {
			var album = mNoiseManager.DataProvider.GetAlbum( albumId );

			if( album != null ) {
				mEventAggregator.GetEvent<Events.AlbumFocusRequested>().Publish( album );
			}
		}

		private void OnAlbumPlay( long albumId ) {
			var album = mNoiseManager.DataProvider.GetAlbum( albumId );

			if( album != null ) {
				mEventAggregator.GetEvent<Events.AlbumPlayRequested>().Publish( album );
			}
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
				node.IsExpanded = true;

				retValue = true;
			}

			return ( retValue );
		}

		static IEnumerable<ArtistTreeNode> FindMatches( string searchText, IEnumerable<ArtistTreeNode> list, IEnumerable<string> options ) {
			IEnumerable<ArtistTreeNode>	retValue;

			if( options.Contains( cSearchIgnoreCase )) {
				var matchText = searchText.ToUpper();

				retValue = from node in list
						   where node.Artist.Name.ToUpper().Contains( matchText )
						   select node;
			}
			else {
				retValue = from node in list
						   where node.Artist.Name.Contains( searchText )
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
