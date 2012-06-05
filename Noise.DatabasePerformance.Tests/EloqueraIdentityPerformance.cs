using Microsoft.Practices.Unity;
using NUnit.Framework;
using Noise.EloqueraDatabase;

namespace Noise.DatabasePerformance.Tests {
	[TestFixture]
	public class EloqueraIdentityPerformance {
		private readonly IUnityContainer	mContainer;

		public EloqueraIdentityPerformance() {
			mContainer = new UnityContainer();

			var databaseModule = new EloqueraDatabaseModule( mContainer );
		}

		private void CreateDatabaseFactory() {
			
		}
	}
}
