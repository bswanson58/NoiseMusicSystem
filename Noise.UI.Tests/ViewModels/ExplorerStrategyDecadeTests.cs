using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Dto;
using Noise.UI.Support;
using Noise.UI.ViewModels;
using ReusableBits.TestSupport.Mocking;
using ReusableBits.TestSupport.Threading;

namespace Noise.UI.Tests.ViewModels {
	internal class TestableExplorerStrategyDecade : Testable<ExplorerStrategyDecade> {
		private readonly DataTemplate	mDataTemplate;
		private readonly TaskScheduler	mTaskScheduler;

		public TestableExplorerStrategyDecade() {
			mDataTemplate = new DataTemplate();

			// Set tpl tasks to use the current thread only.
			mTaskScheduler = new CurrentThreadTaskScheduler();

			Mock<IResourceProvider>().Setup( m => m.RetrieveTemplate( It.IsAny<string>())).Returns( mDataTemplate );
			Mock<ITagManager>().Setup(  m => m.GetGenre( It.IsAny<long>())).Returns( new DbGenre( 1 ) { Name = "test genre" });
		}

		public override ExplorerStrategyDecade ClassUnderTest {
			get {
				var		retValue = base.ClassUnderTest;

				if( retValue != null ) {
					retValue.ArtistPopulateTask = new TaskHandler( mTaskScheduler, mTaskScheduler );
				}

				return( retValue );
			}
		}
	}

	[TestFixture]
	public class ExplorerStrategyDecadeTests {
		[SetUp]
		public void Setup() {
			NoiseLogger.Current = new Mock<ILog>().Object;

			// Set the ui dispatcher to run on the current thread.
			ReusableBits.Mvvm.ViewModelSupport.Execute.ResetWithoutDispatcher();

			// Set up the AutoMapper configurations.
			MappingConfiguration.Configure();
		}

		[Test]
		public void CanCreateStrategy() {
			var testable = new TestableExplorerStrategyDecade();
			var sut = testable.ClassUnderTest;

			Assert.IsNotNullOrEmpty( sut.StrategyId );
			Assert.IsNotNullOrEmpty( sut.StrategyName );
		}

		[Test]
		public void CanInitializeStrategy() {
			var testable = new TestableExplorerStrategyDecade();
			var viewModel = new Mock<ILibraryExplorerViewModel>();
			var sut = testable.ClassUnderTest;

			sut.Initialize( viewModel.Object );
		}

		[Test]
		public void ActivateSetsViewTemplate() {
			var testable = new TestableExplorerStrategyDecade();
			var viewModel = new Mock<ILibraryExplorerViewModel>();

			viewModel.Setup( m => m.SetViewTemplate( It.IsAny<DataTemplate>())).Verifiable();

			var sut = testable.ClassUnderTest;
			sut.Initialize( viewModel.Object );

			sut.Activate();

			viewModel.Verify();
		}

		[Test]
		public void BuildTreeShouldReturnDecadeList() {
			var testable = new TestableExplorerStrategyDecade();
			var viewModel = new Mock<ILibraryExplorerViewModel>();
			var filter = new Mock<IDatabaseFilter>();
			var decadeList = new List<DbDecadeTag> { new DbDecadeTag( " one" ), new DbDecadeTag( "two" ) };

			testable.Mock<ITagManager>().Setup( m => m.DecadeTagList ).Returns( decadeList );

			var sut = testable.ClassUnderTest;
			sut.Initialize( viewModel.Object );

			var uiTagList = sut.BuildTree( filter.Object );

			Assert.IsNotNull( uiTagList );
			uiTagList.Should().HaveCount( 2 );
		}

		[Test]
		public void ExpandingDecadeNodesShouldPopulateArtists() {
			var testable = new TestableExplorerStrategyDecade();
			var viewModel = new Mock<ILibraryExplorerViewModel>();
			var filter = new Mock<IDatabaseFilter>();
			var decadeList = new List<DbDecadeTag> { new DbDecadeTag( " one" ), new DbDecadeTag( "two" ) };

			testable.Mock<ITagManager>().Setup( m => m.DecadeTagList ).Returns( decadeList );

			var artist1 = new DbArtist { Name = "one" };
			var artist2 = new DbArtist { Name = "two" };
			var artistList = new List<long> { artist1.DbId, artist2.DbId };
			testable.Mock<ITagManager>().Setup( m => m.ArtistListForDecade( It.IsAny<long>())).Returns( artistList );
			testable.Mock<IArtistProvider>().Setup( m => m.GetArtist( artist1.DbId )).Returns( artist1 );
			testable.Mock<IArtistProvider>().Setup( m => m.GetArtist( artist2.DbId )).Returns( artist2 );

			var sut = testable.ClassUnderTest;
			sut.Initialize( viewModel.Object );

			var uiTagList = sut.BuildTree( filter.Object );
			var treeNode = uiTagList.First() as UiDecadeTreeNode;

			Assert.IsNotNull( treeNode );
			treeNode.IsExpanded = true;

			Assert.IsNotNull( treeNode.Children );
			treeNode.Children.Should().HaveCount( 2 );
		}
	}
}
