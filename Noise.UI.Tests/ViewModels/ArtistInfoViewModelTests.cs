using System.Threading.Tasks;
using Caliburn.Micro;
using Moq;
using NUnit.Framework;
using Noise.Infrastructure;
using Noise.UI.ViewModels;
using ReusableBits;
using ReusableBits.TestSupport.Mocking;
using ReusableBits.TestSupport.Threading;
using ILog = Noise.Infrastructure.Interfaces.ILog;

namespace Noise.UI.Tests.ViewModels {
	internal class TestableArtistInfoViewModel : Testable<ArtistInfoViewModel> {
		private readonly TaskScheduler	mTaskScheduler;
		private TaskHandler				mTaskHandler;

		public TestableArtistInfoViewModel() {
			// Set tpl tasks to use the current thread only.
			mTaskScheduler = new CurrentThreadTaskScheduler();
		}

		public override ArtistInfoViewModel ClassUnderTest {
			get {
				var		retValue = base.ClassUnderTest;

				if(( retValue != null ) &&
				   ( mTaskHandler == null )) {
					mTaskHandler = new TaskHandler( mTaskScheduler, mTaskScheduler );
				
					retValue.TaskHandler = mTaskHandler;
				}

				return( retValue );
			}
		}
	}

	[TestFixture]
	public class ArtistInfoViewModelTests {

		[SetUp]
		public void Setup() {
			NoiseLogger.Current = new Mock<ILog>().Object;

			// Set the ui dispatcher to run on the current thread.
			Execute.ResetWithoutDispatcher();
		}

		[Test]
		public void CanCreateArtistInfoViewModel() {
			var sut = new TestableArtistInfoViewModel().ClassUnderTest;

			Assert.IsFalse( sut.ArtistValid );
		}
/*
		[Test]
		public void SameArtistFocusShouldNotRetrieveArtistAgain() {
			var testable = new TestableArtistInfoViewModel();
			var artist = new DbArtist();

			testable.Mock<IArtistProvider>().Setup( m => m.GetArtistSupportInfo( It.IsAny<long>())).Returns( (ArtistSupportInfo)null );

			var sut = testable.ClassUnderTest;

			sut.Handle( new Events.ArtistFocusRequested( artist.DbId ));
			sut.Handle( new Events.ArtistFocusRequested( artist.DbId ));

			testable.Mock<IArtistProvider>().Verify( m => m.GetArtistSupportInfo( It.IsAny<long>()), Times.Once(), "GetArtistSupportInfo request" );
		}

		[Test]
		public void AlbumFocusShouldRequestArtist() {
			var testable = new TestableArtistInfoViewModel();
			var artist = new DbArtist();
			var album = new DbAlbum { Artist = artist.DbId };

			testable.Mock<IArtistProvider>().Setup( m => m.GetArtistSupportInfo( It.Is<long>( p => p == artist.DbId ))).Returns( (ArtistSupportInfo)null ).Verifiable();

			var sut = testable.ClassUnderTest;

			sut.Handle( new Events.AlbumFocusRequested( album ));

			testable.Mock<IArtistProvider>().Verify();
		}

		[Test]
		public void SecondAlbumFocusShouldNotRequestArtist() {
			var testable = new TestableArtistInfoViewModel();
			var artist = new DbArtist();
			var album1 = new DbAlbum { Artist = artist.DbId, Name = "first album" };
			var album2 = new DbAlbum { Artist = artist.DbId, Name = "second album" };

			testable.Mock<IArtistProvider>().Setup( m => m.GetArtistSupportInfo( It.Is<long>( p => p == artist.DbId ))).Returns( (ArtistSupportInfo)null );

			var sut = testable.ClassUnderTest;

			sut.Handle( new Events.AlbumFocusRequested( album1 ));
			sut.Handle( new Events.AlbumFocusRequested( album2 ));

			testable.Mock<IArtistProvider>().Verify( m => m.GetArtistSupportInfo( It.IsAny<long>()), Times.Exactly( 1 ));
		}

		[Test]
		public void ArtistInfoUpdateEventShouldRetrieveArtistInfo() {
			var testable = new TestableArtistInfoViewModel();
			var artist = new DbArtist { Name = "my favorite artist" };

			testable.Mock<IArtistProvider>().Setup( m => m.GetArtistSupportInfo( It.IsAny<long>())).Returns( (ArtistSupportInfo)null );

			var sut = testable.ClassUnderTest;

			sut.Handle( new Events.ArtistFocusRequested( artist.DbId ));
			sut.Handle( new Events.ArtistMetadataUpdated( artist.Name ));

			testable.Mock<IArtistProvider>().Verify( m => m.GetArtistSupportInfo( It.IsAny<long>()), Times.Exactly( 2 ), "GetArtistSupportInfo request" );
		}
 */
	}
}
