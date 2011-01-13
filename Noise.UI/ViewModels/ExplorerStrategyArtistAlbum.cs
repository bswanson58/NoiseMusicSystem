using System;
using System.Collections.Generic;
using System.ComponentModel;
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
		private readonly Observal.Observer		mChangeObserver;
		private	LibraryExplorerViewModel		mViewModel;
		private	bool							mUseSortPrefixes;
		private IEnumerable<string>				mSortPrefixes;
		private IEnumerator<ArtistTreeNode>		mTreeEnumerator;
		private string							mLastSearchOptions;

		public ExplorerStrategyArtistAlbum( IUnityContainer container ) {
			mContainer = container;
			mEventAggregator = mContainer.Resolve<IEventAggregator>();
			mNoiseManager = mContainer.Resolve<INoiseManager>();

			mChangeObserver = new Observal.Observer();
			mChangeObserver.Extend( new PropertyChangedExtension()).WhenPropertyChanges( OnNodeChanged );

			mEventAggregator.GetEvent<Events.DatabaseItemChanged>().Subscribe( OnDatabaseItemChanged );
		}

		public void UseSortPrefixes( bool enable, IEnumerable<string> sortPrefixes ) {
			mUseSortPrefixes =enable;
			mSortPrefixes = sortPrefixes;
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
						var treeNode = ( from ArtistTreeNode node in mViewModel.TreeData
										 where artist.DbId == node.Artist.DbId select node ).FirstOrDefault();

						switch( args.Change ) {
							case DbItemChanged.Update:
								if( treeNode != null ) {
									UpdateUiArtist( treeNode.Artist, artist );
								}
								break;

							case DbItemChanged.Insert:
								mViewModel.TreeData.SuspendNotification();
								AddArtist( mViewModel.TreeData, artist );
								mViewModel.TreeData.Sort( node => node.Artist.SortName, ListSortDirection.Ascending );
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
					foreach( var artist in list.List ) {
						AddArtist( retValue, artist );
					}
				}

				retValue.Sort( ( node1, node2 ) => string.Compare( node1.Artist.SortName, node2.Artist.SortName ));
			}

			return( retValue );
		}

		public IEnumerable<IndexNode> BuildIndex( IEnumerable<ArtistTreeNode> artistList ) {
			var retValue = new List<IndexNode>();

			const string alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
			var obs = alphabet.ToObservable();

			obs.Subscribe( ch =>
			{
				var artist = artistList.FirstOrDefault( node => node.Artist.SortName.StartsWith( ch.ToString()));

				if( artist != null ) {
					retValue.Add( new IndexNode( ch.ToString(), artist ));
				}
			});

			return( retValue );
		}

		private void UpdateUiArtist( UiArtist uiArtist, DbArtist artist ) {
			Mapper.DynamicMap( artist, uiArtist );
			uiArtist.DisplayGenre = mNoiseManager.TagManager.GetGenre( artist.Genre );
		}

		private void AddArtist( ICollection<ArtistTreeNode> tree, DbArtist artist ) {
			var uiArtist = new UiArtist( OnArtistSelect );
			UpdateUiArtist( uiArtist, artist );

			var parent = new ArtistTreeNode( uiArtist, FillChildren );

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

		private List<UiAlbum> FillChildren( ArtistTreeNode parent ) {
			var	retValue = new List<UiAlbum>();
			var artist = parent.Artist;

			if( artist != null ) {
				using( var albumList = mNoiseManager.DataProvider.GetAlbumList( artist.DbId )) {
					foreach( var dbAlbum in from DbAlbum album in albumList.List orderby album.Name select album ) {
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
				GlobalCommands.PlayAlbum.Execute( album );
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
