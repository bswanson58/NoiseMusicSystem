namespace Noise.Core.Support {
	public interface IRequireConstruction {
	}

	public interface IRequireInitialization : IRequireConstruction {
		void	Initialize();
		void	Shutdown();
	}
}
