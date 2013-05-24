using System.Threading.Tasks;

namespace Noise.Infrastructure.Interfaces {
	public interface INoiseManager {
		bool		Initialize();
		Task<bool>	AsyncInitialize();

		void		Shutdown();
	}
}
