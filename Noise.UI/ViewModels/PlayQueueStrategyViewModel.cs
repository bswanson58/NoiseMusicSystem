using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Views;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
	public class PlayQueueStrategyViewModel : AutomaticPropertyBase,
											  IHandle<Events.PlayQueueChanged>, IHandle<Events.PlayStrategyChanged>,
                                              IHandle<Events.PlaybackTrackStarted>, IHandle<Events.PlaybackStopped> {
		private readonly IEventAggregator	mEventAggregator;
		private readonly IPlayQueue         mPlayQueue;
		private readonly IDialogService     mDialogService;
		private PlayQueueTrack				mPlayingItem;

		public	DelegateCommand				StartStrategy { get; }
		public	DelegateCommand				PlayingFocus { get; }
		public	DelegateCommand				ConfigureStrategy { get; }
		public	DelegateCommand				SuggestedTrackPicker { get; }

		public PlayQueueStrategyViewModel( IEventAggregator eventAggregator, IPlayQueue playQueue, IDialogService dialogService ) {
			mEventAggregator = eventAggregator;
			mPlayQueue = playQueue;
			mDialogService = dialogService;

			StartStrategy = new DelegateCommand( OnStartStrategy, CanStartStrategy  );
			PlayingFocus = new DelegateCommand( OnPlayingFocus, CanPlayingFocus );
			ConfigureStrategy = new DelegateCommand( OnConfigureStrategy );
			SuggestedTrackPicker = new DelegateCommand( OnPickTracks );

			SetStrategyDescriptions();

			mEventAggregator.Subscribe( this );
		}

		public void Handle( Events.PlayQueueChanged eventArgs ) {
			StartStrategy.RaiseCanExecuteChanged();
		}

		public void Handle( Events.PlayStrategyChanged args ) {
			SetStrategyDescriptions();
        }

		public void Handle( Events.PlaybackTrackStarted args ) {
			mPlayingItem = args.Track;

			PlayingFocus.RaiseCanExecuteChanged();
        }

		public void Handle( Events.PlaybackStopped args ) {
			mPlayingItem = null;

			PlayingFocus.RaiseCanExecuteChanged();
        }

		private void SetStrategyDescriptions() {
            PlayStrategyDescription = mPlayQueue.PlayStrategy?.ConfiguredDescription;
            PlayExhaustedDescription = mPlayQueue.PlayExhaustedStrategy?.Description;
        }

		private void OnStartStrategy() {
			mPlayQueue.StartPlayStrategy();
		}

		private bool CanStartStrategy() {
			return( mPlayQueue.CanStartPlayStrategy );
		}

		public string PlayExhaustedDescription {
			get {  return( Get( () => PlayExhaustedDescription )); }
			set {  Set( () => PlayExhaustedDescription, value ); }
		}

		public string PlayStrategyDescription {
			get {  return( Get( () => PlayStrategyDescription )); }
			set {  Set( () => PlayStrategyDescription, value ); }
		}

		private void OnPlayingFocus() {
			if( mPlayingItem?.Album != null ) {
                mEventAggregator.PublishOnUIThread( new Events.AlbumFocusRequested( mPlayingItem.Artist.DbId, mPlayingItem.Album.DbId, true ));
            }
        }

		private bool CanPlayingFocus() {
			return  mPlayingItem != null;
        }

		private void OnConfigureStrategy() {
			mDialogService.ShowDialog( nameof( PlayStrategyDialog ), new DialogParameters(), result => {
				if( result.Result == ButtonResult.OK ) {
					StartStrategy.RaiseCanExecuteChanged();
                }
            });
		}

		private void OnPickTracks() {
			mDialogService.ShowDialog( nameof( ExhaustedPlayPickerView ), new DialogParameters(), result => { });
        }
	}
}
