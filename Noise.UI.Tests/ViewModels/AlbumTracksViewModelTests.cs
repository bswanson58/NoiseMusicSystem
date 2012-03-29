using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.ViewModels;
using ReusableBits;
using ReusableBits.TestSupport.Mocking;
using ReusableBits.TestSupport.Threading;

namespace Noise.UI.Tests.ViewModels {
	internal class TestableAlbumTracksViewModel : Testable<AlbumTracksViewModel> {
		private readonly TaskScheduler		mTaskScheduler;
 

		public TestableAlbumTracksViewModel() {
			// Set tpl tasks to use the current thread only.
			mTaskScheduler = new CurrentThreadTaskScheduler();
		}

		public override AlbumTracksViewModel ClassUnderTest {
			get {
				var		retValue = base.ClassUnderTest;

				if( retValue != null ) {
					retValue.TracksRetrievalTaskHandler = new TaskHandler( mTaskScheduler, mTaskScheduler );
				}

				return( retValue );
			}
		}
	}

	[TestFixture]
	public class AlbumTracksViewModelTests {
		[SetUp]
		public void Setup() {
			NoiseLogger.Current = new Mock<ILog>().Object;

			// Set the ui dispatcher to run on the current thread.
			Caliburn.Micro.Execute.ResetWithoutDispatcher();

			// Set up the AutoMapper configurations.
			MappingConfiguration.Configure();
		}

		[Test]
		public void CanCreateAlbumTracksViewModel() {
			var sut = new TestableAlbumTracksViewModel().ClassUnderTest;

			sut.TrackList.Should().HaveCount( 0 );
		}

		[Test]
		public void AlbumRequestShouldRequestTrackList() {
			var testable = new TestableAlbumTracksViewModel();
			var provider = new Mock<IDataProviderList<DbTrack>>(); 

			provider.Setup( m => m.List ).Returns( new List<DbTrack>());
			testable.Mock<ITrackProvider>().Setup( m => m.GetTrackList( It.IsAny<long>())).Returns( provider.Object ).Verifiable();

			var sut= testable.ClassUnderTest;
			var album = new DbAlbum { Artist = 1 };

			sut.Handle( new Events.AlbumFocusRequested( album ));

			testable.Mock<ITrackProvider>().Verify();
		}

		[Test]
		public void AlbumRequestShouldPresentTrackList() {
			var testable = new TestableAlbumTracksViewModel();

			var track1 = new DbTrack();
			var track2 = new DbTrack();
			var provider = new Mock<IDataProviderList<DbTrack>>();
 
			provider.Setup( m => m.List ).Returns(  new List<DbTrack> { track1, track2 });
			testable.Mock<ITrackProvider>().Setup( m => m.GetTrackList( It.IsAny<long>())).Returns( provider.Object );

			var sut= testable.ClassUnderTest;
			var album = new DbAlbum { Artist = 1 };

			sut.Handle( new Events.AlbumFocusRequested( album ));

			sut.TrackList.Should().HaveCount( 2 );
		}

		[Test]
		public void DifferentArtistShouldClearTrackList() {
			var testable = new TestableAlbumTracksViewModel();

			var track1 = new DbTrack();
			var track2 = new DbTrack();
			var provider = new Mock<IDataProviderList<DbTrack>>();
 
			provider.Setup( m => m.List ).Returns(  new List<DbTrack> { track1, track2 });
			testable.Mock<ITrackProvider>().Setup( m => m.GetTrackList( It.IsAny<long>())).Returns( provider.Object );

			var sut= testable.ClassUnderTest;
			var album = new DbAlbum { Artist = 1 };

			sut.Handle( new Events.AlbumFocusRequested( album ));
			sut.Handle( new Events.ArtistFocusRequested( 2 ));

			sut.TrackList.Should().HaveCount( 0 );
		}
		
		[Test]
		public void TrackListShouldBePresentedInVolumeTrackOrder() {
			var testable = new TestableAlbumTracksViewModel();

			var track1 = new DbTrack { VolumeName = "Disc 2", TrackNumber = 1 };
			var track2 = new DbTrack { VolumeName = "Disc 1", TrackNumber = 3 };
			var track3 = new DbTrack { VolumeName = "Disc 2", TrackNumber = 7 };
			var provider = new Mock<IDataProviderList<DbTrack>>();
 
			provider.Setup( m => m.List ).Returns(  new List<DbTrack> { track1, track2, track3 });
			testable.Mock<ITrackProvider>().Setup( m => m.GetTrackList( It.IsAny<long>())).Returns( provider.Object );

			var sut= testable.ClassUnderTest;
			var album = new DbAlbum { Artist = 1 };

			sut.Handle( new Events.AlbumFocusRequested( album ));

			sut.TrackList.Should().HaveCount( 3 );
			sut.TrackList[0].DbId.Should().Be( track2.DbId );
			sut.TrackList[1].DbId.Should().Be( track1.DbId );
			sut.TrackList[2].DbId.Should().Be( track3.DbId );
		}

		[Test]
		public void AlbumPlayTimeShouldBeSumOfTrackTime() {
			var testable = new TestableAlbumTracksViewModel();

			var	time1 = new TimeSpan( 0, 3, 20 );
			var track1 = new DbTrack { DurationMilliseconds = (int)time1.TotalMilliseconds };
 			var time2 = new TimeSpan( 0, 2, 30 );
			var track2 = new DbTrack { DurationMilliseconds = (int)time2.TotalMilliseconds };
			var provider = new Mock<IDataProviderList<DbTrack>>();
 
			provider.Setup( m => m.List ).Returns(  new List<DbTrack> { track1, track2 });
			testable.Mock<ITrackProvider>().Setup( m => m.GetTrackList( It.IsAny<long>())).Returns( provider.Object );

			var sut= testable.ClassUnderTest;
			var album = new DbAlbum { Artist = 1 };

			sut.Handle( new Events.AlbumFocusRequested( album ));

			sut.AlbumPlayTime.Should().Be( time1 + time2 );
		}
	}
}
