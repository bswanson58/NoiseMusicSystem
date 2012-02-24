using Noise.EntityFrameworkDatabase.Interfaces;

namespace Noise.EntityFrameworkDatabase {
	public class ContextProvider : IContextProvider {
		public IDbContext	CreateContext() {
			return( new NoiseContext());
		}
	}
}
