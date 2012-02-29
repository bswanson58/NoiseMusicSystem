namespace Noise.EntityFrameworkDatabase.Interfaces {
	public interface IDatabaseInitializeStrategy {
		bool	InitializeDatabase( IDbContext context );

		bool	DidCreateDatabase { get; }
	}
}
