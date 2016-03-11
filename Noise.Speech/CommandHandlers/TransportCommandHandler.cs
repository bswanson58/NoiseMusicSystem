using System;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Speech.CommandHandlers {
	internal class TransportCommandHandler {
		private readonly ISpeechRecognizer	mSpeechRecognizer;
		private readonly IPlayController	mPlayController;

		public TransportCommandHandler( ISpeechRecognizer speechRecognizer, IPlayController playController ) {
			mSpeechRecognizer = speechRecognizer;
			mPlayController = playController;

			mSpeechRecognizer.SpeechCommandRequest.Subscribe( OnCommandRecognized );
		}

		private void OnCommandRecognized( CommandRequest command ) {
			if( command.Category == CommandRequestCategory.Transport ) {
				switch( command.Command ) {
					case CommandRequestItem.StartPlaying:
						StartPlaying();
						break;

					case CommandRequestItem.PausePlaying:
						PausePlaying();
						break;

					case CommandRequestItem.FinishPlaying:
						FinishPlaying();
						break;

					case CommandRequestItem.NextTrack:
						PlayNextTrack();
						break;

					case CommandRequestItem.PreviousTrack:
						PlayPreviousTrack();
						break;

					case CommandRequestItem.ReplayTrack:
						break;

					case CommandRequestItem.StopPlaying:
						StopPlaying();
						break;
				}
			}
		}

		private void StartPlaying() {
			if( mPlayController.CanPlay ) {
				mPlayController.Play();
			}
		}

		private void StopPlaying() {
			if( mPlayController.CanStop ) {
				mPlayController.Stop();
			}
		}

		private void PausePlaying() {
			if( mPlayController.CanPause ) {
				mPlayController.Pause();
			}
		}

		private void FinishPlaying() {
			if( mPlayController.CanStopAtEndOfTrack ) {
				mPlayController.StopAtEndOfTrack();
			}
		}

		private void PlayNextTrack() {
			if( mPlayController.CanPlayNextTrack ) {
				mPlayController.PlayNextTrack();
			}
		}

		private void PlayPreviousTrack() {
			if( mPlayController.CanPlayPreviousTrack ) {
				mPlayController.PlayPreviousTrack();
			}
		}
	}
}
