using NUnit.Framework;
using Noise.BaseDatabase.Tests.DataProviders;
using Noise.EntityFrameworkDatabase.DataProviders;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.Tests.DataProviders {
	[TestFixture]
	public class StorageFolderProviderTests : BaseStorageFolderProviderTests {
		private readonly ProviderTestSetup	mTestSetup;

		public StorageFolderProviderTests() {
			mTestSetup = new ProviderTestSetup();
		}

		[SetUp]
		public void Setup() {
			mTestSetup.Setup();
		}

		protected override IStorageFolderProvider CreateSut() {
			return( new StorageFolderProvider( mTestSetup.ContextProvider ));
		}
	}
}
