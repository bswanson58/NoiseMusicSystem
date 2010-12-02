using System.Windows;
using System.Windows.Controls;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Behaviours;
using Noise.UI.Behaviours.EventCommandTriggers;
using Noise.UI.Dto;
using Noise.UI.Support;

namespace Noise.UI.ViewModels {
	public class PlayQueueViewModel : ViewModelBase {
		private IUnityContainer		mContainer;
		private INoiseManager		mNoiseManager;
		private IEventAggregator	mEventAggregator;
		private int					mPlayingIndex;
		private	ListViewDragDropManager<PlayQueueTrack>					mDragManager;
		private readonly ObservableCollectionEx<PlayQueueTrack>			mPlayQueue;
		private	readonly ObservableCollectionEx<ExhaustedStrategyItem>	mExhaustedStrategies;
		private readonly ObservableCollectionEx<PlayStrategyItem>		mPlayStrategies;

		public PlayQueueViewModel() {
			mPlayQueue = new ObservableCollectionEx<PlayQueueTrack>();
			mPlayingIndex = -1;

			mPlayStrategies = new ObservableCollectionEx<PlayStrategyItem>{
			                                    new PlayStrategyItem( ePlayStrategy.Next, "Normal" ),
												new PlayStrategyItem( ePlayStrategy.Random, "Random" ),
												new PlayStrategyItem( ePlayStrategy.TwoFers, "2 Fers" )};

			mExhaustedStrategies = new ObservableCollectionEx<ExhaustedStrategyItem>{
												new ExhaustedStrategyItem( ePlayExhaustedStrategy.Stop, "Stop" ),
												new ExhaustedStrategyItem( ePlayExhaustedStrategy.Replay, "Replay" ),
												new ExhaustedStrategyItem( ePlayExhaustedStrategy.PlayFavorites, "Play Favorites" ),
												new ExhaustedStrategyItem( ePlayExhaustedStrategy.PlaySimilar, "Play Similar" ),
												new ExhaustedStrategyItem( ePlayExhaustedStrategy.PlayList, "Playlist..." ),
												new ExhaustedStrategyItem( ePlayExhaustedStrategy.PlayStream, "Radio Station..." ),
												new ExhaustedStrategyItem( ePlayExhaustedStrategy.PlayGenre, "Play Genre..." )};
		}

		[Dependency]
		public IUnityContainer Container {
			get { return( mContainer ); }
			set {
				mContainer = value;

				mEventAggregator = mContainer.Resolve<IEventAggregator>();
				mNoiseManager = mContainer.Resolve<INoiseManager>();
				mEventAggregator.GetEvent<Events.PlayQueueChanged>().Subscribe( OnPlayQueueChanged );
				mEventAggregator.GetEvent<Events.PlaybackTrackStarted>().Subscribe( OnTrackStarted );

				var	systemConfig = mContainer.Resolve<ISystemConfiguration>();
				var configuration = systemConfig.RetrieveConfiguration<ExplorerConfiguration>( ExplorerConfiguration.SectionName );

				if( configuration != null ) {
					mNoiseManager.PlayQueue.SetPlayExhaustedStrategy( configuration.PlayExhaustedStrategy, configuration.PlayExhaustedItem );
					mNoiseManager.PlayQueue.PlayStrategy = configuration.PlayStrategy;
				}

				LoadPlayQueue();
			}
		}

		public void Execute_OnLoaded( EventCommandParameter<object, RoutedEventArgs> args ) {
			mDragManager = new ListViewDragDropManager<PlayQueueTrack>( args.EventArgs.Source as ListView );
			mDragManager.ProcessDrop += OnDragManagerProcessDrop;
		}

		public void Execute_PlayRequested( EventCommandParameter<object, RoutedEventArgs> args ) {
			if( args.CustomParameter != null ) {
				mEventAggregator.GetEvent<Events.PlayRequested>().Publish( args.CustomParameter as PlayQueueTrack );
			}
		}

		public void Execute_SavePlayList() {
			var	dialogService = mContainer.Resolve<IDialogService>();
			var playList = new DbPlayList();

			if( dialogService.ShowDialog( DialogNames.PlayListEdit, playList ) == true ) {
				mNoiseManager.PlayListMgr.Create( mNoiseManager.PlayQueue.PlayList, playList.Name, playList.Description );
			}
		}

		public bool CanExecute_SavePlayList() {
			return(!mNoiseManager.PlayQueue.IsQueueEmpty );
		}

		public void Execute_DeleteCommand() {
			if( SelectedItem != null ) {
				mNoiseManager.PlayQueue.RemoveTrack( SelectedItem );
			}
		}

		public bool CanExecute_DeleteCommand() {
			return( SelectedItem != null );
		}

		private void OnDragManagerProcessDrop( object sender, ProcessDropEventArgs<PlayQueueTrack> args ) {
			mNoiseManager.PlayQueue.ReorderQueueItem( args.OldIndex, args.NewIndex );
		}

		public void Execute_MoveItemUp() {
			if(( SelectedItem != null ) &&
			   ( SelectedIndex > 0 )) {
				mNoiseManager.PlayQueue.ReorderQueueItem( SelectedIndex, SelectedIndex - 1 );
			}
		}

		[DependsUpon( "SelectedItem" )]
		public bool CanExecute_MoveItemUp() {
			return(( SelectedItem != null ) && ( SelectedIndex > 0 ));
		}

		public void Execute_MoveItemDown() {
			if( SelectedItem != null ) {
				mNoiseManager.PlayQueue.ReorderQueueItem( SelectedIndex, SelectedIndex + 1 );
			}
		}

