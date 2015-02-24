using Moq;
using Noise.RavenDatabase.Logging;
using NUnit.Framework;
using Noise.BaseDatabase.Tests.DataProviders;
using Noise.Infrastructure.Interfaces;
using Noise.RavenDatabase.DataProviders;

namespace Noise.RavenDatabase.Tests.DataProviders {
	[TestFixture]
	public class ArtistProviderTests : BaseArtistProviderTests {
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

			mTagProvider = new Mock<ITagAssociationProvider>();
		}

		[TearDown]
		public void Teardown() {
			mCommon.Teardown();
		}

		protected override IArtistProvider CreateSut() {
			return( new ArtistProvider( mCommon.DatabaseFactory.Object, mTagProvider.Object, mLog.Object ));
		}
	}
}
