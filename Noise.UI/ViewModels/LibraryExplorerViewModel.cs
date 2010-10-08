using System;
using System.Collections.Generic;
using System.Windows;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Adapters;
using Noise.UI.Behaviours.EventCommandTriggers;
using Noise.UI.Controls;
using Noise.UI.Dto;
using Noise.UI.Support;

namespace Noise.UI.ViewModels {
	public class LibraryExplorerViewModel : ViewModelBase {
		private IUnityContainer					mContainer;
		private INoiseManager					mNoiseManager;
		private IEventAggregator				mEvents;
		private IExplorerViewStrategy			mViewStrategy;
		private ObservableCollectionEx<ExplorerTreeNode>	mTreeItems;
		private List<string>					mSearchOptions;
		private readonly LibraryExplorerFilter	mExplorerFilter;
		private DbArtist						mCurrentArtist;
		private DateTime						mLastExplorerRequest;
		private	readonly TimeSpan				mPlayTrackDelay;

		public LibraryExplorerViewModel() {
			mExplorerFilter = new LibraryExplorerFilter { IsEnabled = false };

			mPlayTrackDelay = new TimeSpan( 0, 0, 30 );
			mLastExplorerRequest = DateTime.Now - mPlayTrackDelay;
		}

		[Dependency]
		public IUnityContainer Container {
			get { return( mContainer ); }
			set {
				mContainer = value; 
				mSearchOptions = new List<string>();

				mViewStrategy = mContainer.Resolve<IExplorerViewStrategy>( "ArtistAlbum" );
				mViewStrategy.Initialize( this );

				mNoiseManager = mContainer.Resolve<INoiseManager>();

				mEvents = mContainer.Resolve<IEventAggregator>();
				mEvents.GetEvent<Events.ExplorerItemSelected>().Subscribe( OnExplorerItemSelected );
				mEvents.GetEvent<Events.ArtistFocusRequested>().Subscribe( OnArtistRequested );
				mEvents.GetEvent<Events.AlbumFocusRequested>().Subscribe( OnAlbumRequested );
				mEvents.GetEvent<Events.PlaybackTrackStarted>().Subscribe( OnPlaybackStarted );
			}
		}

		public void OnExplorerItemSelected( object item ) {
			if( item is DbArtist ) {
				mCurrentArtist = item as DbArtist;

				mEvents.GetEvent<Events.ArtistFocusRequested>().Publish( item as DbArtist );
			}
			else if( item is DbAlbum ) {
				var album = item as DbAlbum;

				mEvents.GetEvent<Events.ArtistFocusRequested>().Publish( mNoiseManager.DataProvider.GetArtistForAlbum( album ));
				mEvents.GetEvent<Events.AlbumFocusRequested>().Publish( album );
			}
		}

		private void OnArtistRequested( DbArtist artist ) {
			if(( artist != null ) &&
			   ( mCurrentArtist != null ) &&
			   ( mCurrentArtist.DbId != artist.DbId )) {
				mViewStrategy.ClearCurrentSearch();
				mViewStrategy.Search( artist.Name, new List<string> { "Artist" });
			}

			mLastExplorerRequest = DateTime.Now;
		}

		private void OnAlbumRequested( DbAlbum album ) {
			mLastExplorerRequest = DateTime.Now;
		}

		private void OnPlaybackStarted( PlayQueueTrack track ) {
			if( mLastExplorerRequest + mPlayTrackDelay < DateTime.Now ) {
				var savedTime = mLastExplorerRequest;

				mEvents.GetEvent<Events.ArtistFocusRequested>().Publish( track.Artist );
				mEvents.GetEvent<Events.AlbumFocusRequested>().Publish( track.Album );

				mLastExplorerRequest = savedTime;
			}
		}

		private void UpdateTree() {
			mTreeItems.SuspendNotification();
			mTreeItems.Clear();
			mViewStrategy.PopulateTree( mTreeItems, mExplorerFilter );
			mTreeItems.ResumeNotification();
		}

		public ObservableCollectionEx<ExplorerTreeNode> TreeData {
			get {
				if(( mTreeItems == null ) &&
				   ( mViewStrategy != null )) {
					mTreeItems = new ObservableCollectionEx<ExplorerTreeNode>();

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
	}
}
