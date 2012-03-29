using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Dto;
using Noise.UI.ViewModels;
using ReusableBits;
using ReusableBits.TestSupport.Mocking;
using ReusableBits.TestSupport.Threading;

namespace Noise.UI.Tests.ViewModels {
	internal class TestableLibraryExplorerViewModel : Testable<LibraryExplorerViewModel> {
		private readonly TaskScheduler		mTaskScheduler;
		
		public TestableLibraryExplorerViewModel() {
			// Set tpl tasks to use the current thread only.
			mTaskScheduler = new CurrentThreadTaskScheduler();
		}

		public override LibraryExplorerViewModel ClassUnderTest {
			get {
				var		retValue = base.ClassUnderTest;

				if( retValue != null ) {
					retValue.TreeBuilderTask = new TaskHandler( mTaskScheduler, mTaskScheduler );
				}

				return( retValue );
			}
		}
	}

	[TestFixture]
	public class LibraryExplorerViewModelTests {
		[SetUp]
		public void Setup() {
			NoiseLogger.Current = new Mock<ILog>().Object;
		}

		[Test]
		public void CanCreateViewModel() {
			var testable = new TestableLibraryExplorerViewModel();
			var sut = testable.ClassUnderTest;

			Assert.IsNotNull( sut.TreeData );
		}

		[Test]
		public void DefaultViewStrategyShouldBeActivatedOnTreeDataAccess() {
			var strategy = new Mock<IExplorerViewStrategy>();
			var testable = new TestableLibraryExplorerViewModel();
			testable.Inject( strategy.Object );

			strategy.Setup( m => m.IsDefaultStrategy ).Returns( true );
			strategy.Setup( m => m.Activate()).Verifiable();

			var sut = testable.ClassUnderTest;
			var treeData = sut.TreeData;
			Assert.IsNotNull( treeData );

			strategy.Verify();
		}

		[Test]
		public void SelectedStrategyShouldActivateStrategy() {
			var strategy = new Mock<IExplorerViewStrategy>();
			var testable = new TestableLibraryExplorerViewModel();
			testable.Inject( strategy.Object );

			strategy.Setup( m => m.Activate()).Verifiable();

			var sut = testable.ClassUnderTest;
			sut.SelectedStrategy = strategy.Object;

			strategy.Verify();
		}

		[Test]
		public void SelectedStrategyShouldRequestTreeBuild() {
			var strategy = new Mock<IExplorerViewStrategy>();
			var testable = new TestableLibraryExplorerViewModel();
			testable.Inject( strategy.Object );

			strategy.Setup( m => m.BuildTree( It.IsAny<IDatabaseFilter>())).Returns( new List<UiTreeNode>()).Verifiable();

			var sut = testable.ClassUnderTest;
			sut.SelectedStrategy = strategy.Object;

			strategy.Verify();
		}

		[Test]
		public void NewStrategyShouldDeactivateActiveStrategy() {
			var strategy1 = new Mock<IExplorerViewStrategy>();
			var strategy2 = new Mock<IExplorerViewStrategy>();
			var testable = new TestableLibraryExplorerViewModel();
			testable.Inject( strategy1.Object );
			testable.Inject( strategy2.Object );

			strategy1.Setup( m => m.IsDefaultStrategy ).Returns( true );
			strategy1.Setup( m => m.Deactivate()).Verifiable();
			strategy2.Setup( m => m.Activate()).Verifiable();

			var sut = testable.ClassUnderTest;
			var treeData = sut.TreeData;
			Assert.IsNotNull( treeData );
			sut.SelectedStrategy = strategy2.Object;

			strategy1.Verify();
			strategy2.Verify();
		}

		[Test]
		public void BuildingTreeShouldPopulateTreeData() {
			var strategy = new Mock<IExplorerViewStrategy>();
			var testable = new TestableLibraryExplorerViewModel();
			testable.Inject( strategy.Object );

			var treeNode = new UiTreeNode();
			strategy.Setup( m => m.BuildTree( It.IsAny<IDatabaseFilter>())).Returns( new List<UiTreeNode> { treeNode });

			var sut = testable.ClassUnderTest;
			sut.SelectedStrategy = strategy.Object;

			sut.TreeData.Should().HaveCount( 1 );
		}
	}
}
