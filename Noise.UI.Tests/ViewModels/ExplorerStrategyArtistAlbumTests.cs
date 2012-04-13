using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using FluentAssertions;
using FluentAssertions.EventMonitoring;
using Moq;
using NUnit.Framework;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Dto;
using Noise.UI.ViewModels;
using ReusableBits;
using ReusableBits.Interfaces;
using ReusableBits.TestSupport.Mocking;
using ReusableBits.TestSupport.Threading;
using ILog = Noise.Infrastructure.Interfaces.ILog;

namespace Noise.UI.Tests.ViewModels {
	internal class TestableStrategyArtistAlbum : Testable<ExplorerStrategyArtistAlbum> {
		private readonly DataTemplate	mDataTemplate;
		private readonly TaskScheduler	mTaskScheduler;
		
		public TestableStrategyArtistAlbum() {
			mDataTemplate = new DataTemplate();

			// Set tpl tasks to use the current thread only.
			mTaskScheduler = new CurrentThreadTaskScheduler();

			Mock<IResourceProvider>().Setup( m => m.RetrieveTemplate( It.IsAny<string>())).Returns( mDataTemplate );
			Mock<ITagManager>().Setup(  m => m.GetGenre( It.IsAny<long>())).Returns( new DbGenre( 1 ) { Name = "test genre" });
		}

		public override ExplorerStrategyArtistAlbum ClassUnderTest {
			get {
				var		retValue = base.ClassUnderTest;

				if( retValue != null ) {
					retValue.AlbumPopulateTask = new TaskHandler( mTaskScheduler, mTaskScheduler );
				}

				return( retValue );
			}
		}
	}

	[TestFixture]
	public class ExplorerStrategyArtistAlbumTests {
		[SetUp]
		public void Setup() {
			NoiseLogger.Current = new Mock<ILog>().Object;

			// Set the ui dispatcher to run on the current thread.
			Caliburn.Micro.Execute.ResetWithoutDispatcher();

			// Set up the AutoMapper configurations.
			MappingConfiguration.Configure();
		}

		[Test]
		public void CanCreateStrategy() {
			var testable = new TestableStrategyArtistAlbum();

			var sut = testable.ClassUnderTest;

			Assert.IsNotNullOrEmpty( sut.StrategyId );
			Assert.IsNotNullOrEmpty( sut.StrategyName );
		}

		[Test]
		public void CanInitializeStrategy() {
			var	testable = new TestableStrategyArtistAlbum();
			var viewModel = new Mock<ILibraryExplorerViewModel>();
			var sut = testable.ClassUnderTest;

			sut.Initialize( viewModel.Object );
		}

		[Test]
		public void ActivateSetsViewTemplate() {
			var testable = new TestableStrategyArtistAlbum();
			var viewModel = new Mock<ILibraryExplorerViewModel>();

			viewModel.Setup( m => m.SetViewTemplate( It.IsAny<DataTemplate>())).Verifiable();

			var sut = testable.ClassUnderTest;
			sut.Initialize( viewModel.Object );

			sut.Activate();

			viewModel.Verify();
		}

		[Test]
		public void BuildTreeShouldReturnArtistList() {
			var testable = new TestableStrategyArtistAlbum();
			var viewModel = new Mock<ILibraryExplorerViewModel>();
			var filter = new Mock<IDatabaseFilter>();
			var artistList = new List<DbArtist> { new DbArtist(), new DbArtist(), new DbArtist() };
			var provider = new Mock<IDataProviderList<DbArtist>>(); 

			provider.Setup( m => m.List ).Returns( artistList );
			testable.Mock<IArtistProvider>().Setup( m => m.GetArtistList( filter.Object )).Returns( provider.Object );

			var sut = testable.ClassUnderTest;

			sut.Initialize( viewModel.Object );

			var uiArtistList = sut.BuildTree( filter.Object );

			Assert.IsNotNull( uiArtistList );
			uiArtistList.Should().HaveCount( 3 );
		}

