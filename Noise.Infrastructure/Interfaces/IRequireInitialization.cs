namespace Noise.Infrastructure.Interfaces {
	public interface IRequireConstruction {
	}

	public interface IRequireInitialization : IRequireConstruction {
		void	Initialize();
		void	Shutdown();
	}
}
