using Moq;
using NUnit.Framework;
using Noise.BaseDatabase.Tests.DataProviders;
using Noise.EloqueraDatabase.DataProviders;
using Noise.Infrastructure.Interfaces;

namespace Noise.EloqueraDatabase.Tests.DataProviders {
	[TestFixture]
	public class StorageFileProviderTests : BaseStorageFileProviderTests {
		private readonly ProviderTestSetup	mTestSetup;

		public StorageFileProviderTests() {
			mTestSetup = new ProviderTestSetup();
		}

		[SetUp]
		public void Setup() {
			mTestSetup.Setup();

			mAlbumProvider = new Mock<IAlbumProvider>();
			mTrackProvider = new TrackProvider( mTestSetup.DatabaseManager );
			mFolderProvider = new StorageFolderProvider( mTestSetup.DatabaseManager );
		}

		protected override IStorageFileProvider CreateSut() {
			return( new StorageFileProvider( mTestSetup.DatabaseManager, mAlbumProvider.Object, mTrackProvider, mFolderProvider ));
		}

		[TearDown]
		public void TearDown() {
			mTestSetup.Teardown();
		}
	}
}
