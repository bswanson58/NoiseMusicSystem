using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using AutoMapper;
using CuttingEdge.Conditions;
using Microsoft.Practices.Prism.Events;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Adapters;
using Noise.UI.Dto;
using Noise.UI.Support;
using Observal.Extensions;
using Condition = CuttingEdge.Conditions.Condition;

namespace Noise.UI.ViewModels {
	[Export( typeof( IExplorerViewStrategy ))]
	internal class ExplorerStrategyDecade : ViewModelBase, IExplorerViewStrategy {
		private const string					cSearchOptionDefault = "!";
		private const string					cSearchArtists = "Artists";
		private const string					cSearchAlbums = "Albums";
		private const string					cSearchIgnoreCase = "Ignore Case";

		private readonly IEventAggregator		mEventAggregator;
		private readonly IArtistProvider		mArtistProvider;
		private readonly IAlbumProvider			mAlbumProvider;
		private readonly ITagManager			mTagManager;
		private readonly IDialogService			mDialogService;
		private readonly Observal.Observer		mChangeObserver;
		private	LibraryExplorerViewModel		mViewModel;
		private	bool							mUseSortPrefixes;
		private IEnumerable<string>				mSortPrefixes;
		private IEnumerator<UiArtistTreeNode>	mTreeEnumerator;
		private string							mLastSearchOptions;

		private readonly IEnumerable<ViewSortStrategy>	mArtistSorts;
		private ViewSortStrategy				mCurrentArtistSort;
		private readonly Subject<ViewSortStrategy>		mArtistSortSubject;
		private	IObservable<ViewSortStrategy>	ArtistSortChange { get { return( mArtistSortSubject.AsObservable()); }}
		private readonly IEnumerable<ViewSortStrategy>	mAlbumSorts;
		private ViewSortStrategy				mCurrentAlbumSort;
		private readonly Subject<ViewSortStrategy>		mAlbumSortSubject;
		private	IObservable<ViewSortStrategy>	AlbumSortChange { get { return( mAlbumSortSubject.AsObservable()); }}

		public ExplorerStrategyDecade( IEventAggregator eventAggregator, IArtistProvider artistProvider, IAlbumProvider albumProvider,
									   ITagManager tagManager, IDialogService dialogService ) {
			mEventAggregator = eventAggregator;
			mArtistProvider = artistProvider;
			mAlbumProvider = albumProvider;
			mTagManager = tagManager;
			mDialogService = dialogService;

			mChangeObserver = new Observal.Observer();
			mChangeObserver.Extend( new PropertyChangedExtension()).WhenPropertyChanges( OnNodeChanged );

			var	strategies = new List<ViewSortStrategy> { new ViewSortStrategy( "Artist Name", new List<SortDescription> { new SortDescription( "Artist.Name", ListSortDirection.Ascending ) }),
														  new ViewSortStrategy( "Unprefixed Artist Name", new List<SortDescription> { new SortDescription( "Artist.SortName", ListSortDirection.Ascending ) }),
														  new ViewSortStrategy( "Genre", new List<SortDescription> { new SortDescription( "Artist.Genre", ListSortDirection.Ascending ),
																												     new SortDescription( "Artist.SortName", ListSortDirection.Ascending )}) };
			mArtistSorts = strategies;
			mCurrentArtistSort = strategies[1];
			mArtistSortSubject = new Subject<ViewSortStrategy>();

			strategies = new List<ViewSortStrategy> { new ViewSortStrategy( "Album Name", new List<SortDescription> { new SortDescription( "Album.Name", ListSortDirection.Ascending ) } ),
													  new ViewSortStrategy( "Published Year", new List<SortDescription> { new SortDescription( "Album.PublishedYear", ListSortDirection.Ascending ),
																														  new SortDescription( "Album.Name", ListSortDirection.Ascending ) })};
			mAlbumSorts = strategies;
			mCurrentAlbumSort = strategies[1];
			mAlbumSortSubject = new Subject<ViewSortStrategy>();
		}

		public void Initialize( LibraryExplorerViewModel viewModel ) {
			mViewModel = viewModel;
		}

		public string StrategyId {
			get{ return( "ViewStrategy_Decade" ); }
		}

		public string StrategyName {
			get{ return( "Decades / Artists / Albums" ); }
		}

		public bool IsDefaultStrategy {
			get{ return( false ); }
		}

