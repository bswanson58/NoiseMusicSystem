using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Windows;
using AutoMapper;
using Caliburn.Micro;
using CuttingEdge.Conditions;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Adapters;
using Noise.UI.Dto;
using Noise.UI.Support;
using Observal.Extensions;
using ReusableBits.Mvvm.ViewModelSupport;
using Condition = CuttingEdge.Conditions.Condition;

namespace Noise.UI.ViewModels {
	[Export( typeof( IExplorerViewStrategy ))]
	public class ExplorerStrategyArtistAlbum : IExplorerViewStrategy,
											   IHandle<Events.DatabaseItemChanged> {
		internal const string					cSearchOptionDefault = "!";
		internal const string					cSearchArtists = "Artists";
		internal const string					cSearchAlbums = "Albums";
		internal const string					cSearchIgnoreCase = "Ignore Case";

		private readonly IEventAggregator		mEventAggregator;
		private readonly IResourceProvider		mResourceProvider;
		private readonly IArtistProvider		mArtistProvider;
		private readonly IAlbumProvider			mAlbumProvider;
		private readonly ITagManager			mTagManager;
		private DataTemplate					mViewTemplate;
		private	readonly Observal.Observer		mChangeObserver;
		private	ILibraryExplorerViewModel		mViewModel;
		private	bool							mUseSortPrefixes;
		private IEnumerable<string>				mSortPrefixes;
		private IEnumerator<UiArtistTreeNode>	mTreeEnumerator;
		private string							mLastSearchOptions;
		private TaskHandler						mAlbumPopulateTask;

		private IEnumerable<ViewSortStrategy>	mArtistSorts;
		private ViewSortStrategy				mCurrentArtistSort;
		private IEnumerable<ViewSortStrategy>	mAlbumSorts;
		private ViewSortStrategy				mCurrentAlbumSort;
		private Subject<ViewSortStrategy>		mAlbumSortSubject;
		private	IObservable<ViewSortStrategy>	AlbumSortChange { get { return( mAlbumSortSubject.AsObservable()); }}

		public ExplorerStrategyArtistAlbum( IEventAggregator eventAggregator, IResourceProvider resourceProvider,
											IArtistProvider artistProvider, IAlbumProvider albumProvider, ITagManager tagManager ) {
			mEventAggregator = eventAggregator;
			mResourceProvider = resourceProvider;
			mArtistProvider = artistProvider;
			mAlbumProvider = albumProvider;
			mTagManager = tagManager;

			mChangeObserver = new Observal.Observer();
			mChangeObserver.Extend( new PropertyChangedExtension()).WhenPropertyChanges( OnNodeChanged );
		}

		public void Initialize( ILibraryExplorerViewModel viewModel ) {
			Condition.Requires( viewModel ).IsNotNull();
			mViewModel = viewModel;

			var	strategies = new List<ViewSortStrategy> { new ViewSortStrategy( "Artist Name", new List<SortDescription> { new SortDescription( "Artist.Name", ListSortDirection.Ascending ) }),
														  new ViewSortStrategy( "Unprefixed Artist Name", new List<SortDescription> { new SortDescription( "Artist.SortName", ListSortDirection.Ascending ) }),
														  new ViewSortStrategy( "Genre", new List<SortDescription> { new SortDescription( "Artist.Genre", ListSortDirection.Ascending ),
																												     new SortDescription( "Artist.SortName", ListSortDirection.Ascending )}) };
			mArtistSorts = strategies;
			mCurrentArtistSort = strategies[1];

			strategies = new List<ViewSortStrategy> { new ViewSortStrategy( "Album Name", new List<SortDescription> { new SortDescription( "Album.Name", ListSortDirection.Ascending ) } ),
													  new ViewSortStrategy( "Published Year", new List<SortDescription> { new SortDescription( "Album.PublishedYear", ListSortDirection.Ascending ),
																														  new SortDescription( "Album.Name", ListSortDirection.Ascending ) })};
			mAlbumSorts = strategies;
			mCurrentAlbumSort = strategies[0];

			mAlbumSortSubject = new Subject<ViewSortStrategy>();
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
			mEventAggregator.Subscribe( this );

			if( mViewTemplate == null ) {
				mViewTemplate = mResourceProvider.RetrieveTemplate( "ArtistAlbumTemplate" ) as DataTemplate;
 			}
			Condition.Requires( mViewTemplate ).IsNotNull();

			mViewModel.SetViewTemplate( mViewTemplate );
			mViewModel.SetSearchOptions( new List<string> { cSearchOptionDefault + cSearchArtists,
															cSearchAlbums,
															cSearchOptionDefault + cSearchIgnoreCase });
		}

		public void Deactivate() {
			mEventAggregator.Unsubscribe( this );
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

		public void Handle( Events.DatabaseItemChanged eventArgs ) {
			var item = eventArgs.ItemChangedArgs.Item;

			if( item != null ) {
				if( item is DbArtist ) {
					UpdateArtist( item as DbArtist, eventArgs.ItemChangedArgs.Change );
				}
				if( item is DbAlbum ) {
					UpdateAlbum( item as DbAlbum, eventArgs.ItemChangedArgs.Change );
				}
			}
		}

		private void UpdateArtist( DbArtist artist, DbItemChanged changeReason ) {
			var treeNode = ( from UiArtistTreeNode node in mViewModel.TreeData
								where artist.DbId == node.Artist.DbId select node ).FirstOrDefault();

			switch( changeReason ) {
				case DbItemChanged.Update:
					if( treeNode != null ) {
						UpdateUiArtist( treeNode.Artist, artist );
						// The tree should autmatically resort if the sort property was changed...
						// mViewModel.SetTreeSortDescription( mCurrentArtistSort.SortDescriptions );
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

		private void UpdateAlbum( DbAlbum album, DbItemChanged changeReason ) {
			if( changeReason == DbItemChanged.Update ) {
				Execute.OnUIThread( () => {
					var treeNode = ( from UiArtistTreeNode node in mViewModel.TreeData
									 where album.Artist == node.Artist.DbId select node ).FirstOrDefault();
					if( treeNode != null ) {
						var uiAlbum = ( from UiAlbumTreeNode a in treeNode.Children
										where a.Album.DbId == album.DbId select a.Album ).FirstOrDefault();

						if( uiAlbum != null ) {
							Mapper.DynamicMap( album, uiAlbum );

							treeNode.UpdateSort();
						}
					}
				});
			}
		}

		public IEnumerable<UiTreeNode> BuildTree( IDatabaseFilter filter ) {
			Condition.Requires( mViewModel ).IsNotNull();

			var retValue = new List<UiTreeNode>();

			using( var list = mArtistProvider.GetArtistList( filter )) {
				foreach( var artist in list.List ) {
					AddArtist( retValue, artist );
				}
			}

			SetArtistSorting( mCurrentArtistSort );

			return( retValue );
		}

		public IEnumerable<IndexNode> BuildIndex( IEnumerable<UiTreeNode> artistList ) {
			var retValue = new List<IndexNode>();

			const string alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
			var obs = alphabet.ToObservable();

			obs.Subscribe( ch =>
			{
				var artist = artistList.FirstOrDefault( node => ( node is UiArtistTreeNode ) &&
																( node as UiArtistTreeNode ).Artist.SortName.StartsWith( ch.ToString( CultureInfo.InvariantCulture )));

				if(( artist != null ) &&
				   ( artist is UiArtistTreeNode )) {
					retValue.Add( new IndexNode( ch.ToString( CultureInfo.InvariantCulture ), artist as UiArtistTreeNode ));
				}
			});

			return( retValue );
		}

		private void UpdateUiArtist( UiArtist uiArtist, DbArtist artist ) {
			Mapper.DynamicMap( artist, uiArtist );
			uiArtist.DisplayGenre = mTagManager.GetGenre( artist.Genre );

			if( mUseSortPrefixes ) {
				FormatSortPrefix( uiArtist );
			}
		}

		private void AddArtist( ICollection<UiTreeNode> tree, DbArtist artist ) {
			var uiArtist = new UiArtist();
			UpdateUiArtist( uiArtist, artist );

			var parent = new UiArtistTreeNode( uiArtist, OnArtistSelect, null, FillChildren, mCurrentAlbumSort, AlbumSortChange );

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

		internal TaskHandler AlbumPopulateTask {
			get {
				if( mAlbumPopulateTask == null ) {
					mAlbumPopulateTask = new TaskHandler();
				}

				return( mAlbumPopulateTask );
			}
			set { mAlbumPopulateTask = value; }
		}

		private void FillChildren( UiArtistTreeNode parent ) {
			AlbumPopulateTask.StartTask( () => {
			                             	var albums = new List<UiAlbumTreeNode>();

											if(( parent != null ) &&
											   ( parent.Artist != null )) {
												using( var albumList = mAlbumProvider.GetAlbumList( parent.Artist.DbId )) {
													foreach( var dbAlbum in from DbAlbum album in albumList.List orderby album.Name select album ) {
														var uiAlbum = new UiAlbum { DisplayGenre = mTagManager.GetGenre( dbAlbum.Genre ) };
														Mapper.DynamicMap( dbAlbum, uiAlbum );

														albums.Add( new UiAlbumTreeNode( uiAlbum, OnAlbumSelect, OnAlbumPlay ));
													}
												}
												parent.SetChildren( albums );
											}
			                             },
										 () => { },
										 ex => NoiseLogger.Current.LogException( "ExplorerStrategyArtistAlbum:FillChildren", ex ));
		}

		private void OnArtistSelect( UiArtistTreeNode artistNode ) {
			if( artistNode.IsSelected ) {
				mEventAggregator.Publish( new Events.ArtistFocusRequested( artistNode.Artist.DbId ));
			}
		}

		private void OnAlbumSelect( UiAlbumTreeNode albumNode ) {
			if( albumNode.IsSelected ) {
				mEventAggregator.Publish( new Events.AlbumFocusRequested( albumNode.Album.Artist, albumNode.Album.DbId ));
			}
		}

		private void OnAlbumPlay( UiAlbumTreeNode albumNode ) {
			var album = mAlbumProvider.GetAlbum( albumNode.Album.DbId );

			if( album != null ) {
				GlobalCommands.PlayAlbum.Execute( album );
			}
		}

		public ArtistAlbumConfigViewModel ConfigureSortRequest() {
			return( new ArtistAlbumConfigViewModel( mArtistSorts, mCurrentArtistSort, mAlbumSorts, mCurrentAlbumSort ));
		}

		public void SortRequest( ArtistAlbumConfigViewModel request ) {
			SetArtistSorting( request.SelectedArtistSort );
			SetAlbumSorting( request.SelectedAlbumSort );
		}

		public bool CanConfigureViewSort() {
			return( true );
		}

		private void SetArtistSorting( ViewSortStrategy strategy ) {
			mViewModel.SetTreeSortDescription( strategy.SortDescriptions );

			mCurrentArtistSort = strategy;
		}

		private void SetAlbumSorting( ViewSortStrategy strategy ) {
			mCurrentAlbumSort = strategy;

			mAlbumSortSubject.OnNext( mCurrentAlbumSort );
		}

		public bool Search( string searchText, IEnumerable<string> searchOptionsList ) {
			var retValue = false;
			var searchOptions = searchOptionsList.ToList();

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