		[Test]
		public void ExpandingNodeShouldPopulateAlbums() {
			var testable = new TestableStrategyArtistAlbum();
			var viewModel = new Mock<ILibraryExplorerViewModel>();
			var filter = new Mock<IDatabaseFilter>();
			var artistList = new List<DbArtist> { new DbArtist() };
			var albumList = new List<DbAlbum> { new DbAlbum(), new DbAlbum() };
			var artistProvider = new Mock<IDataProviderList<DbArtist>>(); 
			var albumProvider = new Mock<IDataProviderList<DbAlbum>>(); 

			artistProvider.Setup( m => m.List ).Returns( artistList );
			albumProvider.Setup( m => m.List ).Returns( albumList );

			testable.Mock<IArtistProvider>().Setup( m => m.GetArtistList( filter.Object )).Returns( artistProvider.Object );
			testable.Mock<IAlbumProvider>().Setup( m => m.GetAlbumList( It.IsAny<long>())).Returns( albumProvider.Object );

			var sut = testable.ClassUnderTest;

			sut.Initialize( viewModel.Object );

			var uiArtistList = sut.BuildTree( filter.Object );
			var treeNode = uiArtistList.First() as UiArtistTreeNode;

			Assert.IsNotNull( treeNode );
			treeNode.IsExpanded = true;

			Assert.IsNotNull( treeNode.Children );
			treeNode.Children.Should().HaveCount( 2 );
		}

		[Test]
		public void SelectingArtistNodeShouldTriggerArtistEvent() {
			var testable = new TestableStrategyArtistAlbum();
			var viewModel = new Mock<ILibraryExplorerViewModel>();
			var filter = new Mock<IDatabaseFilter>();
			var artistList = new List<DbArtist> { new DbArtist() };
			var albumList = new List<DbAlbum> { new DbAlbum() };
			var artistProvider = new Mock<IDataProviderList<DbArtist>>(); 
			var albumProvider = new Mock<IDataProviderList<DbAlbum>>(); 

			artistProvider.Setup( m => m.List ).Returns( artistList );
			albumProvider.Setup( m => m.List ).Returns( albumList );

			testable.Mock<IArtistProvider>().Setup( m => m.GetArtistList( filter.Object )).Returns( artistProvider.Object );
			testable.Mock<IAlbumProvider>().Setup( m => m.GetAlbumList( It.IsAny<long>())).Returns( albumProvider.Object );
			testable.Mock<IEventAggregator>().Setup( m => m.Publish( It.IsAny<Events.ArtistFocusRequested>())).Verifiable();

			var sut = testable.ClassUnderTest;

			sut.Initialize( viewModel.Object );

			var uiArtistList = sut.BuildTree( filter.Object );
			var treeNode = uiArtistList.First() as UiArtistTreeNode;

			Assert.IsNotNull( treeNode );
			treeNode.IsSelected = true;

			testable.Mock<IEventAggregator>().Verify();
		}

		[Test]
		public void SelectingAlbumNodeShouldTriggerAlbumEvent() {
			var testable = new TestableStrategyArtistAlbum();
			var viewModel = new Mock<ILibraryExplorerViewModel>();
			var filter = new Mock<IDatabaseFilter>();
			var artistList = new List<DbArtist> { new DbArtist() };
			var albumList = new List<DbAlbum> { new DbAlbum() };
			var artistProvider = new Mock<IDataProviderList<DbArtist>>(); 
			var albumProvider = new Mock<IDataProviderList<DbAlbum>>(); 

			artistProvider.Setup( m => m.List ).Returns( artistList );
			albumProvider.Setup( m => m.List ).Returns( albumList );

			testable.Mock<IArtistProvider>().Setup( m => m.GetArtistList( filter.Object )).Returns( artistProvider.Object );
			testable.Mock<IAlbumProvider>().Setup( m => m.GetAlbumList( It.IsAny<long>())).Returns( albumProvider.Object );
			testable.Mock<IEventAggregator>().Setup( m => m.Publish( It.IsAny<Events.AlbumFocusRequested>())).Verifiable();

			var sut = testable.ClassUnderTest;

			sut.Initialize( viewModel.Object );

			var uiArtistList = sut.BuildTree( filter.Object );
			var treeNode = uiArtistList.First() as UiArtistTreeNode;

			Assert.IsNotNull( treeNode );
			treeNode.IsExpanded = true;

			var albumNode = treeNode.Children.First();
			Assert.IsNotNull( albumNode );

			albumNode.IsSelected = true;

			testable.Mock<IEventAggregator>().Verify();
		}

