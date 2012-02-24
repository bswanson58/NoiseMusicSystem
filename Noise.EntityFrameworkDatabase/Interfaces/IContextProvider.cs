namespace Noise.EntityFrameworkDatabase.Interfaces {
	public interface IContextProvider {
		IDbContext	CreateContext();
	}
}
