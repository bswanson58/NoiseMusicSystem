using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Interfaces;

namespace Noise.Speech.Recognition {
	internal class SpeechRecognitionManager : ISpeechRecognizer, IRequireInitialization {
		private readonly NoiseCorePreferences	mPreferences;

		public SpeechRecognitionManager( ILifecycleManager lifecycleManager, NoiseCorePreferences preferences ) {
			mPreferences = preferences;

			lifecycleManager.RegisterForInitialize( this );
			lifecycleManager.RegisterForShutdown( this );
		}

		public void Initialize() {
			if( mPreferences.EnableSpeechCommands ) {
				InitializeRecognizer();
			}
		}

		public void Shutdown() {
		}

		private void InitializeRecognizer() {
			
		}
	}
}
