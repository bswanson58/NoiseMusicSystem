using System.Data.Entity;

namespace Noise.EntityFrameworkDatabase.DatabaseManager {
	public class NoiseDbConfiguration : DbConfiguration {
		public NoiseDbConfiguration() {
			SetProviderServices( "System.Data.SqlClient", System.Data.Entity.SqlServer.SqlProviderServices.Instance );
		}
	}
}
