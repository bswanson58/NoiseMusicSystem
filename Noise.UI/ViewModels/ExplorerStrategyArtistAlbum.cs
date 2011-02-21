﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using AutoMapper;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Adapters;
using CuttingEdge.Conditions;
using Noise.UI.Dto;
using Noise.UI.Support;
using Observal.Extensions;
using Condition = CuttingEdge.Conditions.Condition;

namespace Noise.UI.ViewModels {
	[Export( typeof( IExplorerViewStrategy ))]
	public class ExplorerStrategyArtistAlbum : ViewModelBase, IExplorerViewStrategy {
		private const string					cSearchOptionDefault = "!";
		private const string					cSearchArtists = "Artists";
		private const string					cSearchAlbums = "Albums";
		private const string					cSearchIgnoreCase = "Ignore Case";

		private IUnityContainer					mContainer;
		private IEventAggregator				mEventAggregator;
		private INoiseManager					mNoiseManager;
		private readonly Observal.Observer		mChangeObserver;
		private	LibraryExplorerViewModel		mViewModel;
		private	bool							mUseSortPrefixes;
		private IEnumerable<string>				mSortPrefixes;
		private IEnumerator<UiArtistTreeNode>	mTreeEnumerator;
		private string							mLastSearchOptions;

		private readonly IEnumerable<ViewSortStrategy>	mArtistSorts;
		private ViewSortStrategy				mCurrentArtistSort;
		private readonly IEnumerable<ViewSortStrategy>	mAlbumSorts;
		private ViewSortStrategy				mCurrentAlbumSort;

		public ExplorerStrategyArtistAlbum() {
			mChangeObserver = new Observal.Observer();
			mChangeObserver.Extend( new PropertyChangedExtension()).WhenPropertyChanges( OnNodeChanged );

			var	strategies = new List<ViewSortStrategy> { new ViewSortStrategy( "Artist Name", new List<SortDescription> { new SortDescription( "Artist.Name", ListSortDirection.Ascending ) }),
														  new ViewSortStrategy( "Unprefixed Artist Name", new List<SortDescription> { new SortDescription( "Artist.SortName", ListSortDirection.Ascending ) }),
														  new ViewSortStrategy( "Genre", new List<SortDescription> { new SortDescription( "Artist.Genre", ListSortDirection.Ascending ),
																												     new SortDescription( "Artist.SortName", ListSortDirection.Ascending )}) };
			mArtistSorts = strategies;
			mCurrentArtistSort = strategies[1];

			strategies = new List<ViewSortStrategy> { new ViewSortStrategy( "Album Name", new List<SortDescription> { new SortDescription( "Album.Name", ListSortDirection.Ascending ) } ),
													  new ViewSortStrategy( "Published Year", new List<SortDescription> { new SortDescription( "Album.PublishedYear", ListSortDirection.Descending ),
																														  new SortDescription( "Album.Name", ListSortDirection.Ascending ) })};
			mAlbumSorts = strategies;
			mCurrentAlbumSort = strategies[0];
		}

		public void Initialize( IUnityContainer container, LibraryExplorerViewModel viewModel ) {
			mViewModel = viewModel;
			mContainer = container;
			mEventAggregator = mContainer.Resolve<IEventAggregator>();
			mNoiseManager = mContainer.Resolve<INoiseManager>();
		}

		public string StrategyId {
			get{ return( "ViewStrategy_ArtistAlbum" ); }
		}

		public string StrategyName {
			get{ return( "Artists / Albums" ); }
		}

		public bool IsDefaultStrategy {
			get{ return( true ); }
		}

		public void UseSortPrefixes( bool enable, IEnumerable<string> sortPrefixes ) {
			mUseSortPrefixes =enable;
			mSortPrefixes = sortPrefixes;
		}

		public void Activate() {
			mEventAggregator.GetEvent<Events.DatabaseItemChanged>().Subscribe( OnDatabaseItemChanged );

			mViewModel.TreeViewItemTemplate = Application.Current.TryFindResource( "ArtistAlbumTemplate" ) as HierarchicalDataTemplate;

			mViewModel.SearchOptions.Add( cSearchOptionDefault + cSearchArtists );
			mViewModel.SearchOptions.Add( cSearchAlbums );
			mViewModel.SearchOptions.Add( cSearchOptionDefault + cSearchIgnoreCase );
		}