		[Test]
		public void ShouldFormatNamesBasedOnSortPrefixes() {
			var testable = new TestableStrategyArtistAlbum();
			var viewModel = new Mock<ILibraryExplorerViewModel>();
			var filter = new Mock<IDatabaseFilter>();
			var artistList = new List<DbArtist> { new DbArtist { Name = "The Rolling Stones" }};
			var artistProvider = new Mock<IDataProviderList<DbArtist>>(); 

			artistProvider.Setup( m => m.List ).Returns( artistList );
			testable.Mock<IArtistProvider>().Setup( m => m.GetArtistList( filter.Object )).Returns( artistProvider.Object );

			var sut = testable.ClassUnderTest;

			sut.Initialize( viewModel.Object );
			sut.UseSortPrefixes( true, new [] { "the" });

			var uiArtistList = sut.BuildTree( filter.Object );
			var uiArtist = uiArtistList.First() as UiArtistTreeNode;

			Assert.IsNotNull( uiArtist );

			uiArtist.Artist.SortName.Should().NotContainEquivalentOf( "the" );
		}

		[Test]
		public void ShouldOnlyFormatNamesStartingWithSortPrefix() {
			var testable = new TestableStrategyArtistAlbum();
			var viewModel = new Mock<ILibraryExplorerViewModel>();
			var filter = new Mock<IDatabaseFilter>();
			var artistList = new List<DbArtist> { new DbArtist { Name = "Joan Jett and The Blackhearts" }};
			var artistProvider = new Mock<IDataProviderList<DbArtist>>(); 

			artistProvider.Setup( m => m.List ).Returns( artistList );
			testable.Mock<IArtistProvider>().Setup( m => m.GetArtistList( filter.Object )).Returns( artistProvider.Object );

			var sut = testable.ClassUnderTest;

			sut.Initialize( viewModel.Object );
			sut.UseSortPrefixes( true, new [] { "the" });

			var uiArtistList = sut.BuildTree( filter.Object );
			var uiArtist = uiArtistList.First() as UiArtistTreeNode;

			Assert.IsNotNull( uiArtist );

			uiArtist.Artist.SortName.Should().Be( uiArtist.Artist.Name );
			uiArtist.Artist.DisplayName.Should().Be( uiArtist.Artist.Name );
		}

		[Test]
		public void ShouldBuildIndexOfUniqueStartingLetterArtists() {
			var testable = new TestableStrategyArtistAlbum();
			var viewModel = new Mock<ILibraryExplorerViewModel>();
			var filter = new Mock<IDatabaseFilter>();
			var artistList = new List<DbArtist> { new DbArtist { Name = "Joan Jett and The Blackhearts" },
												  new DbArtist { Name = "Jethro Tull" },
												  new DbArtist { Name = "The Rolling Stones" }};
			var artistProvider = new Mock<IDataProviderList<DbArtist>>(); 

			artistProvider.Setup( m => m.List ).Returns( artistList );
			testable.Mock<IArtistProvider>().Setup( m => m.GetArtistList( filter.Object )).Returns( artistProvider.Object );

			var sut = testable.ClassUnderTest;

			sut.Initialize( viewModel.Object );
			var uiArtistList = sut.BuildTree( filter.Object );
			var indexList = sut.BuildIndex( uiArtistList );

			Assert.IsNotNull( indexList );
			indexList.Should().HaveCount( 2 );
		}

		[Test]
		public void IndexShouldObserveSortPrefixes() {
			var testable = new TestableStrategyArtistAlbum();
			var viewModel = new Mock<ILibraryExplorerViewModel>();
			var filter = new Mock<IDatabaseFilter>();
			var artistList = new List<DbArtist> { new DbArtist { Name = "The Rolling Stones" }};
			var artistProvider = new Mock<IDataProviderList<DbArtist>>(); 

			artistProvider.Setup( m => m.List ).Returns( artistList );
			testable.Mock<IArtistProvider>().Setup( m => m.GetArtistList( filter.Object )).Returns( artistProvider.Object );

			var sut = testable.ClassUnderTest;

			sut.Initialize( viewModel.Object );
			sut.UseSortPrefixes( true, new [] { "the" });

			var uiArtistList = sut.BuildTree( filter.Object );
			var indexList = sut.BuildIndex( uiArtistList );
			var indexNode = indexList.First();

			Assert.IsNotNull( indexNode );

			indexNode.DisplayText.Should().Be( "R" );
		}

