using Noise.EntityFrameworkDatabase.Interfaces;

namespace Noise.EntityFrameworkDatabase.DatabaseManager {
	public class ContextProvider : IContextProvider {
		public IDbContext	CreateContext() {
			return( new NoiseContext());
		}
	}
}
