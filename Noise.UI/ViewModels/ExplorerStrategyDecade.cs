using System.Collections.Generic;
using System.ComponentModel;
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
		private readonly Observal.Observer		mChangeObserver;
		private	LibraryExplorerViewModel		mViewModel;
		private	bool							mUseSortPrefixes;
		private IEnumerable<string>				mSortPrefixes;
		private IEnumerator<ArtistTreeNode>		mTreeEnumerator;
		private string							mLastSearchOptions;

		public ExplorerStrategyDecade( IUnityContainer container ) {
			mContainer = container;
			mEventAggregator = mContainer.Resolve<IEventAggregator>();
			mNoiseManager = mContainer.Resolve<INoiseManager>();

			mChangeObserver = new Observal.Observer();
			mChangeObserver.Extend( new PropertyChangedExtension()).WhenPropertyChanges( OnNodeChanged );

			mEventAggregator.GetEvent<Events.DatabaseItemChanged>().Subscribe( OnDatabaseItemChanged );
		}

		public void Initialize( LibraryExplorerViewModel viewModel ) {
			mViewModel = viewModel;

			mViewModel.TreeViewItemTemplate = Application.Current.TryFindResource( "ArtistAlbumTemplate" ) as HierarchicalDataTemplate;

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

		public IEnumerable<IndexNode> BuildIndex( IEnumerable<object> artistList ) {
			return( null );
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
