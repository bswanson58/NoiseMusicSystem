using System;
using System.Collections.Generic;
using System.Windows;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Support;
using Noise.UI.Adapters;
using Noise.UI.Behaviours.EventCommandTriggers;
using Noise.UI.Controls;
using Noise.UI.Dto;
using Noise.UI.Support;

namespace Noise.UI.ViewModels {
	public class LibraryExplorerViewModel : ViewModelBase {
		private const string					cVisualStateNormal	= "Normal";
		private const string					cVisualStateIndex	= "DisplayIndex";

		private IUnityContainer					mContainer;
		private IEventAggregator				mEvents;
		private IExplorerViewStrategy			mViewStrategy;
		private List<string>					mSearchOptions;
		private readonly LibraryExplorerFilter	mExplorerFilter;
		private DateTime						mLastExplorerRequest;
		private	readonly TimeSpan				mPlayTrackDelay;
		private string							mVisualState;
		private bool							mEnableSortPrefixes;
		private readonly List<string>			mSortPrefixes;
		private ObservableCollectionEx<UiTreeNode>		mTreeItems;
		private ObservableCollectionEx<IndexNode>		mIndexItems;

		public LibraryExplorerViewModel() {
			mExplorerFilter = new LibraryExplorerFilter { IsEnabled = false };

			mPlayTrackDelay = new TimeSpan( 0, 0, 30 );
			mLastExplorerRequest = DateTime.Now - mPlayTrackDelay;

			mSortPrefixes = new List<string>();

			mVisualState = cVisualStateNormal;
		}

		[Dependency]
		public IUnityContainer Container {
			get { return( mContainer ); }
			set {
				mContainer = value; 
				mSearchOptions = new List<string>();

				var	systemConfig = mContainer.Resolve<ISystemConfiguration>();
				var configuration = systemConfig.RetrieveConfiguration<ExplorerConfiguration>( ExplorerConfiguration.SectionName );
				if( configuration != null ) {
					mEnableSortPrefixes = configuration.EnableSortPrefixes;

					if( mEnableSortPrefixes ) {
						mSortPrefixes.AddRange( configuration.SortPrefixes.Split( '|' ));
					}
				}

				mViewStrategy = mContainer.Resolve<IExplorerViewStrategy>( "ArtistAlbum" );
//				mViewStrategy = mContainer.Resolve<IExplorerViewStrategy>( "DecadeArtist" );
				mViewStrategy.Initialize( this );
				mViewStrategy.UseSortPrefixes( mEnableSortPrefixes, mSortPrefixes );

				mEvents = mContainer.Resolve<IEventAggregator>();
				mEvents.GetEvent<Events.ArtistFocusRequested>().Subscribe( OnArtistRequested );
				mEvents.GetEvent<Events.AlbumFocusRequested>().Subscribe( OnAlbumRequested );
				mEvents.GetEvent<Events.PlaybackTrackStarted>().Subscribe( OnPlaybackStarted );
			}
		}

		private void OnArtistRequested( DbArtist artist ) {
			mLastExplorerRequest = DateTime.Now;
		}

		private void OnAlbumRequested( DbAlbum album ) {
			mLastExplorerRequest = DateTime.Now;
		}

		private void OnPlaybackStarted( PlayQueueTrack track ) {
			if( mLastExplorerRequest + mPlayTrackDelay < DateTime.Now ) {
				var savedTime = mLastExplorerRequest;

				if( track.Artist != null ) {
					mEvents.GetEvent<Events.ArtistFocusRequested>().Publish( track.Artist );
				}
				if( track.Album != null ) {
					mEvents.GetEvent<Events.AlbumFocusRequested>().Publish( track.Album );
				}

				mLastExplorerRequest = savedTime;
			}
		}

		private void UpdateTree() {
		 	PopulateTree( BuildTree());
		}

		private IEnumerable<UiTreeNode> BuildTree() {
			return( mViewStrategy.BuildTree( mExplorerFilter ));
		}

		private void PopulateTree( IEnumerable<UiTreeNode> newNodes ) {
			mTreeItems.SuspendNotification();
			mTreeItems.Clear();
		 	mTreeItems.AddRange( newNodes );
			mTreeItems.ResumeNotification();
		}

		private void UpdateIndex() {
			if(( mViewStrategy != null ) &&
			   ( mTreeItems != null )) {
				mIndexItems.SuspendNotification();
				mIndexItems.Clear();
				mIndexItems.AddRange( mViewStrategy.BuildIndex( mTreeItems ));
				mIndexItems.ResumeNotification();
			}
		}

		public ObservableCollectionEx<UiTreeNode> TreeData {
			get {
				if(( mTreeItems == null ) &&
				   ( mViewStrategy != null )) {
					mTreeItems = new ObservableCollectionEx<UiTreeNode>();

					UpdateTree();
				}

				return( mTreeItems );
			}
		}

		public DataTemplate TreeViewItemTemplate {
			get{ return( Get( () => TreeViewItemTemplate )); }
			set{ Set( () => TreeViewItemTemplate, value ); }
		}

		public string SearchText {
			get{ return( Get( () => SearchText )); }
			set {
				Set( () => SearchText, value );

				mViewStrategy.ClearCurrentSearch();
			}
		}

		public void Execute_Filter() {
			if( mContainer != null ) {
				var	dialogService = mContainer.Resolve<IDialogService>();

				if( dialogService.ShowDialog( DialogNames.LibraryExplorerFilter, mExplorerFilter ) == true ) {
					UpdateTree();
				}
			}
		}

		public void Execute_Search( EventCommandParameter<object, RoutedEventArgs> args ) {
			var searchArgs = args.EventArgs as SearchEventArgs;

			if( searchArgs != null ) {
				mViewStrategy.Search( searchArgs.Keyword, searchArgs.Sections );
			}
		}

		[DependsUpon( "SearchText" )]
		public bool CanExecute_Search() {
			return(!string.IsNullOrWhiteSpace( SearchText ));
		}

		public List<string> SearchOptions {
			get{ return( mSearchOptions ); }
		}

		public bool HaveSearchOptions {
			get{ return(( mSearchOptions != null ) && ( mSearchOptions.Count > 0 )); }
		}

		public string VisualStateName {
			get{ return( mVisualState ); }
		}

		public void Execute_ToggleIndexDisplay() {
			mVisualState = mVisualState == cVisualStateNormal ? cVisualStateIndex : cVisualStateNormal;

			RaisePropertyChanged( () => VisualStateName );
		}

		public IEnumerable<IndexNode> IndexData {
			get {
				if( mIndexItems == null ) {
					mIndexItems = new ObservableCollectionEx<IndexNode>();
					UpdateIndex();
				}

				return( mIndexItems );
			}
		}

		public IndexNode SelectedIndex {
			get{ return( null ); }
			set {
				mVisualState = cVisualStateNormal;

				if( value != null ) {
					value.DisplayNode();
				}

				RaisePropertyChanged( () => VisualStateName );
			}
		}
	}
}