		public void UseSortPrefixes( bool enable, IEnumerable<string> sortPrefixes ) {
			mUseSortPrefixes =enable;
			mSortPrefixes = sortPrefixes;
		}

		public void Activate() {
			mViewModel.TreeViewItemTemplate = Application.Current.TryFindResource( "DecadeExplorerTemplate" ) as HierarchicalDataTemplate;

			mEventAggregator.GetEvent<Events.DatabaseItemChanged>().Subscribe( OnDatabaseItemChanged );

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
			var item = args.Item;

			if( item is DbArtist ) {
				BeginInvoke( () => {
					var artist = item as DbArtist;

					if( artist != null ) {
						foreach( var decadeNode in mViewModel.TreeData.OfType<UiDecadeTreeNode>() ) {
							var treeNode = ( from UiArtistTreeNode node in decadeNode.Children
											 where artist.DbId == node.Artist.DbId select node ).FirstOrDefault();

							switch( args.Change ) {
								case DbItemChanged.Update:
									if( treeNode != null ) {
										UpdateUiArtist( treeNode.Artist, artist );

										decadeNode.UpdateSort();
									}
									break;

								case DbItemChanged.Insert:
									decadeNode.Children.Add( CreateArtistNode( artist, decadeNode ));
									break;

								case DbItemChanged.Delete:
									if( treeNode != null ) {
										mViewModel.TreeData.Remove( treeNode );
									}
									break;
							}
						}
					}
				} );
			}
			else if(( item is DbAlbum ) &&
			        ( args.Change == DbItemChanged.Update )) {
				BeginInvoke( () => {
					var dbAlbum = item as DbAlbum;

					if( dbAlbum != null ) {
						foreach( var decadeNode in mViewModel.TreeData.OfType<UiDecadeTreeNode>() ) {
							var treeNode = ( from UiArtistTreeNode node in decadeNode.Children
											 where dbAlbum.Artist == node.Artist.DbId select node ).FirstOrDefault();
							if( treeNode != null ) {
								var uiAlbum = ( from UiAlbumTreeNode a in treeNode.Children
												where a.Album.DbId == dbAlbum.DbId select a.Album ).FirstOrDefault();

								if( uiAlbum != null ) {
									UpdateUiAlbum( uiAlbum, dbAlbum );

									treeNode.UpdateSort();
								}
							}
						}
					}
				});
			}
		}

		public IEnumerable<UiTreeNode> BuildTree( IDatabaseFilter filter ) {
			Condition.Requires( mViewModel ).IsNotNull();

			var retValue = new List<UiDecadeTreeNode>();

			retValue.AddRange( from tag in mTagManager.DecadeTagList
								select new UiDecadeTreeNode( tag, null, null, FillDecadeArtists, WebsiteRequest, mCurrentArtistSort, ArtistSortChange ));

			mViewModel.TreeViewSource.SortDescriptions.Clear();
			mViewModel.TreeViewSource.SortDescriptions.Add( new SortDescription( "Tag.StartYear", ListSortDirection.Descending ));

			return( retValue );
		}

		private void WebsiteRequest( UiDecadeTreeNode decadeNode ) {
			if(!string.IsNullOrWhiteSpace( decadeNode.Tag.Website )) {
				mEventAggregator.GetEvent<Events.WebsiteRequest>().Publish( decadeNode.Tag.Website );
			}
		}

		private void FillDecadeArtists( UiDecadeTreeNode decadeNode ) {
			var	artistIdList = mTagManager.ArtistListForDecade( decadeNode.Tag.DbId );
			var childNodes = ( from artistId in artistIdList
			                   select mArtistProvider.GetArtist( artistId )
			                   into dbArtist where dbArtist != null select CreateArtistNode( dbArtist, decadeNode )).ToList();

//			childNodes.Sort( ( node1, node2 ) => string.Compare( node1.Artist.SortName, node2.Artist.SortName ));
			decadeNode.SetChildren( childNodes );
		}

