using NUnit.Framework;
using Noise.BaseDatabase.Tests.DataProviders;
using Noise.Infrastructure.Interfaces;
using Noise.RavenDatabase.DataProviders;

namespace Noise.RavenDatabase.Tests.DataProviders {
	[TestFixture]
	public class RootFolderProviderTests : BaseRootFolderProviderTests {
		private CommonTestSetup	mCommon;

		[TestFixtureSetUp]
		public void FixtureSetup() {
			mCommon = new CommonTestSetup();
			mCommon.FixtureSetup();
		}

		[SetUp]
		public void Setup() {
			mCommon.Setup();

			mStorageFolderProvider = new StorageFolderProvider( mCommon.DatabaseFactory.Object );
		}

		[TearDown]
		public void Teardown() {
			mCommon.Teardown();
		}

		protected override IRootFolderProvider CreateSut() {
			return ( new RootFolderProvider( mCommon.DatabaseFactory.Object ) );
		}
	}
}
