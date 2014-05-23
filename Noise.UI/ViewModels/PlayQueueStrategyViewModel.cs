using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Support;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
	public class PlayQueueStrategyViewModel : AutomaticCommandBase,
											  IHandle<Events.PlayQueueChanged> {
		private readonly IEventAggregator                           mEventAggregator;
		private readonly IPlayQueue                                 mPlayQueue;
		private readonly IDialogService                             mDialogService;
		private readonly PlayStrategyDialogModel					mConfigurationDialog;

		public PlayQueueStrategyViewModel( IEventAggregator eventAggregator, IPlayQueue playQueue, IDialogService dialogService, PlayStrategyDialogModel configurationDialog ) {
			mEventAggregator = eventAggregator;
			mPlayQueue = playQueue;
			mDialogService = dialogService;
			mConfigurationDialog = configurationDialog;

			PlayStrategyDescription = mPlayQueue.PlayStrategy.ConfiguredDescription;
			PlayExhaustedDescription = mPlayQueue.PlayExhaustedStrategy.ConfiguredDescription;

			mEventAggregator.Subscribe( this );
		}

		public void Handle( Events.PlayQueueChanged eventArgs ) {
			RaiseCanExecuteChangedEvent( "CanExecute_StartStrategy" );
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

		private void SetPlayStrategy( ePlayStrategy strategy, IPlayStrategyParameters parameters ) {
			mPlayQueue.SetPlayStrategy( strategy, parameters );
		
			PlayStrategyDescription = mPlayQueue.PlayStrategy.ConfiguredDescription;
		}

		private void SetPlayExhaustedStrategy( ePlayExhaustedStrategy strategy, IPlayStrategyParameters parameters ) {
			mPlayQueue.SetPlayExhaustedStrategy( strategy, parameters );
		
			PlayExhaustedDescription = mPlayQueue.PlayExhaustedStrategy.ConfiguredDescription;
		}

		public string PlayStrategyDescription {
			get {  return( Get( () => PlayStrategyDescription )); }
			set {  Set( () => PlayStrategyDescription, value ); }
		}

		public void Execute_ConfigureStrategy() {
			mConfigurationDialog.PlayStrategy = mPlayQueue.PlayStrategy.StrategyId;
			mConfigurationDialog.PlayStrategyParameter = mPlayQueue.PlayStrategy.Parameters;
			mConfigurationDialog.ExhaustedStrategy = mPlayQueue.PlayExhaustedStrategy.StrategyId;
			mConfigurationDialog.ExhaustedStrategyParameter = mPlayQueue.PlayExhaustedStrategy.Parameters;

			if( mDialogService.ShowDialog( DialogNames.PlayStrategyConfiguration, mConfigurationDialog ) == true ) {
				if( mConfigurationDialog.IsConfigurationValid ) {
					SetPlayStrategy( mConfigurationDialog.PlayStrategy, mConfigurationDialog.PlayStrategyParameter );
					SetPlayExhaustedStrategy( mConfigurationDialog.ExhaustedStrategy, mConfigurationDialog.ExhaustedStrategyParameter );

					RaiseCanExecuteChangedEvent( "CanExecute_StartStrategy" );
				}
			} 
		}
	}
}
