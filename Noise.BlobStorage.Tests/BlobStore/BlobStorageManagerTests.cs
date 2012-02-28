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
		private const string				cTestStorageName = "unit test storage";

		private	Mock<IBlobStorageResolver>	mBlobResolver;
		private IBlobStorageManager			mStorageManager;
		private string						mStorageName;

		private IBlobStorageManager CreateSut( string storageName ) {
			var blobStoragePath = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ), Constants.CompanyName );

			mBlobResolver = new Mock<IBlobStorageResolver>();

			var storageManager = new BlobStorageManager( mBlobResolver.Object );

			storageManager.Initialize( blobStoragePath );
			if( storageManager.CreateStorage( storageName )) {
				mStorageName = storageName;
				mStorageManager = storageManager;
			}

			return( mStorageManager );
		}

		[TearDown]
		public void TearDown() {
			if( mStorageManager != null ) {
				mStorageManager.DeleteStorage( mStorageName );
			}
		}

		[Test]
		public void CanCreateStorage() {
			var sut = CreateSut( cTestStorageName );

			Assert.IsNotNull( sut );
		}

		[Test]
		public void CanOpenStorage() {
			var sut = CreateSut( cTestStorageName );

			var opened = sut.OpenStorage( cTestStorageName );

			Assert.IsTrue( opened );
		}

		[Test]
		public void CannotOpenNonExistingStorage() {
			var sut = CreateSut( cTestStorageName );

			var opened = sut.OpenStorage( "my great storage" );

			Assert.IsFalse( opened );
		}

		[Test]
		public void OpenedStorageIndicatesIsIOpen() {
			var sut = CreateSut( cTestStorageName );

			var opened = sut.OpenStorage( cTestStorageName );

			Assert.IsTrue( opened );
			Assert.IsTrue( sut.IsOpen );
		}

		[Test]
		public void CanCloseStorage() {
			var sut = CreateSut( cTestStorageName );

			sut.OpenStorage( cTestStorageName );
			sut.CloseStorage();

			Assert.IsFalse( sut.IsOpen );
		}

		[Test]
		public void CanDeleteStorage() {
			var sut = CreateSut( cTestStorageName );

			sut.DeleteStorage( cTestStorageName );

			var opened = sut.OpenStorage( cTestStorageName );

			Assert.IsFalse( opened );
		}
	}
}
