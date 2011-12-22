namespace Noise.Infrastructure.Interfaces {
	public interface IDatabaseShell {
		IDatabase	Database { get; }

		void		FreeDatabase();
	}
}
