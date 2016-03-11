using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Unity;
using Noise.Infrastructure.Interfaces;
using Noise.Speech.Recognition;

namespace Noise.Speech {
	public class NoiseSpeechModule : IModule {
		private readonly IUnityContainer    mContainer;

		public NoiseSpeechModule( IUnityContainer container ) {
			mContainer = container;
		}

		public void Initialize() {
			mContainer.RegisterType<ISpeechRecognizer, SpeechRecognitionManager>( new HierarchicalLifetimeManager());
		}
	}
}
