using System;
using System.IO;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Noise.BlobStorage.BlobStore;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;

namespace Noise.BlobStorage.Tests.BlobStore {
	internal class TestData {
		public string	String {get; set; }
		public int		Integer { get; set; }
	}

	[TestFixture]
	public class BlobStorageTests {
		private Mock<IBlobStorageResolver>	mStorageResolver;
		private IBlobStorageManager			mStorageManager;

		[SetUp]
		public void Setup() {
			var blobStoragePath = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ), Constants.CompanyName );
			blobStoragePath = Path.Combine( blobStoragePath, "Test Blob Storage" );

			mStorageResolver = new Mock<IBlobStorageResolver>();
			mStorageResolver.Setup( m => m.StorageLevels ).Returns( 1 );
			mStorageResolver.Setup( m => m.KeyForStorageLevel( It.IsAny<long>(), It.Is<uint>( p => p == 0  ))).Returns( "first storage level" );
			mStorageResolver.Setup( m => m.KeyForStorageLevel( It.IsAny<string>(), It.Is<uint>( p => p == 0  ))).Returns( "first string level" );

			var storageManager = new BlobStorageManager();
			storageManager.SetResolver( mStorageResolver.Object );
			storageManager.Initialize( blobStoragePath );

			storageManager.DeleteStorage();
			if( storageManager.CreateStorage()) {
				if( storageManager.OpenStorage()) {
					mStorageManager = storageManager;
				}
			}
		}

		[TearDown]
		public void TearDown() {
			if( mStorageManager != null ) {
				mStorageManager.DeleteStorage();
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
		public void CanInsertString() {
			const string stringId = "string id";
			const string stringData = "some important string data";
			var sut = CreateSut();

			sut.Insert( stringId, stringData );

			var retrievedText = sut.RetrieveText( stringId );

			retrievedText.Should().Be( stringData );
		}

		[Test]
		public void CanInsertObject() {
			const string stringId = "string id";
			var testData = new TestData { String = "some string", Integer = 1234 };
			var sut = CreateSut();

			sut.Store( stringId, testData );

			var retrievedData = sut.RetrieveObject<TestData>( stringId );

			retrievedData.ShouldHave().AllProperties().EqualTo( testData );
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
		public void CanDetermineNonExistingBlob() {
			var sut = CreateSut();

			sut.Insert( "1", "1" );
			var exists = sut.BlobExists( "12" );

			exists.Should().BeFalse();
		}

		[Test]
		public void CanDetermineExistingBlob() {
			var sut = CreateSut();

			sut.Insert( "1", "Data" );

			var exists = sut.BlobExists( "1" );

			exists.Should().BeTrue();
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
		public void CanDelete() {
			const string textToStore = "delete me";

			var sut = CreateSut();
			sut.StoreText( 3, textToStore );
			sut.Delete( 3 );

			var retrievedText = sut.RetrieveText( 3 );

			Assert.IsNullOrEmpty( retrievedText );
		}

		[Test]
		public void CanDeleteStringId() {
			const string stringId = "my id";
			const string textToStore = "delete me";

			var sut = CreateSut();
			sut.StoreText( stringId, textToStore );
			sut.Delete( stringId );

			var retrievedText = sut.RetrieveText( stringId );

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
