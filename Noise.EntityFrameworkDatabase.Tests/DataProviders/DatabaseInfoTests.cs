using NUnit.Framework;
using Noise.BaseDatabase.Tests.DataProviders;
using Noise.EntityFrameworkDatabase.DataProviders;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.Tests.DataProviders {
	[TestFixture]
	public class DatabaseInfoTests : BaseDatabaseInfoTests {
		private readonly ProviderTestSetup	mTestSetup;

		public DatabaseInfoTests() {
			mTestSetup = new ProviderTestSetup();
		}

		[SetUp]
		public void Setup() {
			mTestSetup.Setup();
		}

		protected override IDatabaseInfo CreateSut() {
			return( new DbVersionProvider( mTestSetup.ContextProvider ));
		}
	}
}
