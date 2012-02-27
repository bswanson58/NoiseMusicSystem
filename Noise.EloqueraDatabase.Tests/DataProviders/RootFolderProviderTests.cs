using NUnit.Framework;
using Noise.BaseDatabase.Tests.DataProviders;
using Noise.EloqueraDatabase.DataProviders;
using Noise.Infrastructure.Interfaces;

namespace Noise.EloqueraDatabase.Tests.DataProviders {
	[TestFixture]
	public class RootFolderProviderTests : BaseRootFolderProviderTests {
		private readonly ProviderTestSetup	mTestSetup;

		public RootFolderProviderTests() {
			mTestSetup = new ProviderTestSetup();
		}

		[SetUp]
		public void Setup() {
			mTestSetup.Setup();

			mStorageFolderProvider = new StorageFolderProvider( mTestSetup.DatabaseManager );
		}

		protected override IRootFolderProvider CreateSut() {
			return( new RootFolderProvider( mTestSetup.DatabaseManager ));
		}

		[TearDown]
		public void TearDown() {
			mTestSetup.Teardown();
		}
	}
}
