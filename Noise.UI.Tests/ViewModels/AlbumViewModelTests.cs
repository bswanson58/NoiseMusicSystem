using FluentAssertions;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Support;
using Noise.UI.ViewModels;
using ReusableBits.TestSupport.Mocking;
using ReusableBits.TestSupport.Threading;

namespace Noise.UI.Tests.ViewModels {
	internal class TestableAlbumViewModel : Testable<AlbumViewModel> {
		private readonly TaskScheduler		mTaskScheduler;
 

		public TestableAlbumViewModel() {
			// Set tpl tasks to use the current thread only.
			mTaskScheduler = new CurrentThreadTaskScheduler();
		}

		public override AlbumViewModel ClassUnderTest {
			get {
				var		retValue = base.ClassUnderTest;

				if( retValue != null ) {
					retValue.AlbumRetrievalTaskHandler = new TaskHandler( mTaskScheduler, mTaskScheduler );
				}

				return( retValue );
			}
		}
	}

	[TestFixture]
	public class AlbumViewModelTests {

		[SetUp]
		public void Setup() {
			NoiseLogger.Current = new Mock<ILog>().Object;

			// Set the ui dispatcher to run on the current thread.
			ReusableBits.Mvvm.ViewModelSupport.Execute.ResetWithoutDispatcher();

			// Set up the AutoMapper configurations.
			MappingConfiguration.Configure();
		}

		[Test]
		public void CanCreateAlbumViewModel() {
			var sut = new TestableAlbumViewModel().ClassUnderTest;

			Assert.IsNull( sut.Album );
			Assert.IsFalse( sut.AlbumValid );
		}

		[Test]
		public void AlbumFocusShouldRequestAlbum() {
			var testable = new TestableAlbumViewModel();
			var album = new DbAlbum { Artist = 1, Name = "album name" };

			testable.Mock<IAlbumProvider>().Setup( m => m.GetAlbum( It.IsAny<long>())).Returns( album ).Verifiable( "GetAlbum not called." );

			var sut = testable.ClassUnderTest;
			sut.Handle( new Events.AlbumFocusRequested( album.Artist, album.DbId ));

			testable.Mock<IAlbumProvider>().Verify();
			Assert.IsNotNull( sut.Album );
			Assert.IsTrue( sut.AlbumValid );
		}

		[Test]
		public void DifferentArtistFocusShouldClearAlbum() {
			var testable = new TestableAlbumViewModel();
			var album = new DbAlbum { Artist = 1, Name = "album name" };

			testable.Mock<IAlbumProvider>().Setup( m => m.GetAlbum( It.IsAny<long>())).Returns( album );

			var sut = testable.ClassUnderTest;
			sut.Handle( new Events.AlbumFocusRequested( album.Artist, album.DbId ));
			sut.Handle( new Events.ArtistFocusRequested( album.Artist + 1 ));

			Assert.IsNull( sut.Album );
			Assert.IsFalse( sut.AlbumValid );
		}

		[Test]
		public void SecondAlbumRequestShouldNotRequestAlbum() {
			var testable = new TestableAlbumViewModel();
			var album = new DbAlbum { Artist = 1, Name = "album name" };

			testable.Mock<IAlbumProvider>().Setup( m => m.GetAlbum( It.IsAny<long>())).Returns( album ).Verifiable();

			var sut = testable.ClassUnderTest;
			sut.Handle( new Events.AlbumFocusRequested( album.Artist, album.DbId ));
			sut.Handle( new Events.AlbumFocusRequested( album.Artist, album.DbId ));

			testable.Mock<IAlbumProvider>().Verify( m => m.GetAlbum( It.IsAny<long>()), Times.Once());
		}

		[Test]
		public void SameArtistRequestShouldNotClearAlbum() {
			var testable = new TestableAlbumViewModel();
			var album = new DbAlbum { Artist = 1, Name = "album name" };

			testable.Mock<IAlbumProvider>().Setup( m => m.GetAlbum( It.IsAny<long>())).Returns( album ).Verifiable();

			var sut = testable.ClassUnderTest;
			sut.Handle( new Events.AlbumFocusRequested( album.Artist, album.DbId ));
			sut.Handle( new Events.ArtistFocusRequested( album.Artist ));

			testable.Mock<IAlbumProvider>().Verify( m => m.GetAlbum( It.IsAny<long>()), Times.Once());
			Assert.IsNotNull( sut.Album );
		}

