using System.Threading.Tasks;

namespace Noise.Infrastructure.Interfaces {
	public interface INoiseManager {
		bool		Initialize();
		bool		InitializeAndNotify();
		Task<bool>	AsyncInitialize();

		void		Shutdown();
	}
}
