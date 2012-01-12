using System;
using System.IO;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Noise.EloqueraDatabase.BlobStore;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;

namespace Noise.EloqueraDatabase.Tests.BlobStore {
	[TestFixture]
	public class BlobStorageTests {
		private	const string				cTestStorageName = "unit test storage";
		private Mock<IBlobStorageResolver>	mStorageResolver;
		private IBlobStorageManager			mStorageManager;

		[SetUp]
		public void Setup() {
			var blobStoragePath = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ), Constants.CompanyName );

			mStorageResolver = new Mock<IBlobStorageResolver>();
			mStorageResolver.Setup( m => m.StorageLevels ).Returns( 1 );
			mStorageResolver.Setup( m => m.KeyForStorageLevel( It.IsAny<long>(), It.Is<uint>( p => p == 0  ))).Returns( "first storage level" );

			var storageManager = new BlobStorageManager( mStorageResolver.Object, blobStoragePath );

			storageManager.DeleteStorage( cTestStorageName );
			if( storageManager.CreateStorage( cTestStorageName )) {
				if( storageManager.OpenStorage( cTestStorageName )) {
					mStorageManager = storageManager;
				}
			}
		}

		[TearDown]
		public void TearDown() {
			if( mStorageManager != null ) {
				mStorageManager.DeleteStorage( cTestStorageName );
			}
		}

		private IBlobStorage CreateSut() {
			return( mStorageManager.GetStorage());
		}

		[Test]
		public void CanInsertStream() {
			var buffer = new byte[] { 0, 1, 2, 3, 4 };
			var memoryStream = new MemoryStream( buffer );

			var sut = CreateSut();

			sut.Insert( 1, memoryStream );
		}

		[Test]
		[Ignore( "Requires external file to test." )]
		public void CanInsertFromFileSource() {
		}

		[Test]
		[ExpectedException( typeof( BlobStorageException ))]
		public void CannotInsertExistingItem() {
			var buffer = new byte[] { 0, 1, 2, 3, 4 };
			var memoryStream = new MemoryStream( buffer );

			var sut = CreateSut();
			sut.Insert( 1, memoryStream );
			sut.Insert( 1, memoryStream );
		}

		[Test]
		public void CanStoreBlob() {
			var buffer = new byte[] { 0, 1, 2, 3, 4 };
			var memoryStream = new MemoryStream( buffer );

			var sut = CreateSut();

			sut.Store( 1, memoryStream );

			using( var retrievedStream = sut.Retrieve( 1 )) {
				retrievedStream.Length.Should().Be( memoryStream.Length );
			}
		}

		[Test]
		public void CanStoreText() {
			var sut = CreateSut();

			const string textToStore = "my text to store";
			sut.StoreText( 2, textToStore );
		}

		[Test]
		[Ignore( "Requires external file to test." )]
		public void CanStoreFromFileSource() {
		}

		[Test]
		public void CanRetrieveText() {
			var sut = CreateSut();

			const string textToStore = "my text to store";
			sut.StoreText( 2, textToStore );

			var retrievedText = sut.RetrieveText( 2 );

			retrievedText.Should().Be( textToStore );
		}

		[Test]
		public void CanRetrieveStream() {
			var buffer = new byte[] { 0, 1, 2, 3, 4 };
			var memoryStream = new MemoryStream( buffer );

			var sut = CreateSut();

			sut.Insert( 1, memoryStream );

			using( var retrievedStream = sut.Retrieve( 1 )) {
				retrievedStream.Length.Should().Be( memoryStream.Length );
			}
		}

		[Test]
		public void CanRetrieveBytes() {
			var buffer = new byte[] { 0, 1, 2, 3, 4 };
			var memoryStream = new MemoryStream( buffer );

			var sut = CreateSut();

			sut.Insert( 1, memoryStream );

			var retrievedBytes = sut.RetrieveBytes( 1 );

			retrievedBytes.Length.Should().Be( (int)memoryStream.Length );
		}

		[Test]
		public void CanUpdateStorage() {
			var buffer1 = new byte[] { 0, 1, 2, 3, 4 };
			var buffer2 = new byte[] { 4, 3, 2 };
			var memoryStream = new MemoryStream( buffer1 );

			var sut = CreateSut();

			sut.Insert( 1, memoryStream );
			memoryStream = new MemoryStream( buffer2 );
			sut.Store( 1, memoryStream );

			using( var retrievedStream = sut.Retrieve( 1 )) {
				retrievedStream.Length.Should().Be( buffer2.Length );
			}
		}

		[Test]
		[ExpectedException( typeof( BlobStorageException ))]
		public void CanDelete() {
			const string textToStore = "delete me";

			var sut = CreateSut();
			sut.StoreText( 3, textToStore );
			sut.Delete( 3 );

			var retrievedText = sut.RetrieveText( 3 );

			Assert.IsNullOrEmpty( retrievedText );
		}

		[Test]
		public void CanDeleteStorage() {
			var sut = CreateSut();

			sut.DeleteStorage();

			Assert.IsFalse( mStorageManager.IsOpen );
		}
	}
}
