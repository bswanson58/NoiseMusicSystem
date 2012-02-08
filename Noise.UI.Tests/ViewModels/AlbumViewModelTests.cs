using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Support;
using Noise.UI.ViewModels;
using ReusableBits.Mvvm.ViewModelSupport;
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
			Execute.ResetWithoutDispatcher();

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
		public void DifferentAritsFocusShouldClearAlbum() {
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
	}
}
