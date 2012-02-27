using NUnit.Framework;
using Noise.BaseDatabase.Tests.DataProviders;
using Noise.EloqueraDatabase.DataProviders;
using Noise.Infrastructure.Interfaces;

namespace Noise.EloqueraDatabase.Tests.DataProviders {
	[TestFixture]
	public class TimestampProviderTests : BaseTimestampProviderTests {
		private readonly ProviderTestSetup	mTestSetup;

		public TimestampProviderTests() {
			mTestSetup = new ProviderTestSetup();
		}

		[SetUp]
		public void Setup() {
			mTestSetup.Setup();
		}

		protected override ITimestampProvider CreateSut() {
			return( new TimestampProvider( mTestSetup.DatabaseManager ));
		}

		[TearDown]
		public void TearDown() {
			mTestSetup.Teardown();
		}
	}
}
