using System.Collections.Generic;
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
					retValue.AlbumTaskHandler = new TaskHandler<DbAlbum>( mTaskScheduler, mTaskScheduler );
					retValue.AlbumSupportTaskHandler = new TaskHandler<AlbumSupportInfo>( mTaskScheduler, mTaskScheduler );
					retValue.AlbumTracksTaskHandler = new TaskHandler<IEnumerable<DbTrack>>( mTaskScheduler, mTaskScheduler );
					retValue.AlbumCategoriesTaskHandler = new TaskHandler<IEnumerable<long>>( mTaskScheduler, mTaskScheduler );
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
	}
}
