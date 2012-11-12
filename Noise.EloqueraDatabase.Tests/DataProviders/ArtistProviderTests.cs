using Moq;
using NUnit.Framework;
using Noise.BaseDatabase.Tests.DataProviders;
using Noise.EloqueraDatabase.DataProviders;
using Noise.Infrastructure.Interfaces;

namespace Noise.EloqueraDatabase.Tests.DataProviders {
	[TestFixture]
	public class ArtistProviderTests : BaseArtistProviderTests {
		private readonly ProviderTestSetup	mTestSetup;

		public ArtistProviderTests() {
			mTestSetup = new ProviderTestSetup();
		}

		[SetUp]
		public void Setup() {
			mArtworkProvider = new Mock<IArtworkProvider>();
			mTextInfoProvider = new Mock<ITextInfoProvider>();

			mTestSetup.Setup();
		}

		protected override IArtistProvider CreateSut() {
			return( new ArtistProvider( mTestSetup.DatabaseManager, null ));
		}

		[TearDown]
		public void TearDown() {
			mTestSetup.Teardown();
		}
	}
}