		[Test]
		public void CanSearchCaseSensitive() {
			var testable = new TestableStrategyArtistAlbum();
			var viewModel = new Mock<ILibraryExplorerViewModel>();
			var node1 = new UiArtistTreeNode( new UiArtist { Name = "Carpal Tunnel Syndrome" }, null, null, null, null, null );
			var node2 = new UiArtistTreeNode( new UiArtist { Name = "The Rolling Stones" }, null, null, null, null, null );
			var artistList = new Collection<UiTreeNode> { node1, node2 };

			viewModel.Setup( m => m.TreeData ).Returns( artistList );

			var sut = testable.ClassUnderTest;
			sut.Initialize( viewModel.Object );

			var found = sut.Search( "Rolling", new [] { ExplorerStrategyArtistAlbum.cSearchOptionDefault } );

			Assert.IsTrue( found );
			Assert.IsTrue( node2.IsSelected );
		}

		[Test]
		public void CanSearchCaseInsensitive() {
			var testable = new TestableStrategyArtistAlbum();
			var viewModel = new Mock<ILibraryExplorerViewModel>();
			var node1 = new UiArtistTreeNode( new UiArtist { Name = "Carpal Tunnel Syndrome" }, null, null, null, null, null );
			var node2 = new UiArtistTreeNode( new UiArtist { Name = "The Rolling Stones" }, null, null, null, null, null );
			var artistList = new Collection<UiTreeNode> { node1, node2 };

			viewModel.Setup( m => m.TreeData ).Returns( artistList );

			var sut = testable.ClassUnderTest;
			sut.Initialize( viewModel.Object );

			var found = sut.Search( "roll", new [] { ExplorerStrategyArtistAlbum.cSearchIgnoreCase } );

			Assert.IsTrue( found );
			Assert.IsTrue( node2.IsSelected );
		}

		[Test]
		public void SearchingWithSameOptionsContinuesSearch() {
			var testable = new TestableStrategyArtistAlbum();
			var viewModel = new Mock<ILibraryExplorerViewModel>();
			var node1 = new UiArtistTreeNode( new UiArtist { Name = "Stone Roses" }, null, null, null, null, null );
			var node2 = new UiArtistTreeNode( new UiArtist { Name = "The Rolling Stones" }, null, null, null, null, null );
			var artistList = new Collection<UiTreeNode> { node1, node2 };

			viewModel.Setup( m => m.TreeData ).Returns( artistList );

			var sut = testable.ClassUnderTest;
			sut.Initialize( viewModel.Object );

			sut.Search( "one", new [] { ExplorerStrategyArtistAlbum.cSearchOptionDefault } );
			var found = sut.Search( "one", new [] { ExplorerStrategyArtistAlbum.cSearchOptionDefault } );

			Assert.IsTrue( found );
			Assert.IsTrue( node2.IsSelected );
		}

		[Test]
		public void ChangingSearchOptionsResetsSearch() {
			var testable = new TestableStrategyArtistAlbum();
			var viewModel = new Mock<ILibraryExplorerViewModel>();
			var node1 = new UiArtistTreeNode( new UiArtist { Name = "Carpal Tunnel Syndrome" }, null, null, null, null, null );
			var node2 = new UiArtistTreeNode( new UiArtist { Name = "The Rolling Stones" }, null, null, null, null, null );
			var artistList = new Collection<UiTreeNode> { node1, node2 };

			viewModel.Setup( m => m.TreeData ).Returns( artistList );

			var sut = testable.ClassUnderTest;
			sut.Initialize( viewModel.Object );

			sut.Search( "roll", new [] { ExplorerStrategyArtistAlbum.cSearchIgnoreCase } );
			var found = sut.Search( "ones", new [] { ExplorerStrategyArtistAlbum.cSearchOptionDefault } );

			Assert.IsTrue( found );
			Assert.IsTrue( node2.IsSelected );
		}

