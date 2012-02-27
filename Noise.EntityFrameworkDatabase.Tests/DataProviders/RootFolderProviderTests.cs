using NUnit.Framework;
using Noise.BaseDatabase.Tests.DataProviders;
using Noise.EntityFrameworkDatabase.DataProviders;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.Tests.DataProviders {
	[TestFixture]
	public class RootFolderProviderTests : BaseRootFolderProviderTests {
		private readonly ProviderTestSetup	mTestSetup;

		public RootFolderProviderTests() {
			mTestSetup = new ProviderTestSetup();
		}

		[SetUp]
		public void Setup() {
			mTestSetup.Setup();

			mStorageFolderProvider = new StorageFolderProvider( mTestSetup.ContextProvider );
		}

		protected override IRootFolderProvider CreateSut() {
			return( new RootFolderProvider( mTestSetup.ContextProvider ));
		}
	}
}
