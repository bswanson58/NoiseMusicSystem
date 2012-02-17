using System.Threading.Tasks;
using System.Windows.Input;
using FluentAssertions.EventMonitoring;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
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
	internal class TestableArtistViewModel : Testable<ArtistViewModel> {
		private readonly TaskScheduler		mTaskScheduler;
 
		public TestableArtistViewModel() {
			// Set tpl tasks to use the current thread only.
			mTaskScheduler = new CurrentThreadTaskScheduler();

			Mock<ITagManager>().Setup(  m => m.GetGenre( It.IsAny<long>())).Returns( new DbGenre( 1 ) { Name = "test genre" });
		}

		public override ArtistViewModel ClassUnderTest {
			get {
				var		retValue = base.ClassUnderTest;

				if( retValue != null ) {
					retValue.ArtistTaskHandler = new TaskHandler<DbArtist>( mTaskScheduler, mTaskScheduler );
					retValue.ArtworkTaskHandler = new TaskHandler<Artwork>( mTaskScheduler, mTaskScheduler );
				}

				return( retValue );
			}
		}
	}

	[TestFixture]
	public class ArtistViewModelTests {

		[SetUp]
		public void Setup() {
			NoiseLogger.Current = new Mock<ILog>().Object;

			// Set the ui dispatcher to run on the current thread.
			Execute.ResetWithoutDispatcher();

			// Set up the AutoMapper configurations.
			MappingConfiguration.Configure();
		}

		[Test]
		public void CanCreateArtistViewModel() {
			var sut = new TestableArtistViewModel().ClassUnderTest;

			Assert.IsNull( sut.Artist );
		}

		[Test]
		public void ArtistFocusShouldRequestArtist() {
			var testable = new TestableArtistViewModel();
			var artist = new DbArtist { Name = "artist name" };

			testable.Mock<IArtistProvider>().Setup( m => m.GetArtist( It.IsAny<long>())).Returns( artist ).Verifiable( "GetArtist not called." );

			var sut = testable.ClassUnderTest;
			sut.Handle( new Events.ArtistFocusRequested( artist.DbId ));

			testable.Mock<IArtistProvider>().Verify();
			Assert.IsNotNull( sut.Artist );
		}

		[Test]
		public void ArtistFocusShouldRequestArtistImage() {
			var testable = new TestableArtistViewModel();
			var artist = new DbArtist { Name = "artist name" };

			var dbArtwork = new DbArtwork( artist.DbId, ContentType.ArtistPrimaryImage );
			var artwork = new Artwork( dbArtwork ) { Image = new byte[] { 0, 1, 2, 3 }};

			testable.Mock<IArtistProvider>().Setup( m => m.GetArtist( It.IsAny<long>())).Returns( artist );
			testable.Mock<IArtworkProvider>().Setup( m => m.GetArtistArtwork( It.Is<long>( p => p == artist.DbId ),
																			  It.Is<ContentType>( p => p == ContentType.ArtistPrimaryImage ))).Returns( artwork ).Verifiable();

			var sut = testable.ClassUnderTest;
			sut.Handle( new Events.ArtistFocusRequested( artist.DbId ));

			testable.Mock<IArtworkProvider>().Verify();
			Assert.IsNotNull( sut.ArtistImage );
		}

		[Test]
		public void ArtistFocusShouldTriggerPropertyChanges() {
			var testable = new TestableArtistViewModel();
			var artist = new DbArtist { Name = "artist name" };

			testable.Mock<IArtistProvider>().Setup( m => m.GetArtist( It.IsAny<long>() ) ).Returns( artist );

			var sut = testable.ClassUnderTest;
			sut.MonitorEvents();

			sut.Handle( new Events.ArtistFocusRequested( artist.DbId ));

			DispatcherPump.DoEvents();

			sut.ShouldRaisePropertyChangeFor( m => m.Artist );
			sut.ShouldRaisePropertyChangeFor( m => m.ArtistValid );
			sut.ShouldRaisePropertyChangeFor( m => m.ArtistWebsite );
		}

		[Test]
		public void SameArtistFocusShouldNotRetrieveArtistAgain() {
			var testable = new TestableArtistViewModel();
			var artist = new DbArtist { Name = "artist name" };

			testable.Mock<IArtistProvider>().Setup( m => m.GetArtist( It.Is<long>( p => p == artist.DbId ))).Returns( artist );

			var sut = testable.ClassUnderTest;

			sut.Handle( new Events.ArtistFocusRequested( artist.DbId ));
			sut.Handle( new Events.ArtistFocusRequested( artist.DbId ));

			testable.Mock<IArtistProvider>().Verify( m => m.GetArtist( It.IsAny<long>()), Times.Exactly( 1 ));
		}

		[Test]
		public void AlbumFocusShouldRequestArtist() {
			var testable = new TestableArtistViewModel();
			var artist = new DbArtist();
			var album = new DbAlbum { Artist = artist.DbId };

			testable.Mock<IArtistProvider>().Setup( m => m.GetArtist( It.Is<long>( p => p == artist.DbId ))).Returns( artist ).Verifiable();

			var sut = testable.ClassUnderTest;

			sut.Handle( new Events.AlbumFocusRequested( album ));

			testable.Mock<IArtistProvider>().Verify();
		}

		[Test]
		public void SecondAlbumFocusShouldNotRequestArtist() {
			var testable = new TestableArtistViewModel();
			var artist = new DbArtist();
			var album1 = new DbAlbum { Artist = artist.DbId, Name = "first album" };
			var album2 = new DbAlbum { Artist = artist.DbId, Name = "second album" };

			testable.Mock<IArtistProvider>().Setup( m => m.GetArtist( It.Is<long>( p => p == artist.DbId ))).Returns( artist );

			var sut = testable.ClassUnderTest;

			sut.Handle( new Events.AlbumFocusRequested( album1 ));
			sut.Handle( new Events.AlbumFocusRequested( album2 ));

			testable.Mock<IArtistProvider>().Verify( m => m.GetArtist( It.IsAny<long>()), Times.Exactly( 1 ));
		}

		[Test]
		public void EditArtistRequestCanCancel() {
			var testable = new TestableArtistViewModel();
			var artist = new DbArtist();

			testable.Mock<IArtistProvider>().Setup( m => m.GetArtist( It.IsAny<long>())).Returns( artist );
			testable.Mock<IArtistProvider>().Setup( m => m.GetArtistForUpdate( artist.DbId )).Verifiable();

			var sut = testable.ClassUnderTest;

			sut.ArtistEditRequest.Raised += OnArtistEditRequestCancel;

			sut.Handle( new Events.ArtistFocusRequested( artist.DbId ));
			var editCommand = ( testable.ClassUnderTest as dynamic ).EditArtist as ICommand;

			Assert.IsNotNull( editCommand );
			Assert.IsTrue( editCommand.CanExecute( null ));

			editCommand.Execute( null );

			testable.Mock<IArtistProvider>().Verify( m => m.GetArtistForUpdate( It.IsAny<long>()), Times.Never());
		}

		[Test]
		public void EditArtistRequestCanUpdate() {
			var testable = new TestableArtistViewModel();
			var databaseShell = new Mock<IDatabaseShell> { DefaultValue = DefaultValue.Mock };

			var artist = new DbArtist();

			testable.Mock<IArtistProvider>().Setup( m => m.GetArtist( It.IsAny<long>())).Returns( artist );
			testable.Mock<IArtistProvider>().Setup( m => m.GetArtistForUpdate( artist.DbId ))
				.Returns( new DataUpdateShell<DbArtist>( databaseShell.Object, artist )).Verifiable();

			var sut = testable.ClassUnderTest;

			sut.ArtistEditRequest.Raised += OnArtistEditRequestConfirm;

			sut.Handle( new Events.ArtistFocusRequested( artist.DbId ));
			var editCommand = ( testable.ClassUnderTest as dynamic ).EditArtist as ICommand;

			Assert.IsNotNull( editCommand );

			editCommand.Execute( null );

			testable.Mock<IArtistProvider>().Verify( m => m.GetArtistForUpdate( It.IsAny<long>()), Times.Once());
		}

		[Test]
		public void CannotEditArtistWithoutArtist() {
			var testable = new TestableArtistViewModel();

			var sut = testable.ClassUnderTest;
			sut.ArtistEditRequest.Raised += OnArtistEditRequestCancel;

			var editCommand = ( testable.ClassUnderTest as dynamic ).EditArtist as ICommand;

			Assert.IsNotNull( editCommand );
			Assert.IsFalse( editCommand.CanExecute( null ));
		}

		private void OnArtistEditRequestCancel( object sender, InteractionRequestedEventArgs e ) {
			var confirmation = e.Context as Confirmation;

			Assert.IsNotNull( confirmation );

			confirmation.Confirmed = false;
			e.Callback.Invoke();
		}

		private void OnArtistEditRequestConfirm( object sender, InteractionRequestedEventArgs e ) {
			var confirmation = e.Context as Confirmation;

			Assert.IsNotNull( confirmation );

			confirmation.Confirmed = true;
			e.Callback.Invoke();
		}
	}
}
