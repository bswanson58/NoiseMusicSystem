using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using FluentAssertions;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Moq;
using NUnit.Framework;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Interfaces;
using Noise.UI.ViewModels;
using ReusableBits;
using ReusableBits.TestSupport.Mocking;
using ReusableBits.TestSupport.Threading;

namespace Noise.UI.Tests.ViewModels {
	internal class TestableArtistViewModel : Testable<ArtistViewModel> {
		private readonly TaskScheduler		mTaskScheduler;
		private readonly Subject<DbArtist>	mArtistSubject;
 
		public TestableArtistViewModel() {
			// Set tpl tasks to use the current thread only.
			mTaskScheduler = new CurrentThreadTaskScheduler();

			mArtistSubject = new Subject<DbArtist>();

			Mock<ITagManager>().Setup(  m => m.GetGenre( It.IsAny<long>())).Returns( new DbGenre( 1 ) { Name = "test genre" });
			Mock<ISelectionState>().Setup( m => m.CurrentArtistChanged ).Returns( mArtistSubject.AsObservable());
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

		public void FireArtistChanged( DbArtist artist ) {
			mArtistSubject.OnNext( artist );
		}
	}

	[TestFixture]
	public class ArtistViewModelTests {

		[SetUp]
		public void Setup() {
			NoiseLogger.Current = new Mock<INoiseLog>().Object;

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

			testable.FireArtistChanged( artist );
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
			testable.Mock<IMetadataManager>().Setup( m => m.GetArtistArtwork( It.Is<string>( p => p == artist.Name ))).Returns( artwork );

			var sut = testable.ClassUnderTest;

			testable.FireArtistChanged( artist );
			testable.Mock<IMetadataManager>().Verify();
			Assert.IsNotNull( sut.ArtistImage );
		}

		[Test]
		public void ArtistFocusShouldTriggerPropertyChanges() {
			var testable = new TestableArtistViewModel();
			var artist = new DbArtist { Name = "artist name" };

			testable.Mock<IArtistProvider>().Setup( m => m.GetArtist( It.IsAny<long>() ) ).Returns( artist );

			var sut = testable.ClassUnderTest;
			sut.MonitorEvents();

			testable.FireArtistChanged( artist );

			DispatcherPump.DoEvents();

			sut.ShouldRaisePropertyChangeFor( m => m.Artist );
			sut.ShouldRaisePropertyChangeFor( m => m.ArtistValid );
			sut.ShouldRaisePropertyChangeFor( m => m.ArtistWebsite );
		}

		[Test]
		public void EditArtistRequestCanCancel() {
			var testable = new TestableArtistViewModel();
			var artist = new DbArtist();

			testable.Mock<IArtistProvider>().Setup( m => m.GetArtist( It.IsAny<long>())).Returns( artist );
			testable.Mock<IArtistProvider>().Setup( m => m.GetArtistForUpdate( artist.DbId )).Verifiable();

			var sut = testable.ClassUnderTest;

			sut.ArtistEditRequest.Raised += OnArtistEditRequestCancel;

			testable.FireArtistChanged( artist );
			var editCommand = ( testable.ClassUnderTest as dynamic ).EditArtist as ICommand;

			Assert.IsNotNull( editCommand );
			Assert.IsTrue( editCommand.CanExecute( null ));

			editCommand.Execute( null );

			testable.Mock<IArtistProvider>().Verify( m => m.GetArtistForUpdate( It.IsAny<long>()), Times.Never());
		}

		[Test]
		public void EditArtistRequestCanUpdate() {
			var testable = new TestableArtistViewModel();
			var updater = new Mock<IDataUpdateShell<DbArtist>>(); 

			var artist = new DbArtist();

			updater.Setup( m => m.Item ).Returns( artist );
			testable.Mock<IArtistProvider>().Setup( m => m.GetArtist( It.IsAny<long>())).Returns( artist );
			testable.Mock<IArtistProvider>().Setup( m => m.GetArtistForUpdate( artist.DbId )).Returns( updater.Object ).Verifiable();

			var sut = testable.ClassUnderTest;

			sut.ArtistEditRequest.Raised += OnArtistEditRequestConfirm;

			testable.FireArtistChanged( artist );
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
