using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using AutoMapper;
using CuttingEdge.Conditions;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Adapters;
using Noise.UI.Dto;
using Observal.Extensions;
using Condition = CuttingEdge.Conditions.Condition;

namespace Noise.UI.ViewModels {
	internal class ExplorerStrategyDecade : ViewModelBase, IExplorerViewStrategy {
		private readonly IUnityContainer		mContainer;
		private readonly IEventAggregator		mEventAggregator;
		private readonly INoiseManager			mNoiseManager;
		private	LibraryExplorerViewModel		mViewModel;
		private	bool							mUseSortPrefixes;
		private IEnumerable<string>				mSortPrefixes;
		private IEnumerator<UiDecadeTreeNode>	mTreeEnumerator;
		private string							mLastSearchOptions;

		public ExplorerStrategyDecade( IUnityContainer container ) {
			mContainer = container;
			mEventAggregator = mContainer.Resolve<IEventAggregator>();
			mNoiseManager = mContainer.Resolve<INoiseManager>();

			mEventAggregator.GetEvent<Events.DatabaseItemChanged>().Subscribe( OnDatabaseItemChanged );
		}

		public void Initialize( LibraryExplorerViewModel viewModel ) {
			mViewModel = viewModel;

			mViewModel.TreeViewItemTemplate = Application.Current.TryFindResource( "DecadeExplorerTemplate" ) as HierarchicalDataTemplate;

//			mViewModel.SearchOptions.Add( cSearchOptionDefault + cSearchArtists );
//			mViewModel.SearchOptions.Add( cSearchAlbums );
//			mViewModel.SearchOptions.Add( cSearchOptionDefault + cSearchIgnoreCase );
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

/*			if( item is DbArtist ) {
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
			else if(( item is DbAlbum ) &&
			        ( args.Change == DbItemChanged.Update )) {
				BeginInvoke( () => {
					var dbAlbum = item as DbAlbum;

					if( dbAlbum != null ) {
						var treeNode = ( from ArtistTreeNode node in mViewModel.TreeData
										 where dbAlbum.Artist == node.Artist.DbId select node ).FirstOrDefault();
						if( treeNode != null ) {
							var uiAlbum = ( from UiAlbum a in treeNode.Children
											where a.DbId == dbAlbum.DbId select a ).FirstOrDefault();

							if( uiAlbum != null ) {
								Mapper.DynamicMap( dbAlbum, uiAlbum );
							}
						}
					}
				});
			}
*/		}

		public IEnumerable<object> BuildTree( IDatabaseFilter filter ) {
			Condition.Requires( mViewModel ).IsNotNull();

			var retValue = new List<UiDecadeTreeNode>();

			if( mNoiseManager.IsInitialized ) {
				retValue.AddRange( from tag in mNoiseManager.TagManager.DecadeTagList select new UiDecadeTreeNode( tag, null, null, FillDecadeArtists ));
				retValue.Sort( ( node1, node2 ) => node2.Tag.StartYear.CompareTo( node1.Tag.StartYear )); // descending
			}

			return( retValue );
		}

		private void FillDecadeArtists( UiDecadeTreeNode decadeNode ) {
			var	artistIdList = mNoiseManager.TagManager.ArtistList( decadeNode.Tag.DbId );
			var childNodes = new List<UiArtistTreeNode>();

			foreach( var artistId in artistIdList ) {
				var dbArtist = mNoiseManager.DataProvider.GetArtist( artistId );

				if( dbArtist != null ) {
					var	uiArtist = new UiArtist( null );

					UpdateUiArtist( uiArtist, dbArtist );
					childNodes.Add( new UiArtistTreeNode( decadeNode.Tag, uiArtist, OnArtistSelect, null, FillArtistAlbums ));
				}
			}

			childNodes.Sort( ( node1, node2 ) => string.Compare( node1.Artist.SortName, node2.Artist.SortName ));
			decadeNode.SetChildren( childNodes );
		}

		private void FillArtistAlbums( UiArtistTreeNode artistNode ) {
			var	retValue = new List<UiAlbumTreeNode>();
			var artist = artistNode.Artist;

			if( artist != null ) {
				var albumIdList = mNoiseManager.TagManager.AlbumList( artistNode.Artist.DbId, artistNode.DecadeTag.DbId );

				foreach( var albumId in albumIdList ) {
					var dbAlbum = mNoiseManager.DataProvider.GetAlbum( albumId );

					if( dbAlbum != null ) {
						var uiAlbum = new UiAlbum( null, null );
						Mapper.DynamicMap( dbAlbum, uiAlbum );
						var treeNode = new UiAlbumTreeNode( uiAlbum, OnAlbumSelect, OnAlbumPlay );

						retValue.Add( treeNode );
					}
				}
			}

			retValue.Sort( ( node1, node2 ) => node1.Album.PublishedYear.CompareTo( node2.Album.PublishedYear ));
			artistNode.SetChildren( retValue );
		}

		private void UpdateUiArtist( UiArtist uiArtist, DbArtist artist ) {
			Mapper.DynamicMap( artist, uiArtist );
			uiArtist.DisplayGenre = mNoiseManager.TagManager.GetGenre( artist.Genre );

			if( mUseSortPrefixes ) {
				FormatSortPrefix( uiArtist );
			}
		}

		private void UpdateUiAlbum( UiAlbum uiAlbum, DbAlbum dbAlbum ) {
			Mapper.DynamicMap( dbAlbum, uiAlbum );
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

		private void OnArtistSelect( UiArtistTreeNode artistNode ) {
			var artist = mNoiseManager.DataProvider.GetArtist( artistNode.Artist.DbId );

			if( artist != null ) {
				mEventAggregator.GetEvent<Events.ArtistFocusRequested>().Publish( artist );
			}
		}

		private void OnAlbumSelect( UiAlbumTreeNode albumNode ) {
			var album = mNoiseManager.DataProvider.GetAlbum( albumNode.Album.DbId );

			if( album != null ) {
				mEventAggregator.GetEvent<Events.AlbumFocusRequested>().Publish( album );
			}
		}

		private void OnAlbumPlay( UiAlbumTreeNode albumNode ) {
			var album = mNoiseManager.DataProvider.GetAlbum( albumNode.Album.DbId );

			if( album != null ) {
				GlobalCommands.PlayAlbum.Execute( album );
			}
		}

		public IEnumerable<IndexNode> BuildIndex( IEnumerable<object> artistList ) {
			var	retValue = new List<IndexNode>();

			return( retValue );
		}

		private void AddArtist( IEnumerable<object> list, object a ) {
			
		}

		public bool Search( string searchText, IEnumerable<string> searchOptions ) {
			return( false );
		}

		public void ClearCurrentSearch() {
		}
	}
}
