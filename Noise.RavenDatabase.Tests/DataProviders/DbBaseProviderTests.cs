using NUnit.Framework;
using Noise.BaseDatabase.Tests.DataProviders;
using Noise.Infrastructure.Interfaces;
using Noise.RavenDatabase.DataProviders;

namespace Noise.RavenDatabase.Tests.DataProviders {
	[TestFixture]
	public class DbBaseProviderTests : BaseDbBaseProviderTests {
		private CommonTestSetup	mCommon;

		[TestFixtureSetUp]
		public void FixtureSetup() {
			mCommon = new CommonTestSetup();
			mCommon.FixtureSetup();
		}

		[SetUp]
		public void Setup() {
			mCommon.Setup();
		}

		[TearDown]
		public void Teardown() {
			mCommon.Teardown();
		}

		protected override IDbBaseProvider CreateSut() {
			return ( new DbBaseProvider( mCommon.DatabaseFactory.Object ));
		}

		protected override ITrackProvider TrackProvider {
			get {
				return ( new TrackProvider( mCommon.DatabaseFactory.Object ) );
			}
		}
	}
}
