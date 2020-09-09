using System.Threading.Tasks;

namespace Noise.Infrastructure.Interfaces {
	public interface INoiseManager {
		bool		Initialize();
		bool		InitializeAndNotify();
		Task<bool>	AsyncInitialize();

        bool		CanShutDown( out string reason );
        void		Shutdown();
	}
}
