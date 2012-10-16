namespace Noise.Infrastructure.Interfaces {
	public interface ILifecycleManager {
		void	RegisterForInitialize( IRequireInitialization module );
		void	RegisterForShutdown( IRequireInitialization module );

		void	Initialize();
		void	Shutdown();
	}
}
