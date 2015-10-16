using Moq;
using Noise.EntityFrameworkDatabase.Logging;
using NUnit.Framework;
using Noise.BaseDatabase.Tests.DataProviders;
using Noise.EntityFrameworkDatabase.DataProviders;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.Tests.DataProviders {
	[TestFixture]
	public class DatabaseInfoTests : BaseDatabaseInfoTests {
		private readonly ProviderTestSetup	mTestSetup;
		private readonly Mock<ILogDatabase>	mLog;

		public DatabaseInfoTests() {
			mTestSetup = new ProviderTestSetup();
			mLog = new Mock<ILogDatabase>();
		}

		[SetUp]
		public void Setup() {
			mTestSetup.Setup();
		}

		protected override IDatabaseInfo CreateSut() {
			return( new DbVersionProvider( mTestSetup.ContextProvider, mLog.Object ));
		}
	}
}
