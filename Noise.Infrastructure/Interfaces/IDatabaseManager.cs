namespace Noise.Infrastructure.Interfaces {
	public interface IDatabaseManager {
		bool			Initialize();
		void			Shutdown();
	}
}
