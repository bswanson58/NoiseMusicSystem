using System;
using System.IO;
using System.Linq;
using System.Speech.Recognition;
using Microsoft.Practices.Prism.PubSubEvents;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Interfaces;
using Noise.Speech.Logging;

namespace Noise.Speech.Recognition {
	internal class SpeechRecognitionManager : ISpeechRecognizer, IRequireInitialization {
		private readonly IEventAggregator			mEventAggregator;
		private readonly ILogSpeech					mLogger;
		private readonly NoiseCorePreferences		mPreferences;
		private readonly SpeechRecognitionEngine	mRecognitionEngine;

		public SpeechRecognitionManager( IEventAggregator eventAggregator, ILifecycleManager lifecycleManager, NoiseCorePreferences preferences, ILogSpeech log ) {
			mEventAggregator = eventAggregator;
			mPreferences = preferences;
			mLogger = log;

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

		private bool InitializeRecognizer() {
			var retValue = false;

			try {
				mRecognitionEngine.SetInputToDefaultAudioDevice();
				mRecognitionEngine.InitialSilenceTimeout = TimeSpan.FromSeconds( 3 );

				LoadSrgsGrammar( "Noise" );

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
			string displayText = "I have no idea what you just said.";

			if (e.Result != null) {
				if (e.Result.Grammar.Name.Equals("Noise")) {
					var words = string.Join(" ", from w in e.Result.Words select w.Text);

					displayText = string.Format("({0}) - Noise command: {1}-{2}",
						words,
						e.Result.Semantics["category"].Value,
						e.Result.Semantics["command"].Value);
				}
				else {
					displayText = e.Result.Text;
				}
			}
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