		public void Deactivate() {
			mEventAggregator.GetEvent<Events.DatabaseItemChanged>().Unsubscribe( OnDatabaseItemChanged );
		}

		private static void OnNodeChanged( PropertyChangeNotification propertyNotification ) {
			var notifier = propertyNotification.Source as UiBase;

			if( notifier != null ) {
				if( propertyNotification.PropertyName == "UiRating" ) {
					GlobalCommands.SetRating.Execute( new SetRatingCommandArgs( notifier.DbId, notifier.UiRating ));
				}
				if( propertyNotification.PropertyName == "UiIsFavorite" ) {
					GlobalCommands.SetFavorite.Execute( new SetFavoriteCommandArgs( notifier.DbId, notifier.UiIsFavorite ));
				}
			}
		}

		private void OnDatabaseItemChanged( DbItemChangedArgs args ) {
			var item = args.GetItem( mNoiseManager.DataProvider );

			if( item is DbArtist ) {
				BeginInvoke( () => {
					var artist = item as DbArtist;

					if( artist != null ) {
						var treeNode = ( from UiArtistTreeNode node in mViewModel.TreeData
										 where artist.DbId == node.Artist.DbId select node ).FirstOrDefault();

						switch( args.Change ) {
							case DbItemChanged.Update:
								if( treeNode != null ) {
									UpdateUiArtist( treeNode.Artist, artist );
								}
								break;

							case DbItemChanged.Insert:
								AddArtist( mViewModel.TreeData, artist );
								break;

							case DbItemChanged.Delete:
								if( treeNode != null ) {
									mViewModel.TreeData.Remove( treeNode );
									mChangeObserver.Release( treeNode.Artist );
								}
								break;
						}
					}
				} );
			}
			else if(( item is DbAlbum ) &&
			        ( args.Change == DbItemChanged.Update )) {
				BeginInvoke( () => {
					var dbAlbum = item as DbAlbum;

					if( dbAlbum != null ) {
						var treeNode = ( from UiArtistTreeNode node in mViewModel.TreeData
										 where dbAlbum.Artist == node.Artist.DbId select node ).FirstOrDefault();
						if( treeNode != null ) {
							var uiAlbum = ( from UiAlbumTreeNode a in treeNode.Children
											where a.Album.DbId == dbAlbum.DbId select a.Album ).FirstOrDefault();

							if( uiAlbum != null ) {
								Mapper.DynamicMap( dbAlbum, uiAlbum );
							}
						}
					}
				});
			}
		}

		public IEnumerable<UiTreeNode> BuildTree( IDatabaseFilter filter ) {
			Condition.Requires( mViewModel ).IsNotNull();

			var retValue = new List<UiTreeNode>();

			if( mNoiseManager.IsInitialized ) {
				using( var list = mNoiseManager.DataProvider.GetArtistList( filter )) {
					foreach( var artist in list.List ) {
						AddArtist( retValue, artist );
					}
				}

				SetArtistSorting( mCurrentArtistSort );
			}

			return( retValue );
		}

		public IEnumerable<IndexNode> BuildIndex( IEnumerable<UiTreeNode> artistList ) {
			var retValue = new List<IndexNode>();

			const string alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
			var obs = alphabet.ToObservable();

			obs.Subscribe( ch =>
			{
				var artist = artistList.FirstOrDefault( node => node is UiArtistTreeNode ? ( node as UiArtistTreeNode ).Artist.SortName.StartsWith( ch.ToString()) : false );

				if(( artist != null ) &&
				   ( artist is UiArtistTreeNode )) {
					retValue.Add( new IndexNode( ch.ToString(), artist as UiArtistTreeNode ));
				}
			});

			return( retValue );
		}

		private void UpdateUiArtist( UiArtist uiArtist, DbArtist artist ) {
			Mapper.DynamicMap( artist, uiArtist );
			uiArtist.DisplayGenre = mNoiseManager.TagManager.GetGenre( artist.Genre );

			if( mUseSortPrefixes ) {
				FormatSortPrefix( uiArtist );
			}
		}

