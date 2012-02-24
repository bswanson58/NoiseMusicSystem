using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using FluentAssertions;
using System.Threading.Tasks;
using FluentAssertions.EventMonitoring;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
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
		public void AlbumFocusShouldTriggerPropertyChanges() {
			var testable = new TestableAlbumViewModel();
			var album = new DbAlbum { Artist = 1, Name = "album name" };

			var coverList = new [] { new Artwork( new DbArtwork( 1, ContentType.AlbumCover )) };
			var artworkList = new [] { new Artwork( new DbArtwork( 2, ContentType.AlbumArtwork )) };
			var infoList = new [] { new TextInfo( new DbTextInfo( 3, ContentType.TextInfo )) };
			var supportInfo = new AlbumSupportInfo( coverList, artworkList, infoList );

			testable.Mock<IAlbumProvider>().Setup( m => m.GetAlbum( It.Is<long>( p => p == album.DbId ))).Returns( album );
			testable.Mock<IAlbumProvider>().Setup( m => m.GetAlbumSupportInfo( It.IsAny<long>())).Returns( supportInfo );

			var sut = testable.ClassUnderTest;
			sut.MonitorEvents();
			sut.Handle( new Events.AlbumFocusRequested( album.Artist, album.DbId ));

			sut.ShouldRaisePropertyChangeFor( p => p.Album );
			sut.ShouldRaisePropertyChangeFor( p => p.AlbumArtwork );
			sut.ShouldRaisePropertyChangeFor( p => p.AlbumCover );
			sut.ShouldRaisePropertyChangeFor( p => p.AlbumValid );
		}

		[Test]
		public void ShouldDisplayProperCategories() {
			var testable = new TestableAlbumViewModel();
			var album = new DbAlbum();

			testable.Mock<IAlbumProvider>().Setup( m => m.GetAlbum( It.IsAny<long>())).Returns( album );

			var tag1 = new DbTag( eTagGroup.User, "tag one" );
			var tag2 = new DbTag( eTagGroup.User, "tag two" );
			var tagList = new List<DbTag> { tag1, tag2 };
			var tagProvider = new Mock<IDataProviderList<DbTag>>();
			var idProvider =new Mock<IDataProviderList<long>>(); 
 
			tagProvider.Setup( m => m.List ).Returns( tagList );
			idProvider.Setup( m => m.List ).Returns( new [] { tag2.DbId });
			testable.Mock<ITagProvider>().Setup( m => m.GetTagList( It.Is<eTagGroup>( p => p == eTagGroup.User ))).Returns( tagProvider.Object );
			testable.Mock<IAlbumProvider>().Setup( m => m.GetAlbumCategories( It.IsAny<long>())).Returns( idProvider.Object );

			var sut = testable.ClassUnderTest;
			sut.MonitorEvents();

			sut.Handle( new Events.AlbumFocusRequested( album ));

			sut.AlbumCategories.Should().Be( tag2.Name );
			sut.HaveAlbumCategories.Should().Be( true );
			sut.ShouldRaisePropertyChangeFor( p => p.AlbumCategories );
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

		[Test]
		public void AllArtworkShouldBeDisplayed() {
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

			sut.AlbumArtwork.Should().HaveCount( 3 );
		}

		[Test]
		public void MissingArtworkShouldBeDisplayForNoArtwork() {
			var testable = new TestableAlbumViewModel();
			var album = new DbAlbum { Artist = 1, Name = "album name" };
			var supportInfo = new AlbumSupportInfo( new Artwork[0], new Artwork[0], new TextInfo[0] );

			testable.Mock<IAlbumProvider>().Setup( m => m.GetAlbumSupportInfo( It.IsAny<long>())).Returns( supportInfo );

			var sut = testable.ClassUnderTest;
			sut.Handle( new Events.AlbumFocusRequested( album.Artist, album.DbId ));

			sut.AlbumCover.Id.Should().Be( 0 );
			Assert.IsNull( sut.AlbumCover.Image );
		}

		[Test]
		public void DatabaseUpdateShouldUpdateAlbum() {
			var testable = new TestableAlbumViewModel();
			var album = new DbAlbum { Artist = 1, Name = "album name" };

			testable.Mock<IAlbumProvider>().Setup( m => m.GetAlbum( It.IsAny<long>())).Returns( album );
			
			var sut = testable.ClassUnderTest;
			sut.Handle( new Events.AlbumFocusRequested( album.Artist, album.DbId ));

			album.Name = "updated name";
			sut.Handle( new Events.AlbumUserUpdate( album.DbId ));

			sut.Album.Name.Should().Be( album.Name );
		}

		[Test]
		public void EditAlbumRequestCanCancel() {
			var testable = new TestableAlbumViewModel();
			var album = new DbAlbum();

			testable.Mock<IAlbumProvider>().Setup( m => m.GetAlbum( It.IsAny<long>())).Returns( album );
			testable.Mock<IAlbumProvider>().Setup( m => m.GetAlbumForUpdate( album.DbId )).Verifiable();

			var sut = testable.ClassUnderTest;

			sut.AlbumEditRequest.Raised += OnAlbumEditRequestCancel;

			sut.Handle( new Events.AlbumFocusRequested( album ));
			var editCommand = ( testable.ClassUnderTest as dynamic ).EditAlbum as ICommand;

			Assert.IsNotNull( editCommand );
			Assert.IsTrue( editCommand.CanExecute( null ));

			editCommand.Execute( null );

			testable.Mock<IAlbumProvider>().Verify( m => m.GetAlbumForUpdate( It.IsAny<long>()), Times.Never());
		}

		private void OnAlbumEditRequestCancel( object sender, InteractionRequestedEventArgs e ) {
			var confirmation = e.Context as Confirmation;

			Assert.IsNotNull( confirmation );

			confirmation.Confirmed = false;
			e.Callback.Invoke();
		}

		[Test]
		public void EditAlbumRequestCanUpdateAlbum() {
			var testable = new TestableAlbumViewModel();
			var album = new DbAlbum();

			testable.Mock<IAlbumProvider>().Setup( m => m.GetAlbum( It.IsAny<long>())).Returns( album );
			testable.Mock<IAlbumProvider>().Setup( m => m.GetAlbumForUpdate( album.DbId )).Verifiable();

			var sut = testable.ClassUnderTest;

			sut.AlbumEditRequest.Raised += OnAlbumEditRequestConfirm;

			sut.Handle( new Events.AlbumFocusRequested( album ));
			var editCommand = ( testable.ClassUnderTest as dynamic ).EditAlbum as ICommand;

			Assert.IsNotNull( editCommand );
			Assert.IsTrue( editCommand.CanExecute( null ));

			editCommand.Execute( null );

			testable.Mock<IAlbumProvider>().Verify( m => m.GetAlbumForUpdate( It.IsAny<long>()), Times.Once());
		}

		[Test]
		public void EditAlbumRequestDoesUpdateAlbum() {
			var testable = new TestableAlbumViewModel();
			var album = new DbAlbum { Name = "original name", PublishedYear = 2000 };

			testable.Mock<IAlbumProvider>().Setup( m => m.GetAlbum( It.IsAny<long>())).Returns( album );

			var databaseShell = new Mock<IDatabaseShell> { DefaultValue = DefaultValue.Mock };
			databaseShell.Setup( m => m.Database.UpdateItem( album )).Verifiable();
			var updater =new Mock<IDataUpdateShell<DbAlbum>>();

 			updater.Setup( m => m.Item ).Returns( album );
			testable.Mock<IAlbumProvider>().Setup( m => m.GetAlbumForUpdate( album.DbId )).Returns( updater.Object );

			var sut = testable.ClassUnderTest;

			sut.AlbumEditRequest.Raised += OnAlbumEditRequestConfirm;

			sut.Handle( new Events.AlbumFocusRequested( album ));
			var editCommand = ( testable.ClassUnderTest as dynamic ).EditAlbum as ICommand;

			Assert.IsNotNull( editCommand );
			editCommand.Execute( null );

			databaseShell.Verify( m => m.Database.UpdateItem( album ), Times.Once());

			sut.Album.Name.Should().Be( "updated name" );
			sut.Album.PublishedYear.Should().Be( 2001 );
		}

		private void OnAlbumEditRequestConfirm( object sender, InteractionRequestedEventArgs e ) {
			var confirmation = e.Context as AlbumEditRequest;

			Assert.IsNotNull( confirmation );

			confirmation.ViewModel.Album.Name = "updated name";
			confirmation.ViewModel.Album.PublishedYear += 1;

			confirmation.Confirmed = true;
			e.Callback.Invoke();
		}

		[Test]
		public void EditCategoriesCanBeCancelled() {
			var testable = new TestableAlbumViewModel();
			var album = new DbAlbum { Name = "original name", PublishedYear = 2000 };

			testable.Mock<IAlbumProvider>().Setup( m => m.GetAlbum( It.IsAny<long>())).Returns( album );
			testable.Mock<IAlbumProvider>().Setup( m => m.SetAlbumCategories( album.Artist, album.DbId, It.IsAny<IEnumerable<long>>())).Verifiable();

			var sut = testable.ClassUnderTest;
			sut.AlbumCategoryEditRequest.Raised += OnAlbumCategoryEditRequestCancel;

			sut.Handle( new Events.AlbumFocusRequested( album ));
			var editCommand = ( testable.ClassUnderTest as dynamic ).EditCategories as ICommand;

			Assert.IsNotNull( editCommand );
			editCommand.Execute( null );

			testable.Mock<IAlbumProvider>().Verify( m => m.SetAlbumCategories( It.IsAny<long>(), It.IsAny<long>(), It.IsAny<IEnumerable<long>>()), Times.Never());
		}

		private void OnAlbumCategoryEditRequestCancel( object sender, InteractionRequestedEventArgs e ) {
			var confirmation = e.Context as Confirmation;

			Assert.IsNotNull( confirmation );

			confirmation.Confirmed = false;
			e.Callback.Invoke();
		}

		[Test]
		public void EditCategoriesCanUpdate() {
			var testable = new TestableAlbumViewModel();
			var album = new DbAlbum { Name = "original name", PublishedYear = 2000 };

			testable.Mock<IAlbumProvider>().Setup( m => m.GetAlbum( It.IsAny<long>())).Returns( album );
			testable.Mock<IAlbumProvider>().Setup( m => m.SetAlbumCategories( album.Artist, album.DbId, It.IsAny<IEnumerable<long>>())).Verifiable();

			var sut = testable.ClassUnderTest;
			sut.AlbumCategoryEditRequest.Raised += OnAlbumCategoryEditRequestConfirm;

			sut.Handle( new Events.AlbumFocusRequested( album ));
			var editCommand = ( testable.ClassUnderTest as dynamic ).EditCategories as ICommand;

			Assert.IsNotNull( editCommand );
			editCommand.Execute( null );

			testable.Mock<IAlbumProvider>().Verify( m => m.SetAlbumCategories( It.IsAny<long>(), It.IsAny<long>(), It.IsAny<IEnumerable<long>>()), Times.Once());
		}

		private void OnAlbumCategoryEditRequestConfirm( object sender, InteractionRequestedEventArgs e ) {
			var confirmation = e.Context as AlbumCategoryEditInfo;

			Assert.IsNotNull( confirmation );

			if(( confirmation.ViewModel != null ) &&
			   ( confirmation.ViewModel.SelectedCategories.Any())) {
				confirmation.ViewModel.OnSelectionChanged( confirmation.ViewModel.SelectedCategories[0], false );
			}

			confirmation.Confirmed = true;
			e.Callback.Invoke();
		}

		[Test]
		public void EditCategoriesDoesUpdate() {
			var testable = new TestableAlbumViewModel();
			var album = new DbAlbum { Name = "original name", PublishedYear = 2000 };

			testable.Mock<IAlbumProvider>().Setup( m => m.GetAlbum( It.IsAny<long>())).Returns( album );
			testable.Mock<IAlbumProvider>().Setup( m => m.SetAlbumCategories( album.Artist, album.DbId, It.IsAny<IEnumerable<long>>()));

			var tag1 = new DbTag( eTagGroup.User, "tag one" );
			var tag2 = new DbTag( eTagGroup.User, "tag two" );
			var tagList = new List<DbTag> { tag1, tag2 };
			var tagProvider = new Mock<IDataProviderList<DbTag>>();
 			var idProvider = new Mock<IDataProviderList<long>>();
 
			tagProvider.Setup( m => m.List ).Returns( tagList );
			idProvider.Setup( m => m.List ).Returns(  new [] { tag1.DbId, tag2.DbId });
			testable.Mock<ITagProvider>().Setup( m => m.GetTagList( It.Is<eTagGroup>( p => p == eTagGroup.User ))).Returns( tagProvider.Object );
			testable.Mock<IAlbumProvider>().Setup( m => m.GetAlbumCategories( It.IsAny<long>())).Returns( idProvider.Object );

			var sut = testable.ClassUnderTest;
			sut.AlbumCategoryEditRequest.Raised += OnAlbumCategoryEditRequestConfirm;

			sut.Handle( new Events.AlbumFocusRequested( album ));
			var editCommand = ( testable.ClassUnderTest as dynamic ).EditCategories as ICommand;

			Assert.IsNotNull( editCommand );
			editCommand.Execute( null );

			sut.AlbumCategories.Should().Be( tag2.Name );
		}

		[Test]
		public void CannotDisplayEmptyArtworkList() {
			var testable = new TestableAlbumViewModel();
			var album = new DbAlbum();

			testable.Mock<IAlbumProvider>().Setup( m => m.GetAlbum( It.IsAny<long>())).Returns( album );

			var sut = testable.ClassUnderTest;
			sut.Handle( new Events.AlbumFocusRequested( album ));

			var editCommand = ( testable.ClassUnderTest as dynamic ).DisplayPictures as ICommand;
			Assert.IsNotNull( editCommand );
			editCommand.Execute( null );

			Assert.IsFalse( editCommand.CanExecute( null ));
		}

		[Test]
		public void CanDisplayAlbumArtworkList() {
			var testable = new TestableAlbumViewModel();
			var album = new DbAlbum();

			testable.Mock<IAlbumProvider>().Setup( m => m.GetAlbum( It.IsAny<long>())).Returns( album );

			var coverList = new [] { new Artwork( new DbArtwork( 1, ContentType.AlbumArtwork )) }; 
			var infoList = new [] { new TextInfo( new DbTextInfo( 3, ContentType.TextInfo )) };
			var artwork1 = new Artwork( new DbArtwork( 2, ContentType.AlbumArtwork ));
			var artwork2 =  new Artwork( new DbArtwork( 2, ContentType.AlbumArtwork ));
			var artworkList =  new [] { artwork1, artwork2 };
			var supportInfo = new AlbumSupportInfo( coverList, artworkList, infoList );

			testable.Mock<IAlbumProvider>().Setup( m => m.GetAlbumSupportInfo( It.IsAny<long>())).Returns( supportInfo );

			var sut = testable.ClassUnderTest;

			sut.Handle( new Events.AlbumFocusRequested( album ));

			var editCommand = ( testable.ClassUnderTest as dynamic ).DisplayPictures as ICommand;
			Assert.IsNotNull( editCommand );
			Assert.IsTrue( editCommand.CanExecute( null ));
		}

		[Test]
		public void DisplayArtworkCanBeCancelled() {
			var testable = new TestableAlbumViewModel();
			var album = new DbAlbum();

			testable.Mock<IAlbumProvider>().Setup( m => m.GetAlbum( It.IsAny<long>())).Returns( album );
			testable.Mock<IArtworkProvider>().Setup( m => m.GetArtworkForUpdate( It.IsAny<long>())).Verifiable();

			var coverList = new [] { new Artwork( new DbArtwork( 1, ContentType.AlbumArtwork )) }; 
			var infoList = new [] { new TextInfo( new DbTextInfo( 3, ContentType.TextInfo )) };
			var artwork1 = new Artwork( new DbArtwork( 2, ContentType.AlbumArtwork ));
			var artwork2 =  new Artwork( new DbArtwork( 2, ContentType.AlbumArtwork ));
			var artworkList =  new [] { artwork1, artwork2 };
			var supportInfo = new AlbumSupportInfo( coverList, artworkList, infoList );

			testable.Mock<IAlbumProvider>().Setup( m => m.GetAlbumSupportInfo( It.IsAny<long>())).Returns( supportInfo );

			var sut = testable.ClassUnderTest;
			sut.AlbumArtworkDisplayRequest.Raised += OnAlbumArtworkDisplayRequestCancel;

			sut.Handle( new Events.AlbumFocusRequested( album ));

			var editCommand = ( testable.ClassUnderTest as dynamic ).DisplayPictures as ICommand;
			Assert.IsNotNull( editCommand );
			editCommand.Execute( null );

			testable.Mock<IArtworkProvider>().Verify( m => m.GetArtworkForUpdate( It.IsAny<long>()), Times.Never());
		}

		private void OnAlbumArtworkDisplayRequestCancel( object sender, InteractionRequestedEventArgs e ) {
			var confirmation = e.Context as Confirmation;

			Assert.IsNotNull( confirmation );

			confirmation.Confirmed = false;
			e.Callback.Invoke();
		}

		[Test]
		public void DisplayArtworkCanUpdateArtwork() {
			var testable = new TestableAlbumViewModel();
			var album = new DbAlbum();

			testable.Mock<IAlbumProvider>().Setup( m => m.GetAlbum( It.IsAny<long>())).Returns( album );
			testable.Mock<IArtworkProvider>().Setup( m => m.GetArtworkForUpdate( It.IsAny<long>())).Verifiable();

			var coverList = new [] { new Artwork( new DbArtwork( 1, ContentType.AlbumCover )) }; 
			var infoList = new [] { new TextInfo( new DbTextInfo( 3, ContentType.TextInfo )) };
			var artwork1 = new Artwork( new DbArtwork( 2, ContentType.AlbumArtwork ) { IsUserSelection = true });
			var artwork2 =  new Artwork( new DbArtwork( 2, ContentType.AlbumArtwork ));
			var artworkList =  new [] { artwork1, artwork2 };
			var supportInfo = new AlbumSupportInfo( coverList, artworkList, infoList );

			testable.Mock<IAlbumProvider>().Setup( m => m.GetAlbumSupportInfo( It.IsAny<long>())).Returns( supportInfo );

			var sut = testable.ClassUnderTest;
			sut.AlbumArtworkDisplayRequest.Raised += OnAlbumArtworkDisplayRequestSetCover;

			sut.Handle( new Events.AlbumFocusRequested( album ));

			var editCommand = ( testable.ClassUnderTest as dynamic ).DisplayPictures as ICommand;
			Assert.IsNotNull( editCommand );
			editCommand.Execute( null );

			testable.Mock<IArtworkProvider>().Verify( m => m.GetArtworkForUpdate( It.IsAny<long>()), Times.Once());
		}

		[Test]
		public void DisplayArtworkCanSetUserSelectedCover() {
			var testable = new TestableAlbumViewModel();
			var album = new DbAlbum();

			testable.Mock<IAlbumProvider>().Setup( m => m.GetAlbum( It.IsAny<long>())).Returns( album );

			var coverList = new [] { new Artwork( new DbArtwork( 1, ContentType.AlbumCover )) }; 
			var infoList = new [] { new TextInfo( new DbTextInfo( 3, ContentType.TextInfo )) };
			var artwork1 = new Artwork( new DbArtwork( 2, ContentType.AlbumArtwork ) { IsUserSelection = true });
			var artwork2 =  new Artwork( new DbArtwork( 2, ContentType.AlbumArtwork ));
			var artworkList =  new [] { artwork1, artwork2 };
			var supportInfo = new AlbumSupportInfo( coverList, artworkList, infoList );

			testable.Mock<IAlbumProvider>().Setup( m => m.GetAlbumSupportInfo( It.IsAny<long>())).Returns( supportInfo );

			var sut = testable.ClassUnderTest;
			sut.AlbumArtworkDisplayRequest.Raised += OnAlbumArtworkDisplayRequestSetCover;

			sut.Handle( new Events.AlbumFocusRequested( album ));

			var editCommand = ( testable.ClassUnderTest as dynamic ).DisplayPictures as ICommand;
			Assert.IsNotNull( editCommand );
			editCommand.Execute( null );

			sut.AlbumCover.Id.Should().Be( artwork2.DbId );
		}

		private void OnAlbumArtworkDisplayRequestSetCover( object sender, InteractionRequestedEventArgs e ) {
			DispatcherPump.DoEvents();

			var confirmation = e.Context as AlbumArtworkDisplayInfo;

			Assert.IsNotNull( confirmation );

			foreach( var image in confirmation.ViewModel.AlbumImages ) {
				if(( image.IsImage ) &&
				   (!image.Artwork.IsUserSelection ) &&
				   ( image.Artwork.ContentType == ContentType.AlbumArtwork )) {
					image.SetPreferredImage();

					break;
				}
			}

			confirmation.Confirmed = true;
			e.Callback.Invoke();
		}
	}
}