		[Test]
		public void CanClearSearch() {
			var testable = new TestableStrategyArtistAlbum();
			var viewModel = new Mock<ILibraryExplorerViewModel>();
			var node1 = new UiArtistTreeNode( new UiArtist { Name = "Stone Roses" }, null, null, null, null, null );
			var node2 = new UiArtistTreeNode( new UiArtist { Name = "The Rolling Stones" }, null, null, null, null, null );
			var artistList = new Collection<UiTreeNode> { node1, node2 };

			viewModel.Setup( m => m.TreeData ).Returns( artistList );

			var sut = testable.ClassUnderTest;
			sut.Initialize( viewModel.Object );

			sut.Search( "one", new [] { ExplorerStrategyArtistAlbum.cSearchOptionDefault } );
			sut.ClearCurrentSearch();
			var found = sut.Search( "one", new [] { ExplorerStrategyArtistAlbum.cSearchOptionDefault } );

			Assert.IsTrue( found );
			Assert.IsTrue( node1.IsSelected );
		}

		[Test]
		public void DatabaseChangedAddsArtistNode() {
			var testable = new TestableStrategyArtistAlbum();
			var viewModel = new Mock<ILibraryExplorerViewModel>();
			var artist = new DbArtist();

			var treeData = new Collection<UiTreeNode>();
			viewModel.Setup( m => m.TreeData ).Returns( treeData );
			testable.Mock<IArtistProvider>().Setup( m => m.GetArtist( It.IsAny<long>())).Returns( artist );
			
			var sut = testable.ClassUnderTest;
			sut.Initialize( viewModel.Object );

			sut.Handle( new Events.ArtistAdded( artist.DbId ));

			treeData.Should().HaveCount( 1 );			
		}

		[Test]
		public void DatabaseChangedUpdatesArtistNode() {
			var testable = new TestableStrategyArtistAlbum();
			var viewModel = new Mock<ILibraryExplorerViewModel>();
			var artist = new DbArtist { Name = "artist name" };
			var provider = new Mock<IDataProviderList<DbArtist>>(); 

			provider.Setup( m => m.List ).Returns( new List<DbArtist> { artist });
			testable.Mock<IArtistProvider>().Setup( m => m.GetArtist( It.IsAny<long>())).Returns( artist );
			testable.Mock<IArtistProvider>().Setup( m => m.GetArtistList( It.IsAny<IDatabaseFilter>())).Returns( provider.Object );

			var treeData = new BindableCollection<UiTreeNode>();
			viewModel.Setup( m => m.TreeData ).Returns( treeData );
			
			var sut = testable.ClassUnderTest;
			sut.Initialize( viewModel.Object );
			treeData.AddRange( sut.BuildTree( null ));

			var treeNode = treeData.First() as UiArtistTreeNode;
			Assert.IsNotNull( treeNode );
			Assert.IsNotNull( treeNode.Artist );
			treeNode.Artist.MonitorEvents();

			artist.Name = "updated name";
			sut.Handle( new Events.ArtistUserUpdate( artist.DbId ));

			treeNode.Artist.ShouldRaisePropertyChangeFor( a => a.Name );
		}

		[Test]
		public void DatabaseChangedRemovedArtistNode() {
			var testable = new TestableStrategyArtistAlbum();
			var viewModel = new Mock<ILibraryExplorerViewModel>();
			var artist = new DbArtist();
			var artistProvider = new Mock<IDataProviderList<DbArtist>>(); 

			artistProvider.Setup( m => m.List ).Returns( new List<DbArtist> { artist });
			testable.Mock<IArtistProvider>().Setup( m => m.GetArtistList( It.IsAny<IDatabaseFilter>())).Returns( artistProvider.Object );

			var treeData = new BindableCollection<UiTreeNode>();
			viewModel.Setup( m => m.TreeData ).Returns( treeData );
			
			var sut = testable.ClassUnderTest;
			sut.Initialize( viewModel.Object );
			treeData.AddRange( sut.BuildTree( null ));
			treeData.Should().HaveCount( 1 );

			sut.Handle( new Events.ArtistRemoved( artist.DbId ));

			treeData.Should().HaveCount( 0 );
		}
	}
}
