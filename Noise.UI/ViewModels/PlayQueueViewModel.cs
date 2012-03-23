﻿using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Behaviours;
using Noise.UI.Behaviours.EventCommandTriggers;
using Noise.UI.Dto;
using Noise.UI.Support;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
	public class PlayQueueViewModel : ViewModelBase, IHandle<Events.PlayQueueChanged>, IHandle<Events.PlaybackTrackStarted> {
		private readonly IEventAggregator			mEventAggregator;
		private readonly IGenreProvider				mGenreProvider;
		private readonly ITagProvider				mTagProvider;
		private readonly IInternetStreamProvider	mStreamProvider;
		private readonly IPlayQueue					mPlayQueue;
		private readonly IPlayListProvider			mPlayListProvider;
		private readonly IDialogService				mDialogService;
		private int									mPlayingIndex;
		private TimeSpan							mTotalTime;
		private TimeSpan							mRemainingTime;
		private	ListViewDragDropManager<UiPlayQueueTrack>			mDragManager;
		private readonly BindableCollection<UiPlayQueueTrack>		mQueue;
		private	readonly BindableCollection<ExhaustedStrategyItem>	mExhaustedStrategies;
		private readonly BindableCollection<PlayStrategyItem>		mPlayStrategies;

		public PlayQueueViewModel( IEventAggregator eventAggregator,
								   ITagProvider tagProvider, IGenreProvider genreProvider, IInternetStreamProvider streamProvider,
								   IPlayQueue playQueue, IPlayListProvider playListProvider, IDialogService dialogService ) {
			mEventAggregator = eventAggregator;
			mGenreProvider = genreProvider;
			mStreamProvider = streamProvider;
			mTagProvider = tagProvider;
			mPlayQueue = playQueue;
			mPlayListProvider = playListProvider;
			mDialogService = dialogService;

			mQueue = new BindableCollection<UiPlayQueueTrack>();
			mPlayingIndex = -1;

			mPlayStrategies = new BindableCollection<PlayStrategyItem>{
			                                    new PlayStrategyItem( ePlayStrategy.Next, "Normal" ),
												new PlayStrategyItem( ePlayStrategy.Random, "Random" ),
												new PlayStrategyItem( ePlayStrategy.TwoFers, "2 Fers" )};

			mExhaustedStrategies = new BindableCollection<ExhaustedStrategyItem>{
												new ExhaustedStrategyItem( ePlayExhaustedStrategy.Stop, "Stop" ),
												new ExhaustedStrategyItem( ePlayExhaustedStrategy.Replay, "Replay" ),
												new ExhaustedStrategyItem( ePlayExhaustedStrategy.PlayCategory, "Play Category..." ),
												new ExhaustedStrategyItem( ePlayExhaustedStrategy.PlayFavorites, "Play Favorites" ),
												new ExhaustedStrategyItem( ePlayExhaustedStrategy.PlaySimilar, "Play Similar" ),
												new ExhaustedStrategyItem( ePlayExhaustedStrategy.PlayList, "Playlist..." ),
												new ExhaustedStrategyItem( ePlayExhaustedStrategy.PlayStream, "Radio Station..." ),
												new ExhaustedStrategyItem( ePlayExhaustedStrategy.PlayGenre, "Play Genre..." )};

			mEventAggregator.Subscribe( this );

			var configuration = NoiseSystemConfiguration.Current.RetrieveConfiguration<ExplorerConfiguration>( ExplorerConfiguration.SectionName );

			if( configuration != null ) {
				mPlayQueue.SetPlayExhaustedStrategy( configuration.PlayExhaustedStrategy, configuration.PlayExhaustedItem );
				mPlayQueue.PlayStrategy = configuration.PlayStrategy;
			}

			LoadPlayQueue();
		}

		public void Execute_OnLoaded( EventCommandParameter<object, RoutedEventArgs> args ) {
//			mDragManager = new ListViewDragDropManager<UiPlayQueueTrack>( args.EventArgs.Source as ListView );
//			mDragManager.ProcessDrop += OnDragManagerProcessDrop;
		}

		public void Execute_PlayRequested( EventCommandParameter<object, RoutedEventArgs> args ) {
			if( args.CustomParameter != null ) {
				var queuedItem  = args.CustomParameter as UiPlayQueueTrack;

				if( queuedItem != null ) {
					PlayQueueTrack( queuedItem );
				}
			}
		}

		public void Execute_SavePlayList() {
			var playList = new DbPlayList();

			if( mDialogService.ShowDialog( DialogNames.PlayListEdit, playList ) == true ) {
				mPlayListProvider.AddPlayList( new DbPlayList( playList.Name, playList.Description, mPlayQueue.PlayList.Select( track => track.Track.DbId )));
			}
		}

		public bool CanExecute_SavePlayList() {
			return(!mPlayQueue.IsQueueEmpty );
		}

		public void Execute_DeleteCommand() {
			if( SelectedItem != null ) {
				DequeueTrack( SelectedItem );
			}
		}

		public bool CanExecute_DeleteCommand() {
			return( SelectedItem != null );
		}

		private void OnDragManagerProcessDrop( object sender, ProcessDropEventArgs<UiPlayQueueTrack> args ) {
			mPlayQueue.ReorderQueueItem( args.OldIndex, args.NewIndex );
		}

		public void Execute_MoveItemUp() {
			if(( SelectedItem != null ) &&
			   ( SelectedIndex > 0 )) {
				mPlayQueue.ReorderQueueItem( SelectedIndex, SelectedIndex - 1 );
			}
		}

		[DependsUpon( "SelectedItem" )]
		public bool CanExecute_MoveItemUp() {
			return(( SelectedItem != null ) && ( SelectedIndex > 0 ));
		}

		public void Execute_MoveItemDown() {
			if( SelectedItem != null ) {
				mPlayQueue.ReorderQueueItem( SelectedIndex, SelectedIndex + 1 );
			}
		}

		[DependsUpon( "SelectedItem" )]
		public bool CanExecute_MoveItemDown() {
			return(( SelectedItem != null ) && (( SelectedIndex + 1 ) < mQueue.Count ));
		}

		public void Execute_RemoveItem() {
			if( SelectedItem != null ) {
				DequeueTrack( SelectedItem );
			}
		}

		[DependsUpon( "SelectedItem" )]
		public bool CanExecute_RemoveItem() {
			return( SelectedItem != null );
		}

		public BindableCollection<UiPlayQueueTrack> QueueList {
			get{ return( mQueue ); }
		}

		public UiPlayQueueTrack SelectedItem {
			get{ return( Get( () => SelectedItem )); }
			set{ Set( () => SelectedItem, value ); }
		}

		public int SelectedIndex {
			get{ return( Get( () => SelectedIndex )); }
			set{ Set( () => SelectedIndex, value ); }
		}

		[DependsUpon( "PlayQueueChangedFlag" )]
		public TimeSpan TotalTime {
			get{ return( mTotalTime ); }
		}

		[DependsUpon( "PlayQueueChangedFlag" )]
		public TimeSpan RemainingTime {
			get{ return( mRemainingTime ); }
		}

		public void Handle( Events.PlayQueueChanged eventArgs ) {
			Execute.OnUIThread( LoadPlayQueue );

			PlayQueueChangedFlag++;
		}

		private int PlayQueueChangedFlag {
			get{ return( Get( () => PlayQueueChangedFlag, 0 )); }
			set{ Set( () => PlayQueueChangedFlag, value  ); }
		}

		private UiPlayQueueTrack CreateUiTrack( PlayQueueTrack track ) {
			return( new UiPlayQueueTrack( track, MoveQueueItemUp, MoveQueueItemDown, DisplayQueueItemInfo, DequeueTrack, PlayQueueTrack, PlayFromQueueTrack ));
		}

		private void MoveQueueItemUp( UiPlayQueueTrack track ) {
			var index = mQueue.IndexOf( track );

			mPlayQueue.ReorderQueueItem( index, index - 1 );
		}

		private void MoveQueueItemDown( UiPlayQueueTrack track ) {
			var index = mQueue.IndexOf( track );

			mPlayQueue.ReorderQueueItem( index, index + 1 );
		}

		private void DequeueTrack( UiPlayQueueTrack track ) {
			mPlayQueue.RemoveTrack( track.QueuedTrack );
		}

		private void DisplayQueueItemInfo( UiPlayQueueTrack track ) {
			if( track.QueuedTrack.Artist != null ) {
				if( track.QueuedTrack.Album != null ) {
					mEventAggregator.Publish( new Events.AlbumFocusRequested( track.QueuedTrack.Artist.DbId, track.QueuedTrack.Album.DbId ));
				}
				else {
					mEventAggregator.Publish( new Events.ArtistFocusRequested( track.QueuedTrack.Artist.DbId ));
				}
			}
		}

		private void PlayQueueTrack( UiPlayQueueTrack track ) {
			mEventAggregator.Publish( new Events.PlayQueuedTrackRequest( track.QueuedTrack ));
		}

		private void PlayFromQueueTrack( UiPlayQueueTrack track ) {
			mPlayQueue.ContinuePlayFromTrack( track.QueuedTrack );
		}

		private void LoadPlayQueue() {
			mQueue.Clear();
			mQueue.AddRange( mPlayQueue.PlayList.Select( CreateUiTrack ));

			mTotalTime = new TimeSpan();
			mRemainingTime = new TimeSpan();

			foreach( var track in mQueue ) {
				mTotalTime = mTotalTime.Add( track.QueuedTrack.Track.Duration );

				if((!track.QueuedTrack.HasPlayed ) ||
				   ( track.QueuedTrack.IsPlaying )) {
					mRemainingTime = mRemainingTime.Add( track.QueuedTrack.Track.Duration );
				}
			}

			RaisePropertyChanged( () => TotalTime );
			RaisePropertyChanged( () => RemainingTime );
			RaiseCanExecuteChangedEvent( "CanExecute_SavePlayList" );
		}

		public void Execute_ClearQueue( object sender ) {
			mPlayQueue.ClearQueue();
		}

		[DependsUpon( "PlayQueueChangedFlag" )]
		public bool CanExecute_ClearQueue( object sender ) {
			return(!mPlayQueue.IsQueueEmpty );
		}

		public void Handle( Events.PlaybackTrackStarted eventArgs ) {
			Execute.OnUIThread( () => {
				var index = 0;

				mPlayingIndex = -1;
				mRemainingTime = new TimeSpan();

				foreach( var item in mQueue ) {
					if( item.QueuedTrack.IsPlaying ) {
						mPlayingIndex = index;
					}

					if((!item.QueuedTrack.HasPlayed ) ||
					   ( item.QueuedTrack.IsPlaying )) {
						mRemainingTime = mRemainingTime.Add( item.QueuedTrack.Track.Duration );
					}

					index++;
				}

				RaisePropertyChanged( () => PlayingIndex );
				RaisePropertyChanged( () => RemainingTime );
			});
		}

		public int PlayingIndex {
			get{ return( mPlayingIndex ); }
		}

		public BindableCollection<ExhaustedStrategyItem> ExhaustedStrategyList {
			get{ return( mExhaustedStrategies ); }
		}

		public ePlayExhaustedStrategy ExhaustedStrategy {
			get{ return( mPlayQueue.PlayExhaustedStrategy ); }
			set {
				long itemId = Constants.cDatabaseNullOid;

				if( PromptForStrategyItem( value, ref itemId )) {
					mPlayQueue.SetPlayExhaustedStrategy( value, itemId );

					var configuration = NoiseSystemConfiguration.Current.RetrieveConfiguration<ExplorerConfiguration>( ExplorerConfiguration.SectionName );

					if( configuration != null ) {
						configuration.PlayExhaustedStrategy = value;
						configuration.PlayExhaustedItem = itemId;

						NoiseSystemConfiguration.Current.Save( configuration );
					}
				}
			}
		}

		private bool PromptForStrategyItem( ePlayExhaustedStrategy strategy, ref long selectedItem ) {
			var retValue = false;
			selectedItem = Constants.cDatabaseNullOid;

			if(( strategy == ePlayExhaustedStrategy.PlayStream ) ||
			   ( strategy == ePlayExhaustedStrategy.PlayCategory ) ||
			   ( strategy == ePlayExhaustedStrategy.PlayList ) ||
			   ( strategy == ePlayExhaustedStrategy.PlayGenre )) {
				if( strategy == ePlayExhaustedStrategy.PlayList ) {
					var dialogModel = new SelectPlayListDialogModel( mPlayListProvider );

					if( mDialogService.ShowDialog( DialogNames.SelectPlayList, dialogModel ) == true ) {
						if( dialogModel.SelectedItem != null ) {
							selectedItem = dialogModel.SelectedItem.DbId;
							retValue = true;
						}
					}
				}

				if( strategy == ePlayExhaustedStrategy.PlayStream ) {
					var	dialogModel = new SelectStreamDialogModel( mStreamProvider );

					if( mDialogService.ShowDialog( DialogNames.SelectStream, dialogModel ) == true ) {
						if( dialogModel.SelectedItem != null ) {
							selectedItem = dialogModel.SelectedItem.DbId;
							retValue = true;
						}
					}
				}

				if( strategy == ePlayExhaustedStrategy.PlayCategory ) {
					var dialogModel = new SelectCategoryDialogModel( mTagProvider );

					if( mDialogService.ShowDialog( DialogNames.SelectCategory, dialogModel ) == true ) {
						if( dialogModel.SelectedItem != null ) {
							selectedItem = dialogModel.SelectedItem.DbId;
							retValue = true;
						}
					}
				}

				if( strategy == ePlayExhaustedStrategy.PlayGenre ) {
					var dialogModel = new SelectGenreDialogModel( mGenreProvider );

					if( mDialogService.ShowDialog( DialogNames.SelectGenre, dialogModel ) == true ) {
						if( dialogModel.SelectedItem != null ) {
							selectedItem = dialogModel.SelectedItem.DbId;
							retValue = true;
						}
					}
				}
			}
			else {
				retValue = true;
			}

			return( retValue );
		}

		public BindableCollection<PlayStrategyItem> PlayStrategyList {
			get{ return( mPlayStrategies ); }
		}

		public ePlayStrategy PlayStrategy {
			get{ return( mPlayQueue.PlayStrategy ); }
			set {
				mPlayQueue.PlayStrategy = value;

				var configuration = NoiseSystemConfiguration.Current.RetrieveConfiguration<ExplorerConfiguration>( ExplorerConfiguration.SectionName );

				if( configuration != null ) {
					configuration.PlayStrategy = value;

					NoiseSystemConfiguration.Current.Save( configuration );
				}
			}
		}
	}
}
