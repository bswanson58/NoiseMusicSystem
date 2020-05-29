using Noise.AudioSupport.Logging;
using Noise.AudioSupport.Player;
using Noise.AudioSupport.ReplayGain;
using Noise.Infrastructure.Interfaces;
using Prism.Ioc;
using Prism.Modularity;

namespace Noise.AudioSupport {
	public class AudioSupportModule : IModule {
        public void RegisterTypes( IContainerRegistry containerRegistry ) {
            containerRegistry.RegisterSingleton<IAudioPlayer, AudioPlayer>();
            containerRegistry.RegisterSingleton<IEqManager, EqManager>();

            containerRegistry.Register<IReplayGainScanner, ReplayGainScanner>();

            containerRegistry.Register<ILogAudioPlay, LogAudioPlay>();
        }

        public void OnInitialized( IContainerProvider containerProvider ) {
        }
    }
}
