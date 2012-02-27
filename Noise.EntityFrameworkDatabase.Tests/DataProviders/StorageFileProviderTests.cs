using Moq;
using NUnit.Framework;
using Noise.BaseDatabase.Tests.DataProviders;
using Noise.EntityFrameworkDatabase.DataProviders;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.Tests.DataProviders {
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
			mTrackProvider = new TrackProvider( mTestSetup.ContextProvider );
			mFolderProvider = new StorageFolderProvider( mTestSetup.ContextProvider );
		}

		protected override IStorageFileProvider CreateSut() {
			return( new StorageFileProvider( mTestSetup.ContextProvider, mAlbumProvider.Object, mTrackProvider, mFolderProvider ));
		}
	}
}
