using System;
using System.IO;
using System.Reactive.Subjects;
using System.Speech.Recognition;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Speech.Logging;

namespace Noise.Speech.Recognition {
	internal class SpeechRecognitionManager : ISpeechRecognizer, IRequireInitialization {
		private const string	cNoiseGrammar		= "Noise";
		private const string	cGrammarCategory	= "category";
		private const string	cGrammarCommand		= "command";

		private readonly ILogSpeech					mLogger;
		private readonly NoiseCorePreferences		mPreferences;
		private readonly SpeechRecognitionEngine	mRecognitionEngine;
		private readonly Subject<CommandRequest>	mCommandSubject;

		public SpeechRecognitionManager( ILifecycleManager lifecycleManager, NoiseCorePreferences preferences, ILogSpeech log ) {
			mPreferences = preferences;
			mLogger = log;

			mCommandSubject = new Subject<CommandRequest>();
			mRecognitionEngine = new SpeechRecognitionEngine();

			lifecycleManager.RegisterForInitialize( this );
			lifecycleManager.RegisterForShutdown( this );
		}

		public void Initialize() {
			if( mPreferences.EnableSpeechCommands ) {
				if( InitializeRecognizer() ) {
					ActivateRecognition();
				}
			}
		}

		public void Shutdown() {
			DeactivateRecognition();

			mRecognitionEngine.Dispose();
		}

		public IObservable<CommandRequest> SpeechCommandRequest {
			get {  return( mCommandSubject ); }
		} 

		private bool InitializeRecognizer() {
			var retValue = false;

			try {
				mRecognitionEngine.SetInputToDefaultAudioDevice();
				mRecognitionEngine.InitialSilenceTimeout = TimeSpan.FromSeconds( 3 );

				LoadSrgsGrammar( cNoiseGrammar );

				mRecognitionEngine.SpeechRecognized += OnSpeechRecognized;

				retValue = true;
			}
			catch( Exception ex ) {
				mLogger.LogException( "Initializing speech recognizer", ex );
			}

			return( retValue );
		}
	
		private void LoadSrgsGrammar(string grammarName) {
			var filePath = Path.Combine( Environment.CurrentDirectory, string.Format( "{0}Grammar.grxml", grammarName ));

			mRecognitionEngine.LoadGrammar( new Grammar(filePath) { Name = grammarName });
		}

		private void OnSpeechRecognized(object sender, SpeechRecognizedEventArgs e) {
			if(( e.Result != null ) &&
			   ( e.Result.Grammar != null ) &&
			   ( e.Result.Semantics != null )) {
				if(( e.Result.Grammar.Name.Equals( cNoiseGrammar )) &&
				   ( e.Result.Semantics.ContainsKey( cGrammarCategory )) &&
				   ( e.Result.Semantics.ContainsKey( cGrammarCommand ))) {
					var speechCategory = e.Result.Semantics[cGrammarCategory].Value.ToString();
					var speechCommand = e.Result.Semantics[cGrammarCommand].Value.ToString();
					var command = new CommandRequest( TranslateCategory( speechCategory ), TranslateCommand( speechCommand ));

					mCommandSubject.OnNext( command );
				}
			}
		}

		private CommandRequestCategory TranslateCategory( string category ) {
			var retValue = CommandRequestCategory.Unknown;

			if( category.Equals( "queue", StringComparison.InvariantCultureIgnoreCase )) {
				retValue = CommandRequestCategory.Queue;
			}
			else if( category.Equals( "transport", StringComparison.InvariantCultureIgnoreCase )) {
				retValue = CommandRequestCategory.Transport;
			}
			else if( category.Equals( "view", StringComparison.InvariantCultureIgnoreCase ) ) {
				retValue = CommandRequestCategory.View;
			}

			return( retValue );
		}

		private CommandRequestItem TranslateCommand( string command ) {
			var retValue = CommandRequestItem.Unknown;

			if( command.Equals( "play", StringComparison.InvariantCultureIgnoreCase )) {
				retValue = CommandRequestItem.StartPlaying;
			}
			else if( command.Equals( "pause", StringComparison.InvariantCultureIgnoreCase )) {
				retValue = CommandRequestItem.PausePlaying;
			}
			else if( command.Equals( "next", StringComparison.InvariantCultureIgnoreCase )) {
				retValue = CommandRequestItem.NextTrack;
			}
			else if( command.Equals( "previous", StringComparison.InvariantCultureIgnoreCase )) {
				retValue = CommandRequestItem.PreviousTrack;
			}
			else if( command.Equals( "replay", StringComparison.InvariantCultureIgnoreCase )) {
				retValue = CommandRequestItem.ReplayTrack;
			}
			else if( command.Equals( "stop", StringComparison.InvariantCultureIgnoreCase )) {
				retValue = CommandRequestItem.StopPlaying;
			}
			else if( command.Equals( "finish", StringComparison.InvariantCultureIgnoreCase )) {
				retValue = CommandRequestItem.FinishPlaying;
			}

			return( retValue );
		}

		private void ActivateRecognition() {
			mRecognitionEngine.RecognizeAsync( RecognizeMode.Multiple );
		}

		private void DeactivateRecognition() {
			mRecognitionEngine.RecognizeAsyncCancel();
			mRecognitionEngine.RecognizeAsyncStop();
		}
	}
}
