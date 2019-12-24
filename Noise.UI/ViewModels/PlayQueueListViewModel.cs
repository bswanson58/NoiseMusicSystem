using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using DynamicData;
using DynamicData.Binding;
using GongSolutions.Wpf.DragDrop;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Behaviours.EventCommandTriggers;
using Noise.UI.Dto;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
	public class PlayQueueListViewModel : AutomaticCommandBase, IDropTarget, IDisposable,
										  IHandle<Events.PlayQueueChanged>, IHandle<Events.PlaybackStatusChanged>, IHandle<Events.TrackUserUpdate> {
		private readonly IPlayQueue		mPlayQueue;
		private readonly IRatings		mRatings;
		private IDisposable				mSubscriptions;
        protected IEventAggregator      EventAggregator {  get; }

        public	ObservableCollectionExtended<UiPlayQueueTrack>	QueueList { get; }
	    public  UiPlayQueueTrack								PlayingItem {  get; private set; }

        public PlayQueueListViewModel( IEventAggregator eventAggregator, IPlayQueue playQueue, IRatings ratings ) {
			EventAggregator = eventAggregator;
			mPlayQueue = playQueue;
			mRatings = ratings;

			QueueList = new ObservableCollectionExtended<UiPlayQueueTrack>();

			var uiList = mPlayQueue.PlayQueue.Transform( CreateUiTrack ).AsObservableList();

            var ratingSubscription = uiList.Connect().WhenPropertyChanged( t => t.UiRating ).Subscribe( OnRatingChanged );
			var favoriteSubscription = uiList.Connect().WhenPropertyChanged( t => t.UiIsFavorite ).Subscribe( OnFavoriteChanged );
            var queueSubscription = uiList.Connect().ObserveOnDispatcher().Bind( QueueList ).Subscribe();

			var deleteSubscription = mPlayQueue.PlaySource.Preview().OnItemRemoved( OnTrackRemoved ).Subscribe();

            mSubscriptions = new CompositeDisposable( uiList, queueSubscription, deleteSubscription, favoriteSubscription, ratingSubscription );

			EventAggregator.Subscribe( this );
		}

        private void OnRatingChanged( PropertyValue<UiPlayQueueTrack, Int16> value ) {
            mRatings.SetRating( value.Sender.QueuedTrack.Track, value.Sender.UiRating );
        }

		private void OnFavoriteChanged( PropertyValue<UiPlayQueueTrack, bool> value ) {
			mRatings.SetFavorite( value.Sender.QueuedTrack.Track, value.Sender.UiIsFavorite );
        }

		private async void OnTrackRemoved( PlayQueueTrack track ) {
			await Task.Run( () => {
                var uiTrack = QueueList.FirstOrDefault( i => i.QueuedTrack.Track.DbId.Equals( track.Track.DbId ));

                if( uiTrack != null ) {
                    uiTrack.IsDeleting = true;
					Task.Delay( TimeSpan.FromSeconds( 1 ));
                }
            } );
        }

	    public UiPlayQueueTrack SelectedItem {
			get{ return( Get( () => SelectedItem )); }
			set{ Set( () => SelectedItem, value ); }
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
				var sourceIndex = QueueList.IndexOf( draggedItem );

				if( targetIndex > sourceIndex ) {
					targetIndex--;
				}

				mPlayQueue.ReorderQueueItem( sourceIndex, targetIndex );
			}
		}

		public void Execute_PlayRequested( EventCommandParameter<object, RoutedEventArgs> args ) {
            if( args.CustomParameter is UiPlayQueueTrack queuedItem ) {
                PlayQueueTrack( queuedItem );
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
			PlayQueueChangedFlag++;
		}

		public void Handle( Events.PlaybackStatusChanged eventArgs ) {
			Execute.OnUIThread( () => {
				PlayingItem = QueueList.FirstOrDefault( item => item.QueuedTrack.IsPlaying );

				SelectedItem = null;
				RaisePropertyChanged( () => PlayingItem );
			});
		}

		public void Handle( Events.TrackUserUpdate args ) {
			foreach( var track in QueueList ) {
				if((!track.QueuedTrack.IsStream ) &&
				   ( track.QueuedTrack.Track.DbId == args.Track.DbId )) {
					track.UiIsFavorite = args.Track.IsFavorite;
					track.UiRating = args.Track.Rating;
				}
			}
		}

		private int PlayQueueChangedFlag {
			get{ return( Get( () => PlayQueueChangedFlag, 0 )); }
			set{ Set( () => PlayQueueChangedFlag, value  ); }
		}

		private UiPlayQueueTrack CreateUiTrack( PlayQueueTrack track ) {
			return( new UiPlayQueueTrack( track, MoveQueueItemUp, MoveQueueItemDown, DisplayQueueItemInfo, DequeueTrack,
												 PlayQueueTrack, PlayFromQueueTrack, PromoteSuggestion ));
		}

		private void MoveQueueItemUp( UiPlayQueueTrack track ) {
			var index = QueueList.IndexOf( track );

			mPlayQueue.ReorderQueueItem( index, index - 1 );
		}

		private void MoveQueueItemDown( UiPlayQueueTrack track ) {
			var index = QueueList.IndexOf( track );

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
					EventAggregator.PublishOnUIThread( new Events.AlbumFocusRequested( track.QueuedTrack.Artist.DbId, track.QueuedTrack.Album.DbId ));
				}
				else {
					EventAggregator.PublishOnUIThread( new Events.ArtistFocusRequested( track.QueuedTrack.Artist.DbId ));
				}
			}
		}

		private void PlayQueueTrack( UiPlayQueueTrack track ) {
			EventAggregator.PublishOnUIThread( new Events.PlayQueuedTrackRequest( track.QueuedTrack ));
		}

		private void PlayFromQueueTrack( UiPlayQueueTrack track ) {
			mPlayQueue.ContinuePlayFromTrack( track.QueuedTrack );
		}

        private void PromoteSuggestion( UiPlayQueueTrack track ) {
            mPlayQueue.PromoteTrackFromStrategy( track.QueuedTrack );

            track.NotifyPlayStrategyChanged();
        }

		public void Dispose() {
			mSubscriptions?.Dispose();
			mSubscriptions = null;

			EventAggregator.Unsubscribe( this );
        }
    }
}
