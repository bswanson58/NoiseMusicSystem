using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Support;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
	public class PlayQueueStrategyViewModel : AutomaticCommandBase,
											  IHandle<Events.PlayQueueChanged>, IHandle<Events.PlayStrategyChanged> {
		private readonly IEventAggregator                           mEventAggregator;
		private readonly IPlayQueue                                 mPlayQueue;
		private readonly IDialogService                             mDialogService;
		private readonly PlayStrategyDialogModel					mConfigurationDialog;

		public PlayQueueStrategyViewModel( IEventAggregator eventAggregator, IPlayQueue playQueue, IDialogService dialogService, PlayStrategyDialogModel configurationDialog ) {
			mEventAggregator = eventAggregator;
			mPlayQueue = playQueue;
			mDialogService = dialogService;
			mConfigurationDialog = configurationDialog;

			SetStrategyDescriptions();

			mEventAggregator.Subscribe( this );
		}

		public void Handle( Events.PlayQueueChanged eventArgs ) {
			RaiseCanExecuteChangedEvent( "CanExecute_StartStrategy" );
		}

		public void Handle( Events.PlayStrategyChanged args ) {
			SetStrategyDescriptions();
        }

		private void SetStrategyDescriptions() {
            PlayStrategyDescription = mPlayQueue.PlayStrategy?.ConfiguredDescription;
            PlayExhaustedDescription = mPlayQueue.PlayExhaustedStrategy?.Description;
        }

		public void Execute_StartStrategy() {
			mPlayQueue.StartPlayStrategy();
		}

		public bool CanExecute_StartStrategy() {
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

		public void Execute_ConfigureStrategy() {
			mConfigurationDialog.PlayStrategy = mPlayQueue.PlayStrategy.StrategyId;
			mConfigurationDialog.PlayStrategyParameter = mPlayQueue.PlayStrategy.Parameters;
            mConfigurationDialog.ExhaustedStrategySpecification = mPlayQueue.ExhaustedPlayStrategy;
			mConfigurationDialog.DeletePlayedTracks = mPlayQueue.DeletedPlayedTracks;

			if( mDialogService.ShowDialog( DialogNames.PlayStrategyConfiguration, mConfigurationDialog ) == true ) {
				if( mConfigurationDialog.IsConfigurationValid ) {
					mPlayQueue.ExhaustedPlayStrategy = mConfigurationDialog.ExhaustedStrategySpecification;
					mPlayQueue.DeletedPlayedTracks = mConfigurationDialog.DeletePlayedTracks;
                    mPlayQueue.SetPlayStrategy( mConfigurationDialog.PlayStrategy, mConfigurationDialog.PlayStrategyParameter );

					RaiseCanExecuteChangedEvent( "CanExecute_StartStrategy" );
				}
			} 
		}
	}
}
