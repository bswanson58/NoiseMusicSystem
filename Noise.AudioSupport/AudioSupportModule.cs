using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Unity;
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
			mContainer.RegisterType<IAudioPlayer, AudioPlayer>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<IReplayGainScanner, ReplayGainScanner>();
		}
	}
}
