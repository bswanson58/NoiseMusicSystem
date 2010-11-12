using System.Windows;
using System.Windows.Controls;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Behaviours;
using Noise.UI.Behaviours.EventCommandTriggers;
using Noise.UI.Support;

namespace Noise.UI.ViewModels {
	public class PlayQueueViewModel : ViewModelBase {
		private IUnityContainer		mContainer;
		private INoiseManager		mNoiseManager;
		private IEventAggregator	mEventAggregator;
		private int					mPlayingIndex;
		private	ListViewDragDropManager<PlayQueueTrack>			mDragManager;
		private readonly ObservableCollectionEx<PlayQueueTrack>	mPlayQueue;

		public PlayQueueViewModel() {
			mPlayQueue = new ObservableCollectionEx<PlayQueueTrack>();
			mPlayingIndex = -1;
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

		public ObservableCollectionEx<PlayQueueTrack> QueueList {
			get{ return( mPlayQueue ); }
		}

		public PlayQueueTrack SelectedItem {
			get{ return( Get( () => SelectedItem )); }
			set{ Set( () => SelectedItem, value ); }
		}

		private void OnPlayQueueChanged( IPlayQueue playQueue ) {
			BeginInvoke( LoadPlayQueue );
		}

		private void LoadPlayQueue() {
			mPlayQueue.Clear();
			mPlayQueue.AddRange( mNoiseManager.PlayQueue.PlayList );

			RaiseCanExecuteChangedEvent( "CanExecute_SavePlayList" );
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
	}
}
