using NUnit.Framework;
using Noise.BaseDatabase.Tests.DataProviders;
using Noise.EntityFrameworkDatabase.DataProviders;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.Tests.DataProviders {
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
			mTrackProvider = new TrackProvider( mTestSetup.ContextProvider );
		}

		protected override IDbBaseProvider CreateSut() {
			return( new DbBaseProvider( mTestSetup.ContextProvider ));
		}

		protected override ITrackProvider TrackProvider {
			get { return( mTrackProvider ); }
		}
	}
}