		private void AddArtist( ICollection<UiTreeNode> tree, DbArtist artist ) {
			var uiArtist = new UiArtist();
			UpdateUiArtist( uiArtist, artist );

			var parent = new UiArtistTreeNode( uiArtist, OnArtistSelect, null, FillChildren );

			if( mUseSortPrefixes ) {
				FormatSortPrefix( uiArtist );
			}

			tree.Add( parent );
			mChangeObserver.Add( parent.Artist );
		}

		private void FormatSortPrefix( UiArtist artist ) {
			if( mSortPrefixes != null ) {
				foreach( string prefix in mSortPrefixes ) {
					if( artist.Name.StartsWith( prefix, StringComparison.CurrentCultureIgnoreCase )) {
						artist.SortName = artist.Name.Remove( 0, prefix.Length ).Trim();
						artist.DisplayName = "(" + artist.Name.Insert( prefix.Length, ")" );

						break;
					}
				}
			}
		}

		private void FillChildren( UiArtistTreeNode parent ) {
			var	retValue = new List<UiAlbumTreeNode>();
			var artist = parent.Artist;

			if( artist != null ) {
				using( var albumList = mNoiseManager.DataProvider.GetAlbumList( artist.DbId )) {
					foreach( var dbAlbum in from DbAlbum album in albumList.List orderby album.Name select album ) {
						var uiAlbum = new UiAlbum { DisplayGenre = mNoiseManager.TagManager.GetGenre( dbAlbum.Genre ) };
						Mapper.DynamicMap( dbAlbum, uiAlbum );

						retValue.Add( new UiAlbumTreeNode( uiAlbum, OnAlbumSelect, OnAlbumPlay ));
					}
				}
			}

			parent.SetChildren( retValue );
		}

		private void OnArtistSelect( UiArtistTreeNode artistNode ) {
			if( artistNode.IsSelected ) {
				var artist = mNoiseManager.DataProvider.GetArtist( artistNode.Artist.DbId );

				if( artist != null ) {
					mEventAggregator.GetEvent<Events.ArtistFocusRequested>().Publish( artist );
				}
			}
		}

		private void OnAlbumSelect( UiAlbumTreeNode albumNode ) {
			if( albumNode.IsSelected ) {
				var album = mNoiseManager.DataProvider.GetAlbum( albumNode.Album.DbId );

				if( album != null ) {
					mEventAggregator.GetEvent<Events.AlbumFocusRequested>().Publish( album );
				}
			}
		}

		private void OnAlbumPlay( UiAlbumTreeNode albumNode ) {
			var album = mNoiseManager.DataProvider.GetAlbum( albumNode.Album.DbId );

			if( album != null ) {
				GlobalCommands.PlayAlbum.Execute( album );
			}
		}

		public void ConfigureView() {
			var dialogModel = new ArtistAlbumConfigViewModel( mArtistSorts, mCurrentArtistSort, mAlbumSorts, mCurrentAlbumSort );
			var	dialogService = mContainer.Resolve<IDialogService>();

			if( dialogService.ShowDialog( DialogNames.ArtistAlbumConfiguration, dialogModel ) == true ) {
				SetArtistSorting( dialogModel.SelectedArtistSort );
				SetAlbumSorting( dialogModel.SelectedAlbumSort );
			}
		}

		private void SetArtistSorting( ViewSortStrategy strategy ) {
			mViewModel.TreeViewSource.SortDescriptions.Clear();
			foreach( var sort in strategy.SortDescriptions ) {
				mViewModel.TreeViewSource.SortDescriptions.Add( sort );
			}

			mCurrentArtistSort = strategy;
		}

		private void SetAlbumSorting( ViewSortStrategy strategy ) {
			mCurrentAlbumSort = strategy;
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
				mTreeEnumerator = FindMatches( searchText, mViewModel.TreeData.OfType<UiArtistTreeNode>(), searchOptions ).GetEnumerator();
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

		static IEnumerable<UiArtistTreeNode> FindMatches( string searchText, IEnumerable<UiArtistTreeNode> list, IEnumerable<string> options ) {
			IEnumerable<UiArtistTreeNode>	retValue;

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
