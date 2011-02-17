﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
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
		private const string					cVisualStateNormal		= "Normal";
		private const string					cVisualStateIndex		= "DisplayIndex";
		private const string					cVisualStateStrategy	= "DisplayStrategy";

		private IUnityContainer					mContainer;
		private IEventAggregator				mEvents;
		private IExplorerViewStrategy			mViewStrategy;
		private List<string>					mSearchOptions;
		private readonly LibraryExplorerFilter	mExplorerFilter;
		private DateTime						mLastExplorerRequest;
		private	readonly TimeSpan				mPlayTrackDelay;
		private bool							mEnableSortPrefixes;
		private readonly List<string>			mSortPrefixes;
		private readonly ObservableCollectionEx<UiTreeNode>		mTreeItems;
		private readonly ObservableCollectionEx<IndexNode>		mIndexItems;

		public	CollectionViewSource			TreeViewSource { get; private set; }

		[ImportMany( typeof( IExplorerViewStrategy ))]
		public IEnumerable<IExplorerViewStrategy>	ViewStrategies {get; set; }

		public LibraryExplorerViewModel() {
			mExplorerFilter = new LibraryExplorerFilter { IsEnabled = false };
			mTreeItems = new ObservableCollectionEx<UiTreeNode>();
			mIndexItems = new ObservableCollectionEx<IndexNode>();

			TreeViewSource = new CollectionViewSource { Source = mTreeItems };
			TreeViewSource.SortDescriptions.Add( new SortDescription( "Artist.SortName", ListSortDirection.Ascending ));

			mPlayTrackDelay = new TimeSpan( 0, 0, 30 );
			mLastExplorerRequest = DateTime.Now - mPlayTrackDelay;

			mSortPrefixes = new List<string>();

			VisualStateName = cVisualStateNormal;
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

				var ioc = mContainer.Resolve<IIoc>();

				ioc.ComposeParts( this );

				foreach( var strategy in ViewStrategies ) {
					strategy.Initialize( mContainer, this );
					strategy.UseSortPrefixes( mEnableSortPrefixes, mSortPrefixes );
				}

				SelectedStrategy = ( from strategy in ViewStrategies where strategy.IsDefaultStrategy select strategy ).FirstOrDefault();

				mEvents = mContainer.Resolve<IEventAggregator>();
				mEvents.GetEvent<Events.ArtistFocusRequested>().Subscribe( OnArtistRequested );
				mEvents.GetEvent<Events.AlbumFocusRequested>().Subscribe( OnAlbumRequested );
				mEvents.GetEvent<Events.PlaybackTrackStarted>().Subscribe( OnPlaybackStarted );
			}
		}

		private void ActivateStrategy( IExplorerViewStrategy strategy ) {
			mTreeItems.Clear();
			mIndexItems.Clear();
			mSearchOptions.Clear();

			if( mViewStrategy != null ) {
				mViewStrategy.Deactivate();
			}

			mViewStrategy = strategy;
			if( mViewStrategy != null ) {
				mViewStrategy.Activate();

				UpdateTree();
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
			UpdateIndex();
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
			get { return( mTreeItems ); }
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
			get{ return( Get( () => VisualStateName )); }
			set{ Set( () => VisualStateName, value ); }
		}

		public void Execute_ToggleStrategyDisplay() {
			if(( VisualStateName != cVisualStateNormal ) &&
			   ( VisualStateName != cVisualStateStrategy )) {
				VisualStateName = cVisualStateNormal;
			}

			VisualStateName = VisualStateName == cVisualStateNormal ? cVisualStateStrategy : cVisualStateNormal;
		}

		public IExplorerViewStrategy SelectedStrategy {
			get{ return( mViewStrategy ); }
			set{ 
				VisualStateName = cVisualStateNormal;

				ActivateStrategy( value );
			}
		}

		public void Execute_ToggleIndexDisplay() {
			if(( VisualStateName != cVisualStateNormal ) &&
			   ( VisualStateName != cVisualStateIndex )) {
				VisualStateName = cVisualStateNormal;
			}

			VisualStateName = VisualStateName == cVisualStateNormal ? cVisualStateIndex : cVisualStateNormal;
		}

		public IEnumerable<IndexNode> IndexData {
			get { return( mIndexItems ); }
		}

		public IndexNode SelectedIndex {
			get{ return( null ); }
			set {
				VisualStateName = cVisualStateNormal;

				if( value != null ) {
					value.DisplayNode();
				}
			}
		}
	}
}
