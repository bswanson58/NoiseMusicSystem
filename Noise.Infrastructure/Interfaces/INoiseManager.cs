namespace Noise.Infrastructure.Interfaces {
	public interface INoiseManager {
		bool					Initialize();
		void					StartExplorerJobs();
		void					Shutdown();
	}
}
