using System;
using System.IO;
using Moq;
using NUnit.Framework;
using Noise.BlobStorage.BlobStore;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;

namespace Noise.BlobStorage.Tests.BlobStore {
	[TestFixture]
	public class BlobStorageManagerTests {
		private	Mock<IBlobStorageResolver>	mBlobResolver;
		private IBlobStorageManager			mStorageManager;
        private Mock<ILibraryConfiguration> mLibraryConfiguration;

		private IBlobStorageManager CreateSut() {
            mLibraryConfiguration = new Mock<ILibraryConfiguration>();

			var blobStoragePath = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ), Constants.CompanyName );
			blobStoragePath = Path.Combine( blobStoragePath, "Test Blob Storage" );
            mLibraryConfiguration.Setup( m => m.Current.BlobDatabasePath ).Returns( blobStoragePath );

			mBlobResolver = new Mock<IBlobStorageResolver>();

			var storageManager = new BlobStorageManager( new Mock<INoiseLog>().Object );
			storageManager.SetResolver( mBlobResolver.Object );

			storageManager.Initialize( mLibraryConfiguration.Object );
			if( storageManager.CreateStorage()) {
				mStorageManager = storageManager;
			}

			return( mStorageManager );
		}

		[TearDown]
		public void TearDown() {
			if( mStorageManager != null ) {
				mStorageManager.DeleteStorage();
			}
		}

		[Test]
		public void CanCreateStorage() {
			var sut = CreateSut();

			Assert.IsNotNull( sut );
		}

		[Test]
		public void CanOpenStorage() {
			var sut = CreateSut();

			var opened = sut.OpenStorage();

			Assert.IsTrue( opened );
		}

		[Test]
		public void OpenedStorageIndicatesIsIOpen() {
			var sut = CreateSut();

			var opened = sut.OpenStorage();

			Assert.IsTrue( opened );
			Assert.IsTrue( sut.IsOpen );
		}

		[Test]
		public void CanCloseStorage() {
			var sut = CreateSut();

			sut.OpenStorage();
			sut.CloseStorage();

			Assert.IsFalse( sut.IsOpen );
		}

		[Test]
		public void CanDeleteStorage() {
			var sut = CreateSut();

			sut.DeleteStorage();

			var opened = sut.OpenStorage();

			Assert.IsFalse( opened );
		}
	}
}
