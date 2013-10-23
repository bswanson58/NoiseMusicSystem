using System;
using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure.Interfaces;
using Noise.TenFoot.Ui.Input;
using Noise.TenFoot.Ui.Interfaces;
using Noise.UI.ViewModels;
using Events = Noise.TenFoot.Ui.Input.Events;

namespace Noise.TenFoot.Ui.ViewModels {
	public class QueueListViewModel : PlayQueueListViewModel, IHomeScreen, IActivate, IDeactivate,
									  IHandle<InputEvent>, IHandle<Events.DequeueTrack>, IHandle<Events.DequeueAlbum> {
		private readonly IPlayQueue		mPlayQueue;
		private ePlayExhaustedStrategy	mCurrentStrategy;

		public	event EventHandler<ActivationEventArgs>		Activated = delegate { };
		public	event EventHandler<DeactivationEventArgs>	AttemptingDeactivation = delegate { };
		public	event EventHandler<DeactivationEventArgs>	Deactivated = delegate { };

		public	bool				IsActive { get; private set; }
		public	string				ScreenTitle { get; private set; }
		public	string				MenuTitle { get; private set; }
		public	string				Description { get; private set; }
		public	string				Context { get; private set; }
		public	eMainMenuCommand	MenuCommand { get; private set; }
		public	int					ScreenOrder { get; private set; }

		public QueueListViewModel( IEventAggregator eventAggregator, IPlayQueue playQueue ) :
			base( eventAggregator, playQueue ) {
			mPlayQueue = playQueue;
			mCurrentStrategy = mPlayQueue.PlayExhaustedStrategy.StrategyId;

			ScreenTitle = "Now Playing";
			MenuTitle = "Now Playing";
			Description = "display the songs being played";
			Context = string.Empty;
			SelectedIndex = -1;

			MenuCommand = eMainMenuCommand.Queue;
			ScreenOrder = 3;
		}

		private void SetSelectedItem( int index ) {
			var itemCount = QueueList.Count;

			if( itemCount > 0 ) {
				if( index < 0 ) {
					index = itemCount + index;
				}

				if( index >= itemCount ) {
					index = index % itemCount;
				}

				if( index < itemCount ) {
					SelectedItem = QueueList[index];
					SelectedIndex = index;
				}
			}
			else {
				SelectedIndex = -1;
			}
		}

		private void NextItem() {
			SetSelectedItem( SelectedIndex + 1 );
		}

		private void PreviousItem() {
			SetSelectedItem( SelectedIndex - 1 );
		}

		private void DequeueItem() {
			if( QueueList.Any()) {
				if( SelectedItem != null ) {
					EventAggregator.Publish( new Events.DequeueTrack( SelectedItem.QueuedTrack.Track ));
				}
				else {
					var playedItems = QueueList.Count( item => item.QueuedTrack.HasPlayed );

					if( playedItems > 0 ) {
                        mPlayQueue.RemovePlayedTracks();
					}
					else {
                        mPlayQueue.ClearQueue();
					}
				}

				SetSelectedItem( Math.Min( SelectedIndex, QueueList.Count - 1 ));
			}
		}

		private void ChangeExhaustedStrategy() {
			switch( mCurrentStrategy ) {
				case ePlayExhaustedStrategy.PlayFavorites:
					mCurrentStrategy = ePlayExhaustedStrategy.Replay;
					break;

				case ePlayExhaustedStrategy.Replay:
					mCurrentStrategy = ePlayExhaustedStrategy.Stop;
					break;

				case ePlayExhaustedStrategy.Stop:
					mCurrentStrategy = ePlayExhaustedStrategy.PlayFavorites;
					break;

				default:
					mCurrentStrategy = ePlayExhaustedStrategy.Stop;
					break;
			}

			mPlayQueue.SetPlayExhaustedStrategy( mCurrentStrategy, null );
		}

		public void Handle( InputEvent input ) {
			if( IsActive ) {
				switch( input.Command ) {
					case InputCommand.Up:
						PreviousItem();
						break;

					case InputCommand.Down:
						NextItem();
						break;

					case InputCommand.Enqueue:
						ChangeExhaustedStrategy();
						break;

					case InputCommand.Dequeue:
						DequeueItem();
						break;

					case InputCommand.Back:
						EventAggregator.Publish( new Events.NavigateReturn( this, true ));
						break;
				}
			}
		}

		public void Handle( Events.DequeueTrack track ) {
			var queuedTrack = ( from queued in QueueList where queued.QueuedTrack.Track.DbId == track.Track.DbId select queued ).FirstOrDefault();

			if( queuedTrack != null ) {
				DequeueTrack( queuedTrack.QueuedTrack );
			}
		}

		public void Handle( Events.DequeueAlbum album ) {
			var queuedTracks = ( from queued in QueueList where queued.QueuedTrack.Album.DbId == album.Album.DbId select queued ).ToList();

			foreach( var track in queuedTracks ) {
				DequeueTrack( track.QueuedTrack );
			}
		}

		public void Activate() {
			IsActive = true;

			Activated( this, new ActivationEventArgs());
		}

		public void Deactivate( bool close ) {
			IsActive = false;

			Deactivated( this, new DeactivationEventArgs());
		}
	}
}
