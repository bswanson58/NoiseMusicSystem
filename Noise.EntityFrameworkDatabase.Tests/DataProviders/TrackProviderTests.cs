using NUnit.Framework;
using Noise.BaseDatabase.Tests.DataProviders;
using Noise.EntityFrameworkDatabase.DataProviders;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.Tests.DataProviders {
	[TestFixture]
	public class TrackProviderTests : BaseTrackProviderTests {
		private readonly ProviderTestSetup	mTestSetup;

		public TrackProviderTests() {
			mTestSetup = new ProviderTestSetup();
		}

		[SetUp]
		public void Setup() {
			mTestSetup.Setup();
		}

		protected override ITrackProvider CreateSut() {
			return( new TrackProvider( mTestSetup.ContextProvider, null ));
		}
	}
}
