using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using GongSolutions.Wpf.DragDrop;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Behaviours.EventCommandTriggers;
using Noise.UI.Dto;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
	public class PlayQueueListViewModel : AutomaticCommandBase, IDropTarget, IHandle<Events.PlayQueueChanged>, IHandle<Events.PlaybackStatusChanged> {
		private readonly IEventAggregator			mEventAggregator;
		private readonly IPlayQueue					mPlayQueue;
		private UiPlayQueueTrack					mPlayingItem;
		private readonly BindableCollection<UiPlayQueueTrack>		mQueue;

		public PlayQueueListViewModel( IEventAggregator eventAggregator, IPlayQueue playQueue ) {
			mEventAggregator = eventAggregator;
			mPlayQueue = playQueue;

			mQueue = new BindableCollection<UiPlayQueueTrack>();

			LoadPlayQueue();
			mEventAggregator.Subscribe( this );
		}

		protected IEventAggregator EventAggregator {
			get{ return( mEventAggregator ); }
		}

		public BindableCollection<UiPlayQueueTrack> QueueList {
			get{ return( mQueue ); }
		}

		public UiPlayQueueTrack SelectedItem {
			get{ return( Get( () => SelectedItem )); }
			set{ Set( () => SelectedItem, value ); }
		}

		public UiPlayQueueTrack PlayingItem {
			get{ return( mPlayingItem ); }
		}

		public void DragOver( DropInfo dropInfo ) {
			if(( dropInfo.Data is UiPlayQueueTrack ) &&
			   ( dropInfo.TargetItem is UiPlayQueueTrack )) {
				dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
				dropInfo.Effects = DragDropEffects.Copy;
			}
		}

		public void Drop( DropInfo dropInfo ) {
			var draggedItem = dropInfo.Data as UiPlayQueueTrack;
			var targetIndex = dropInfo.InsertIndex;

			if( draggedItem != null ) {
				var sourceIndex = mQueue.IndexOf( draggedItem );

				if( targetIndex > sourceIndex ) {
					targetIndex--;
				}

				mPlayQueue.ReorderQueueItem( sourceIndex, targetIndex );
			}
		}

		public void Execute_PlayRequested( EventCommandParameter<object, RoutedEventArgs> args ) {
			if( args.CustomParameter != null ) {
				var queuedItem  = args.CustomParameter as UiPlayQueueTrack;

				if( queuedItem != null ) {
					PlayQueueTrack( queuedItem );
				}
			}
		}

		public void Execute_DeleteCommand() {
			if( SelectedItem != null ) {
				DequeueTrack( SelectedItem );
			}
		}

		public bool CanExecute_DeleteCommand() {
			return( SelectedItem != null );
		}

		public void Handle( Events.PlayQueueChanged eventArgs ) {
			Execute.OnUIThread( LoadPlayQueue );

			PlayQueueChangedFlag++;
		}

		public void Handle( Events.PlaybackStatusChanged eventArgs ) {
			Execute.OnUIThread( () => {
				mPlayingItem = mQueue.FirstOrDefault( item => item.QueuedTrack.IsPlaying );

				SelectedItem = null;
				RaisePropertyChanged( () => PlayingItem );
			});
		}

		private int PlayQueueChangedFlag {
			get{ return( Get( () => PlayQueueChangedFlag, 0 )); }
			set{ Set( () => PlayQueueChangedFlag, value  ); }
		}

		private UiPlayQueueTrack CreateUiTrack( PlayQueueTrack track ) {
			return( new UiPlayQueueTrack( track, MoveQueueItemUp, MoveQueueItemDown, DisplayQueueItemInfo, DequeueTrack,
												 PlayQueueTrack, PlayFromQueueTrack, SetFavorite, SetRating ));
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
			DequeueTrack( track.QueuedTrack );
		}

		protected void DequeueTrack( PlayQueueTrack track ) {
			mPlayQueue.RemoveTrack( track );
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

		private void SetFavorite( UiPlayQueueTrack track ) {
			GlobalCommands.SetFavorite.Execute( track.QueuedTrack.IsStream ? new SetFavoriteCommandArgs( track.QueuedTrack.Stream.DbId, track.IsFavorite ) :
																			 new SetFavoriteCommandArgs( track.QueuedTrack.Track.DbId, track.IsFavorite ));
		}

		private void SetRating( UiPlayQueueTrack track ) {
			GlobalCommands.SetRating.Execute( track.QueuedTrack.IsStream ? new SetRatingCommandArgs( track.QueuedTrack.Stream.DbId, track.Rating ) :
																		   new SetRatingCommandArgs( track.QueuedTrack.Track.DbId, track.Rating ));
		}

		private void LoadPlayQueue() {
			UpdateQueueList( mPlayQueue.PlayList );
		}

		private void UpdateQueueList( IEnumerable<PlayQueueTrack> playQueueList ) {
			// Reconcile the local list with the updated list with the least amount of changes to allow the UI to indicate the changed items.
			var newList = playQueueList.ToList();

			if( newList.Any()) {
				// Get off of the UI thread since we are potential going to sleep.
				Task.Factory.StartNew( () => {
					lock( mQueue ) {
						// If there are any deletions, set the delete flag, wait a sec, and then delete them to allow the ui to animate their removal.
						var deleteList = ( from track in mQueue where newList.FirstOrDefault( t => t.Uid == track.QueuedTrack.Uid ) == null select track ).ToList();
						if( deleteList.Any()) {
							foreach( var track in deleteList ) {
								track.IsDeleting = true;
							}
							Thread.Sleep( new TimeSpan( 0, 0, 0, 0, 750 ));
						}

						var removeList = ( from track in mQueue where track.IsDeleting select track ).ToList();
						foreach( var track in removeList ) {
							mQueue.Remove( track );
						}

						var addList = ( from track in newList where mQueue.FirstOrDefault( t => t.QueuedTrack.Uid == track.Uid ) == null select track ).ToList();
						foreach( var track in addList ) {
							mQueue.Insert( newList.IndexOf( track ), CreateUiTrack( track ));
						}

						// finally insure that the order matches.
						for( var index = 0; index < newList.Count; index++ ) {
							var newTrack = newList[index];

							if( mQueue[index].QueuedTrack.Uid != newTrack.Uid ) {
								var oldTrack = mQueue.FirstOrDefault( track => track.QueuedTrack.Uid == newTrack.Uid );

								if( oldTrack != null ) {
									mQueue.Remove( oldTrack );
									mQueue.Insert( index, oldTrack );
								}
							}
						}
					}
				} );
			}
			else {
				mQueue.Clear();
			}
		}
	}
}
