﻿using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Moq;
using Noise.UI.Dto;
using NUnit.Framework;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Interfaces;
using Noise.UI.ViewModels;
using ReusableBits;
using ReusableBits.TestSupport.Mocking;
using ReusableBits.TestSupport.Threading;

namespace Noise.UI.Tests.ViewModels {
	internal class TestableAlbumTracksViewModel : Testable<AlbumTracksViewModel> {
		private readonly TaskScheduler		mTaskScheduler;
		private readonly Subject<DbAlbum>	mAlbumSubject;

		public TestableAlbumTracksViewModel() {
			// Set tpl tasks to use the current thread only.
			mTaskScheduler = new CurrentThreadTaskScheduler();
			mAlbumSubject = new Subject<DbAlbum>();

			Mock<ISelectionState>().Setup( m => m.CurrentAlbumChanged ).Returns( mAlbumSubject.AsObservable());
		}

		public override AlbumTracksViewModel ClassUnderTest {
			get {
				var		retValue = base.ClassUnderTest;

				if( retValue != null ) {
					retValue.TracksRetrievalTaskHandler = new TaskHandler<IEnumerable<UiTrack>>( mTaskScheduler, mTaskScheduler );
				}

				return( retValue );
			}
		}

		public void FireAlbumChanged( DbAlbum album ) {
			mAlbumSubject.OnNext( album );
		}
	}

	[TestFixture]
	public class AlbumTracksViewModelTests {
		[SetUp]
		public void Setup() {
			// Set up the AutoMapper configurations.
		    Mapper.Initialize( cfg => cfg.AddProfiles( "Noise.Ui" ));
		    Mapper.AssertConfigurationIsValid();
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

			testable.FireAlbumChanged( album );

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

			testable.FireAlbumChanged( album );
			
			sut.TrackList.Should().HaveCount( 2 );
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

			testable.FireAlbumChanged( album );

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

			testable.FireAlbumChanged( album );

			sut.AlbumPlayTime.Should().Be( time1 + time2 );
		}
	}
}
