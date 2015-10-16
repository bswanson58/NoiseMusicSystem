using Moq;
using Noise.EntityFrameworkDatabase.Logging;
using NUnit.Framework;
using Noise.BaseDatabase.Tests.DataProviders;
using Noise.EntityFrameworkDatabase.DataProviders;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.Tests.DataProviders {
	[TestFixture]
	public class AlbumProviderTests : BaseAlbumProviderTests {
		private readonly ProviderTestSetup	mTestSetup;
		private readonly Mock<ILogDatabase>	mLog;

		public AlbumProviderTests() {
			mTestSetup = new ProviderTestSetup();
			mLog = new Mock<ILogDatabase>();
		}

		[SetUp]
		public void Setup() {
			mArtworkProvider = new Mock<IArtworkProvider>();
			mTextInfoProvider = new Mock<ITextInfoProvider>();
			mAssociationProvider = new Mock<ITagAssociationProvider>();

			mTestSetup.Setup();
		}

		protected override IAlbumProvider CreateSut() {
			return( new AlbumProvider( mTestSetup.ContextProvider, mArtworkProvider.Object, mTextInfoProvider.Object, mAssociationProvider.Object, mLog.Object ));
		}
	}
}