		[DependsUpon( "SelectedItem" )]
		public bool CanExecute_MoveItemDown() {
			return(( SelectedItem != null ) && (( SelectedIndex + 1 ) < mPlayQueue.Count ));
		}

		public void Execute_RemoveItem() {
			if( SelectedItem != null ) {
				mNoiseManager.PlayQueue.RemoveTrack( SelectedItem );
			}
		}

		[DependsUpon( "SelectedItem" )]
		public bool CanExecute_RemoveItem() {
			return( SelectedItem != null );
		}

		public ObservableCollectionEx<PlayQueueTrack> QueueList {
			get{ return( mPlayQueue ); }
		}

		public PlayQueueTrack SelectedItem {
			get{ return( Get( () => SelectedItem )); }
			set{ Set( () => SelectedItem, value ); }
		}

		public int SelectedIndex {
			get{ return( Get( () => SelectedIndex )); }
			set{ Set( () => SelectedIndex, value ); }
		}

		private void OnPlayQueueChanged( IPlayQueue playQueue ) {
			BeginInvoke( LoadPlayQueue );

			PlayQueueChangedFlag++;
		}

		private int PlayQueueChangedFlag {
			get{ return( Get( () => PlayQueueChangedFlag, 0 )); }
			set{ Set( () => PlayQueueChangedFlag, value  ); }
		}

		private void LoadPlayQueue() {
			mPlayQueue.Clear();
			mPlayQueue.AddRange( mNoiseManager.PlayQueue.PlayList );

			RaiseCanExecuteChangedEvent( "CanExecute_SavePlayList" );
		}

		public void Execute_ClearQueue( object sender ) {
			if( mNoiseManager != null ) {
				mNoiseManager.PlayQueue.ClearQueue();
			}
		}

		[DependsUpon( "PlayQueueChangedFlag" )]
		public bool CanExecute_ClearQueue( object sender ) {
			var retValue = true;

			if( mNoiseManager != null ) {
				retValue = !mNoiseManager.PlayQueue.IsQueueEmpty;
			}

			return( retValue );
		}

		private void OnTrackStarted( PlayQueueTrack track ) {
			BeginInvoke( () => {
				var index = 0;

				mPlayingIndex = -1;

				foreach( var item in mPlayQueue ) {
					if( item.IsPlaying ) {
						mPlayingIndex = index;

						break;
					}

					index++;
				}

				RaisePropertyChanged( () => PlayingIndex );
			});
		}

		public int PlayingIndex {
			get{ return( mPlayingIndex ); }
		}

		public ObservableCollectionEx<ExhaustedStrategyItem> ExhaustedStrategyList {
			get{ return( mExhaustedStrategies ); }
		}

		public ePlayExhaustedStrategy ExhaustedStrategy {
			get{ return( mNoiseManager.PlayQueue.PlayExhaustedStrategy ); }
			set {
				long itemId = Constants.cDatabaseNullOid;

				if( PromptForStrategyItem( value, ref itemId )) {
					mNoiseManager.PlayQueue.SetPlayExhaustedStrategy( value, itemId );

					var	systemConfig = mContainer.Resolve<ISystemConfiguration>();
					var configuration = systemConfig.RetrieveConfiguration<ExplorerConfiguration>( ExplorerConfiguration.SectionName );

					if( configuration != null ) {
						configuration.PlayExhaustedStrategy = value;
						configuration.PlayExhaustedItem = itemId;

						systemConfig.Save( configuration );
					}
				}
			}
		}

		private bool PromptForStrategyItem( ePlayExhaustedStrategy strategy, ref long selectedItem ) {
			var retValue = false;
			selectedItem = Constants.cDatabaseNullOid;

			if(( strategy == ePlayExhaustedStrategy.PlayStream ) ||
			   ( strategy == ePlayExhaustedStrategy.PlayList ) ||
			   ( strategy == ePlayExhaustedStrategy.PlayGenre )) {
				var	dialogService = mContainer.Resolve<IDialogService>();

				if( strategy == ePlayExhaustedStrategy.PlayList ) {
					var dialogModel = new SelectPlayListDialogModel( mContainer );

					if( dialogService.ShowDialog( DialogNames.SelectPlayList, dialogModel ) == true ) {
						if( dialogModel.SelectedItem != null ) {
							selectedItem = dialogModel.SelectedItem.DbId;
							retValue = true;
						}
					}
				}

				if( strategy == ePlayExhaustedStrategy.PlayStream ) {
					var	dialogModel = new SelectStreamDialogModel( mContainer );

					if( dialogService.ShowDialog( DialogNames.SelectStream, dialogModel ) == true ) {
						if( dialogModel.SelectedItem != null ) {
							selectedItem = dialogModel.SelectedItem.DbId;
							retValue = true;
						}
					}
				}

				if( strategy == ePlayExhaustedStrategy.PlayGenre ) {
					var dialogModel = new SelectGenreDialogModel( mContainer );

					if( dialogService.ShowDialog( DialogNames.SelectGenre, dialogModel ) == true ) {
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

		public ObservableCollectionEx<PlayStrategyItem> PlayStrategyList {
			get{ return( mPlayStrategies ); }
		}

		public ePlayStrategy PlayStrategy {
			get{ return( mNoiseManager.PlayQueue.PlayStrategy ); }
			set {
				mNoiseManager.PlayQueue.PlayStrategy = value;

				var	systemConfig = mContainer.Resolve<ISystemConfiguration>();
				var configuration = systemConfig.RetrieveConfiguration<ExplorerConfiguration>( ExplorerConfiguration.SectionName );

				if( configuration != null ) {
					configuration.PlayStrategy = value;

					systemConfig.Save( configuration );
				}
			}
		}
	}
}