		[Test]
		public void AlbumFocusShouldRequestAlbumSupportInfo() {
			var testable = new TestableAlbumViewModel();
			var album = new DbAlbum { Artist = 1, Name = "album name" };
			var coverList = new [] { new Artwork( new DbArtwork( 1, ContentType.AlbumCover )) };
			var artworkList = new [] { new Artwork( new DbArtwork( 2, ContentType.AlbumArtwork )) };
			var infoList = new [] { new TextInfo( new DbTextInfo( 3, ContentType.TextInfo )) };
			var supportInfo = new AlbumSupportInfo( coverList, artworkList, infoList );

			testable.Mock<IAlbumProvider>().Setup( m => m.GetAlbumSupportInfo( It.IsAny<long>())).Returns( supportInfo ).Verifiable( "GetAlbumSupportInfo not called." );

			var sut = testable.ClassUnderTest;
			sut.Handle( new Events.AlbumFocusRequested( album.Artist, album.DbId ));

			testable.Mock<IAlbumProvider>().Verify();
		}

		[Test]
		public void ShouldSelectUserSelectedAlbumCover() {
			var testable = new TestableAlbumViewModel();
			var album = new DbAlbum { Artist = 1, Name = "album name" };
			var artworkList =  new [] { new Artwork( new DbArtwork( 1, ContentType.AlbumCover )) };
			var infoList = new [] { new TextInfo( new DbTextInfo( 3, ContentType.TextInfo )) };
			var artwork1 = new Artwork( new DbArtwork( 2, ContentType.AlbumArtwork ));
			var artwork2 =  new Artwork( new DbArtwork( 2, ContentType.AlbumArtwork ) { IsUserSelection = true });
			var coverList = new [] { artwork1, artwork2 };
			var supportInfo = new AlbumSupportInfo( coverList, artworkList, infoList );

			testable.Mock<IAlbumProvider>().Setup( m => m.GetAlbumSupportInfo( It.IsAny<long>())).Returns( supportInfo );

			var sut = testable.ClassUnderTest;
			sut.Handle( new Events.AlbumFocusRequested( album.Artist, album.DbId ));

			sut.AlbumCover.Id.Should().Be( artwork2.DbId );
		}

		[Test]
		public void ShouldSelectAlbumArtworkNamedFront() {
			var testable = new TestableAlbumViewModel();
			var album = new DbAlbum { Artist = 1, Name = "album name" };
			var coverList = new Artwork[0];
			var infoList = new [] { new TextInfo( new DbTextInfo( 3, ContentType.TextInfo )) };
			var artwork1 = new Artwork( new DbArtwork( 2, ContentType.AlbumArtwork ));
			var artwork2 =  new Artwork( new DbArtwork( 2, ContentType.AlbumArtwork ) { Name = "this is the front" });
			var artworkList =  new [] { artwork1, artwork2 };
			var supportInfo = new AlbumSupportInfo( coverList, artworkList, infoList );

			testable.Mock<IAlbumProvider>().Setup( m => m.GetAlbumSupportInfo( It.IsAny<long>())).Returns( supportInfo );

			var sut = testable.ClassUnderTest;
			sut.Handle( new Events.AlbumFocusRequested( album.Artist, album.DbId ));

			sut.AlbumCover.Id.Should().Be( artwork2.DbId );
		}

		[Test]
		public void ShouldSelectSomeArtwork() {
			var testable = new TestableAlbumViewModel();
			var album = new DbAlbum { Artist = 1, Name = "album name" };
			var coverList = new [] { new Artwork( new DbArtwork( 1, ContentType.AlbumArtwork )) }; 
			var infoList = new [] { new TextInfo( new DbTextInfo( 3, ContentType.TextInfo )) };
			var artwork1 = new Artwork( new DbArtwork( 2, ContentType.AlbumArtwork ));
			var artwork2 =  new Artwork( new DbArtwork( 2, ContentType.AlbumArtwork ));
			var artworkList =  new [] { artwork1, artwork2 };
			var supportInfo = new AlbumSupportInfo( coverList, artworkList, infoList );

			testable.Mock<IAlbumProvider>().Setup( m => m.GetAlbumSupportInfo( It.IsAny<long>())).Returns( supportInfo );

			var sut = testable.ClassUnderTest;
			sut.Handle( new Events.AlbumFocusRequested( album.Artist, album.DbId ));

			sut.AlbumCover.Id.Should().NotBe( Constants.cDatabaseNullOid );
		}
	}
}
