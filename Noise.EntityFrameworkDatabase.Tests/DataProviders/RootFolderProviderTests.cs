using Moq;
using Noise.EntityFrameworkDatabase.Logging;
using NUnit.Framework;
using Noise.BaseDatabase.Tests.DataProviders;
using Noise.EntityFrameworkDatabase.DataProviders;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.Tests.DataProviders {
	[TestFixture]
	public class RootFolderProviderTests : BaseRootFolderProviderTests {
		private readonly ProviderTestSetup	mTestSetup;
		private readonly Mock<ILogDatabase>	mLog;

		public RootFolderProviderTests() {
			mTestSetup = new ProviderTestSetup();
			mLog = new Mock<ILogDatabase>();
		}

		[SetUp]
		public void Setup() {
			mTestSetup.Setup();

			mStorageFolderProvider = new StorageFolderProvider( mTestSetup.ContextProvider, mLog.Object );
		}

		protected override IRootFolderProvider CreateSut() {
			return( new RootFolderProvider( mTestSetup.ContextProvider, mLog.Object ));
		}
	}
}
