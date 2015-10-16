using Moq;
using Noise.RavenDatabase.Logging;
using NUnit.Framework;
using Noise.BaseDatabase.Tests.DataProviders;
using Noise.Infrastructure.Interfaces;
using Noise.RavenDatabase.DataProviders;

namespace Noise.RavenDatabase.Tests.DataProviders {
	[TestFixture]
	public class AlbumProviderTests  : BaseAlbumProviderTests {
		private CommonTestSetup	mCommon;
		private Mock<ILogRaven>	mLog;

		[TestFixtureSetUp]
		public void FixtureSetup() {
			mCommon = new CommonTestSetup();
			mCommon.FixtureSetup();

			mLog = new Mock<ILogRaven>();
		}

		[SetUp]
		public void Setup() {
			mCommon.Setup();

			mArtworkProvider = new Mock<IArtworkProvider>();
			mTextInfoProvider = new Mock<ITextInfoProvider>();
			mAssociationProvider = new Mock<ITagAssociationProvider>();
		}

		[TearDown]
		public void Teardown() {
			mCommon.Teardown();
		}

		protected override IAlbumProvider CreateSut() {
			return( new AlbumProvider( mCommon.DatabaseFactory.Object, mArtworkProvider.Object, mTextInfoProvider.Object, mAssociationProvider.Object, mLog.Object ));
		}
	}
}
