using NUnit.Framework;
using Noise.BaseDatabase.Tests.DataProviders;
using Noise.EloqueraDatabase.DataProviders;
using Noise.Infrastructure.Interfaces;

namespace Noise.EloqueraDatabase.Tests.DataProviders {
	[TestFixture]
	public class DbBaseProviderTests : BaseDbBaseProviderTests {
		private readonly ProviderTestSetup	mTestSetup;
		private ITrackProvider				mTrackProvider;

		public DbBaseProviderTests() {
			mTestSetup = new ProviderTestSetup();
		}

		[SetUp]
		public void Setup() {
			mTestSetup.Setup();
			mTrackProvider = new TrackProvider( mTestSetup.DatabaseManager );
		}

		protected override IDbBaseProvider CreateSut() {
			return( new DbBaseProvider( mTestSetup.DatabaseManager ));
		}

		[TearDown]
		public void TearDown() {
			mTestSetup.Teardown();
		}

		protected override ITrackProvider TrackProvider {
			get { return( mTrackProvider ); }
		}
	}
}
