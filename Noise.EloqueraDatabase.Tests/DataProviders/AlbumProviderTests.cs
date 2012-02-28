using Moq;
using NUnit.Framework;
using Noise.BaseDatabase.Tests.DataProviders;
using Noise.EloqueraDatabase.DataProviders;
using Noise.Infrastructure.Interfaces;

namespace Noise.EloqueraDatabase.Tests.DataProviders {
	[TestFixture]
	public class AlbumProviderTests : BaseAlbumProviderTests {
		private readonly ProviderTestSetup	mTestSetup;

		public AlbumProviderTests() {
			mTestSetup = new ProviderTestSetup();
		}

		[SetUp]
		public void Setup() {
			mArtworkProvider = new Mock<IArtworkProvider>();
			mTextInfoProvider = new Mock<ITextInfoProvider>();
			mAssociationProvider = new Mock<ITagAssociationProvider>();

			mTestSetup.Setup();
		}

		protected override IAlbumProvider CreateSut() {
			return( new AlbumProvider( mTestSetup.DatabaseManager, mArtworkProvider.Object, mTextInfoProvider.Object, mAssociationProvider.Object ));
		}

		[TearDown]
		public void TearDown() {
			mTestSetup.Teardown();
		}
	}
}