		private void FillArtistAlbums( UiArtistTreeNode artistNode ) {
			var	retValue = new List<UiAlbumTreeNode>();
			var artist = artistNode.Artist;

			if( artist != null ) {
				var albumIdList = mTagManager.AlbumListForDecade( artistNode.Artist.DbId, artistNode.Parent.Tag.DbId );

				foreach( var albumId in albumIdList ) {
					var dbAlbum = mAlbumProvider.GetAlbum( albumId );

					if( dbAlbum != null ) {
						var uiAlbum = new UiAlbum();
						UpdateUiAlbum( uiAlbum, dbAlbum );
						var treeNode = new UiAlbumTreeNode( uiAlbum, OnAlbumSelect, OnAlbumPlay );

						retValue.Add( treeNode );
					}
				}
			}

			retValue.Sort( ( node1, node2 ) => node1.Album.PublishedYear.CompareTo( node2.Album.PublishedYear ));
			artistNode.SetChildren( retValue );
		}

		private UiArtistTreeNode CreateArtistNode( DbArtist dbArtist, UiDecadeTreeNode parent ) {
			var	uiArtist = new UiArtist();

			UpdateUiArtist( uiArtist, dbArtist );
			mChangeObserver.Add( uiArtist );

			return( new UiArtistTreeNode( parent, uiArtist, OnArtistSelect, null, FillArtistAlbums, mCurrentAlbumSort, AlbumSortChange ));
		}

		public void ConfigureView() {
			var dialogModel = new ArtistAlbumConfigViewModel( mArtistSorts, mCurrentArtistSort, mAlbumSorts, mCurrentAlbumSort );

			if( mDialogService.ShowDialog( DialogNames.ArtistAlbumConfiguration, dialogModel ) == true ) {
				SetArtistSorting( dialogModel.SelectedArtistSort );
				SetAlbumSorting( dialogModel.SelectedAlbumSort );
			}
		}

		private void SetArtistSorting( ViewSortStrategy strategy ) {
			mCurrentArtistSort = strategy;

			mArtistSortSubject.OnNext( mCurrentArtistSort );
		}

		private void SetAlbumSorting( ViewSortStrategy strategy ) {
			mCurrentAlbumSort = strategy;

			mAlbumSortSubject.OnNext( mCurrentAlbumSort );
		}

		private void UpdateUiArtist( UiArtist uiArtist, DbArtist artist ) {
			Mapper.DynamicMap( artist, uiArtist );
			uiArtist.DisplayGenre = mTagManager.GetGenre( artist.Genre );

			if( mUseSortPrefixes ) {
				FormatSortPrefix( uiArtist );
			}
		}

		private static void UpdateUiAlbum( UiAlbum uiAlbum, DbAlbum dbAlbum ) {
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
			var artist = mArtistProvider.GetArtist( artistNode.Artist.DbId );

			if( artist != null ) {
				mEventAggregator.GetEvent<Events.ArtistFocusRequested>().Publish( artist );
			}
		}

		private void OnAlbumSelect( UiAlbumTreeNode albumNode ) {
			var album = mAlbumProvider.GetAlbum( albumNode.Album.DbId );

			if( album != null ) {
				mEventAggregator.GetEvent<Events.AlbumFocusRequested>().Publish( album );
			}
		}

		private void OnAlbumPlay( UiAlbumTreeNode albumNode ) {
			var album = mAlbumProvider.GetAlbum( albumNode.Album.DbId );

			if( album != null ) {
				GlobalCommands.PlayAlbum.Execute( album );
			}
		}

		public IEnumerable<IndexNode> BuildIndex( IEnumerable<UiTreeNode> artistList ) {
			var	retValue = new List<IndexNode>();

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
				mTreeEnumerator = FindMatches( searchText, searchOptions ).GetEnumerator();
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

		private IEnumerable<UiArtistTreeNode> FindMatches( string searchText, IEnumerable<string> options ) {
			var	retValue = new List<UiArtistTreeNode>();

			foreach( var decadeNode in mViewModel.TreeData.OfType<UiDecadeTreeNode>() ) {
				if( decadeNode.RequiresChildren ) {
					FillDecadeArtists( decadeNode );
				}

				if( options.Contains( cSearchIgnoreCase )) {
					var matchText = searchText.ToUpper();

					retValue.AddRange( from node in decadeNode.Children where node.Artist.Name.ToUpper().Contains( matchText ) select node );
				}
				else {
					retValue.AddRange( from node in decadeNode.Children where node.Artist.Name.Contains( searchText ) select node );
				}
			}

			return( retValue );
		}

		public void ClearCurrentSearch() {
			mTreeEnumerator = null;
		}
	}
}
