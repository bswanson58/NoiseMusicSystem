using System;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
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
										  IHandle<Events.PlaybackStatusChanged>, IHandle<Events.TrackUserUpdate> {
		private readonly IPlayQueue		mPlayQueue;
		private readonly IRatings		mRatings;
		private IDisposable				mSubscriptions;
        protected IEventAggregator      EventAggregator {  get; }

        public	ObservableCollectionExtended<UiPlayQueueTrack>	QueueList { get; }
	    public  UiPlayQueueTrack								PlayingItem {  get; private set; }
		public	int												NextInsertIndex { get; private set; }

        public PlayQueueListViewModel( IEventAggregator eventAggregator, IPlayQueue playQueue, IRatings ratings ) {
			EventAggregator = eventAggregator;
			mPlayQueue = playQueue;
			mRatings = ratings;

			QueueList = new ObservableCollectionExtended<UiPlayQueueTrack>();
			QueueList.CollectionChanged += OnQueueChanged;
			QueueEmpty = true;

			var uiList = mPlayQueue.PlayQueue.Transform( CreateUiTrack ).AsObservableList();

            var ratingSubscription = uiList.Connect().WhenPropertyChanged( t => t.UiRating ).Subscribe( OnRatingChanged );
			var favoriteSubscription = uiList.Connect().WhenPropertyChanged( t => t.UiIsFavorite ).Subscribe( OnFavoriteChanged );
            var queueSubscription = uiList.Connect().ObserveOnDispatcher().Bind( QueueList ).Subscribe();
			// since we aren't based on ReactiveObject, we can't use a ObservableAsPropertyHelper:
			var emptySubscription = uiList.Connect().ToCollection().Select( c => c.Count == 0 ).Subscribe( b => QueueEmpty = b );  //.ToProperty( this, x => x.QueueEmpty );

            mSubscriptions = new CompositeDisposable( uiList, queueSubscription, favoriteSubscription, ratingSubscription, emptySubscription );

			EventAggregator.Subscribe( this );
		}

        private void OnRatingChanged( PropertyValue<UiPlayQueueTrack, Int16> value ) {
			if(( value?.Sender?.QueuedTrack?.Track != null ) &&
               ( value.Sender.QueuedTrack.Track.Rating != value.Value )) {
                mRatings.SetRating( value.Sender.QueuedTrack.Track, value.Sender.UiRating );
            }
        }

		private void OnFavoriteChanged( PropertyValue<UiPlayQueueTrack, bool> value ) {
            if(( value?.Sender?.QueuedTrack?.Track != null ) &&
               ( value.Sender.QueuedTrack.Track.IsFavorite != value.Value )) {
                mRatings.SetFavorite( value.Sender.QueuedTrack.Track, value.Sender.UiIsFavorite );
            }
        }

		private void OnQueueChanged( object sender, NotifyCollectionChangedEventArgs args ) {
			NextInsertIndex = mPlayQueue.IndexOfNextInsert;

			RaisePropertyChanged( () => NextInsertIndex );
        }

		public bool QueueEmpty {
			get => Get(() => QueueEmpty );
			set => Set(() => QueueEmpty, value );
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
