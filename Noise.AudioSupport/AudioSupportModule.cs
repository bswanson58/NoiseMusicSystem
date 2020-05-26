using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Unity;
using Noise.AudioSupport.Logging;
using Noise.AudioSupport.Player;
using Noise.AudioSupport.ReplayGain;
using Noise.Infrastructure.Interfaces;

namespace Noise.AudioSupport {
	public class AudioSupportModule : IModule {
		private readonly IUnityContainer    mContainer;

		public AudioSupportModule( IUnityContainer container ) {
			mContainer = container;
		}

		public void Initialize() {
			mContainer.RegisterType<IAudioPlayer, AudioPlayer>( new ContainerControlledLifetimeManager());
			mContainer.RegisterType<IEqManager, EqManager>( new ContainerControlledLifetimeManager());

			mContainer.RegisterType<IReplayGainScanner, ReplayGainScanner>();

			mContainer.RegisterType<ILogAudioPlay, LogAudioPlay>();
		}
	}
}
