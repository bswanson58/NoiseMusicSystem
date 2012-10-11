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
		private string						mStorageName;

		private IBlobStorageManager CreateSut() {
			var blobStoragePath = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ), Constants.CompanyName );
			blobStoragePath = Path.Combine( blobStoragePath, "Test Blob Storage" );

			mBlobResolver = new Mock<IBlobStorageResolver>();

			var storageManager = new BlobStorageManager( mBlobResolver.Object );

			storageManager.Initialize( blobStoragePath );
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
