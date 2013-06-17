using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using FluentAssertions;
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
					retValue.ChildPopulateTask = new TaskHandler( mTaskScheduler, mTaskScheduler );
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
			Execute.ResetWithoutDispatcher();

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
			var decadeList = new List<DbDecadeTag> { new DbDecadeTag( " one" ), new DbDecadeTag( "two" ) };

			testable.Mock<ITagManager>().Setup( m => m.DecadeTagList ).Returns( decadeList );

			var sut = testable.ClassUnderTest;
			sut.Initialize( viewModel.Object );

			var uiTagList = sut.BuildTree();

			Assert.IsNotNull( uiTagList );
			uiTagList.Should().HaveCount( 2 );
		}

		[Test]
		public void ExpandingDecadeNodesShouldPopulateArtists() {
			var testable = new TestableExplorerStrategyDecade();
			var viewModel = new Mock<ILibraryExplorerViewModel>();
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

			var uiTagList = sut.BuildTree();
			var treeNode = uiTagList.First() as UiDecadeTreeNode;

			Assert.IsNotNull( treeNode );
			treeNode.IsExpanded = true;

			Assert.IsNotNull( treeNode.Children );
			treeNode.Children.Should().HaveCount( 2 );
		}

		[Test]
		public void ExpandingArtistNodeShouldPopulateAlbums() {
			var testable = new TestableExplorerStrategyDecade();
			var viewModel = new Mock<ILibraryExplorerViewModel>();
			var decadeList = new List<DbDecadeTag> { new DbDecadeTag( " one" ), new DbDecadeTag( "two" ) };

			testable.Mock<ITagManager>().Setup( m => m.DecadeTagList ).Returns( decadeList );

			var artist1 = new DbArtist { Name = "one" };
			var artist2 = new DbArtist { Name = "two" };
			var artistList = new List<long> { artist1.DbId, artist2.DbId };
			testable.Mock<ITagManager>().Setup( m => m.ArtistListForDecade( It.IsAny<long>())).Returns( artistList );
			testable.Mock<IArtistProvider>().Setup( m => m.GetArtist( artist1.DbId )).Returns( artist1 );
			testable.Mock<IArtistProvider>().Setup( m => m.GetArtist( artist2.DbId )).Returns( artist2 );

			var album1 = new DbAlbum { Name = "album1" };
			var album2 = new DbAlbum { Name = "album2" };
			var albumList = new List<long> { album1.DbId, album2.DbId };
			testable.Mock<ITagManager>().Setup( m => m.AlbumListForDecade( It.IsAny<long>(), It.IsAny<long>())).Returns( albumList );
			testable.Mock<IAlbumProvider>().Setup( m => m.GetAlbum( album1.DbId )).Returns( album1 );
			testable.Mock<IAlbumProvider>().Setup( m => m.GetAlbum( album2.DbId )).Returns( album2 );

			var sut = testable.ClassUnderTest;
			sut.Initialize( viewModel.Object );

			var uiTagList = sut.BuildTree();
			var treeNode = uiTagList.First() as UiDecadeTreeNode;

			Assert.IsNotNull( treeNode );
			treeNode.IsExpanded = true;

			var artistNode = treeNode.Children.First();
			Assert.IsNotNull( artistNode );
			artistNode.IsExpanded = true;

			artistNode.Children.Should().HaveCount( 2 );
		}

		[Test]
		public void SelectingArtistNodeShouldTriggerArtistEvent() {
			var testable = new TestableExplorerStrategyDecade();
			var viewModel = new Mock<ILibraryExplorerViewModel>();
			var decadeList = new List<DbDecadeTag> { new DbDecadeTag( " one" ), new DbDecadeTag( "two" ) };

			testable.Mock<ITagManager>().Setup( m => m.DecadeTagList ).Returns( decadeList );

			var artist1 = new DbArtist { Name = "one" };
			var artistList = new List<long> { artist1.DbId };
			testable.Mock<ITagManager>().Setup( m => m.ArtistListForDecade( It.IsAny<long>())).Returns( artistList );
			testable.Mock<IArtistProvider>().Setup( m => m.GetArtist( artist1.DbId )).Returns( artist1 );

			testable.Mock<IEventAggregator>().Setup( m => m.Publish( It.IsAny<Events.ArtistFocusRequested>())).Verifiable();

			var sut = testable.ClassUnderTest;
			sut.Initialize( viewModel.Object );

			var uiTagList = sut.BuildTree();
			var treeNode = uiTagList.First() as UiDecadeTreeNode;

			Assert.IsNotNull( treeNode );
			treeNode.IsExpanded = true;

			var artistNode = treeNode.Children.First();
			Assert.IsNotNull( artistNode );

			artistNode.IsSelected = true;

			testable.Mock<IEventAggregator>().Verify();
		}

		[Test]
		public void SelectingAlbumNodeShouldTriggerAlbumEvent() {
			var testable = new TestableExplorerStrategyDecade();
			var viewModel = new Mock<ILibraryExplorerViewModel>();
			var decadeList = new List<DbDecadeTag> { new DbDecadeTag( " one" ), new DbDecadeTag( "two" ) };

			testable.Mock<ITagManager>().Setup( m => m.DecadeTagList ).Returns( decadeList );

			var artist1 = new DbArtist { Name = "one" };
			var artistList = new List<long> { artist1.DbId };
			testable.Mock<ITagManager>().Setup( m => m.ArtistListForDecade( It.IsAny<long>())).Returns( artistList );
			testable.Mock<IArtistProvider>().Setup( m => m.GetArtist( artist1.DbId )).Returns( artist1 );

			var album1 = new DbAlbum { Name = "album1" };
			var albumList = new List<long> { album1.DbId };
			testable.Mock<ITagManager>().Setup( m => m.AlbumListForDecade( It.IsAny<long>(), It.IsAny<long>())).Returns( albumList );
			testable.Mock<IAlbumProvider>().Setup( m => m.GetAlbum( album1.DbId )).Returns( album1 );

			testable.Mock<IEventAggregator>().Setup( m => m.Publish( It.IsAny<Events.AlbumFocusRequested>())).Verifiable();

			var sut = testable.ClassUnderTest;
			sut.Initialize( viewModel.Object );

			var uiTagList = sut.BuildTree();
			var treeNode = uiTagList.First() as UiDecadeTreeNode;

			Assert.IsNotNull( treeNode );
			treeNode.IsExpanded = true;

			var artistNode = treeNode.Children.First();

			Assert.IsNotNull( artistNode );
			artistNode.IsExpanded = true;

			var albumNode = artistNode.Children.First();
			Assert.IsNotNull( albumNode );

			albumNode.IsSelected = true;

			testable.Mock<IEventAggregator>().Verify();
		}

		[Test]
		public void ShouldFormatNamesBasedOnSortPrefixes() {
			var testable = new TestableExplorerStrategyDecade();
			var viewModel = new Mock<ILibraryExplorerViewModel>();
			var decadeList = new List<DbDecadeTag> { new DbDecadeTag( " one" ), new DbDecadeTag( "two" ) };

			testable.Mock<ITagManager>().Setup( m => m.DecadeTagList ).Returns( decadeList );

			var artist = new DbArtist { Name = "The Rolling Stones" };
			var artistList = new List<long> { artist.DbId };
			testable.Mock<ITagManager>().Setup( m => m.ArtistListForDecade( It.IsAny<long>())).Returns( artistList );
			testable.Mock<IArtistProvider>().Setup( m => m.GetArtist( artist.DbId )).Returns( artist );

			var sut = testable.ClassUnderTest;

			sut.Initialize( viewModel.Object );
			sut.UseSortPrefixes( true, new [] { "the" });

			var uiTagList = sut.BuildTree();
			var treeNode = uiTagList.First() as UiDecadeTreeNode;

			Assert.IsNotNull( treeNode );
			treeNode.IsExpanded = true;

			var artistNode = treeNode.Children.First();

			Assert.IsNotNull( artistNode );

			artistNode.Artist.SortName.Should().NotContainEquivalentOf( "the" );
		}

		[Test]
		public void ShouldOnlyFormatNamesStartingWithSortPrefix() {
			var testable = new TestableExplorerStrategyDecade();
			var viewModel = new Mock<ILibraryExplorerViewModel>();
			var decadeList = new List<DbDecadeTag> { new DbDecadeTag( " one" ), new DbDecadeTag( "two" ) };

			testable.Mock<ITagManager>().Setup( m => m.DecadeTagList ).Returns( decadeList );

			var artist = new DbArtist { Name = "Joan Jett and The Blackhearts" };
			var artistList = new List<long> { artist.DbId };
			testable.Mock<ITagManager>().Setup( m => m.ArtistListForDecade( It.IsAny<long>())).Returns( artistList );
			testable.Mock<IArtistProvider>().Setup( m => m.GetArtist( artist.DbId )).Returns( artist );

			var sut = testable.ClassUnderTest;

			sut.Initialize( viewModel.Object );
			sut.UseSortPrefixes( true, new [] { "the" });

			var uiTagList = sut.BuildTree();
			var treeNode = uiTagList.First() as UiDecadeTreeNode;

			Assert.IsNotNull( treeNode );
			treeNode.IsExpanded = true;

			var artistNode = treeNode.Children.First();

			Assert.IsNotNull( artistNode );

			artistNode.Artist.SortName.Should().Be( artistNode.Artist.Name );
			artistNode.Artist.DisplayName.Should().Be( artistNode.Artist.Name );
		}

		[Test]
		public void CanSearchCaseSensitive() {
			var testable = new TestableExplorerStrategyDecade();
			var viewModel = new Mock<ILibraryExplorerViewModel>();
			var node1 = new UiArtistTreeNode( new UiArtist { Name = "Carpal Tunnel Syndrome" }, null, null, null, null, null );
			var node2 = new UiArtistTreeNode( new UiArtist { Name = "The Rolling Stones" }, null, null, null, null, null );
			var artistNodes = new Collection<UiArtistTreeNode> { node1, node2 };
			var decadeNode = new UiDecadeTreeNode( new DbDecadeTag( "decade" ), null, null, null, null, null, null );
			decadeNode.SetChildren( artistNodes );

			viewModel.Setup( m => m.TreeData ).Returns( new Collection<UiTreeNode> { decadeNode });

			var sut = testable.ClassUnderTest;
			sut.Initialize( viewModel.Object );

			var found = sut.Search( "Rolling", new [] { ExplorerStrategyDecade.cSearchOptionDefault } );

			Assert.IsTrue( found );
			Assert.IsTrue( node2.IsSelected );
		}

		[Test]
		public void CanSearchCaseInsensitive() {
			var testable = new TestableExplorerStrategyDecade();
			var viewModel = new Mock<ILibraryExplorerViewModel>();
			var node1 = new UiArtistTreeNode( new UiArtist { Name = "Carpal Tunnel Syndrome" }, null, null, null, null, null );
			var node2 = new UiArtistTreeNode( new UiArtist { Name = "The Rolling Stones" }, null, null, null, null, null );
			var artistNodes = new Collection<UiArtistTreeNode> { node1, node2 };
			var decadeNode = new UiDecadeTreeNode( new DbDecadeTag( "decade" ), null, null, null, null, null, null );
			decadeNode.SetChildren( artistNodes );

			viewModel.Setup( m => m.TreeData ).Returns( new Collection<UiTreeNode> { decadeNode });

			var sut = testable.ClassUnderTest;
			sut.Initialize( viewModel.Object );

			var found = sut.Search( "roll", new [] { ExplorerStrategyDecade.cSearchIgnoreCase } );

			Assert.IsTrue( found );
			Assert.IsTrue( node2.IsSelected );
		}

		[Test]
		public void SearchingWithSameOptionsContinuesSearch() {
			var testable = new TestableExplorerStrategyDecade();
			var viewModel = new Mock<ILibraryExplorerViewModel>();
			var node1 = new UiArtistTreeNode( new UiArtist { Name = "Stone Roses" }, null, null, null, null, null );
			var node2 = new UiArtistTreeNode( new UiArtist { Name = "The Rolling Stones" }, null, null, null, null, null );
			var artistNodes = new Collection<UiArtistTreeNode> { node1, node2 };
			var decadeNode = new UiDecadeTreeNode( new DbDecadeTag( "decade" ), null, null, null, null, null, null );
			decadeNode.SetChildren( artistNodes );

			viewModel.Setup( m => m.TreeData ).Returns( new Collection<UiTreeNode> { decadeNode });

			var sut = testable.ClassUnderTest;
			sut.Initialize( viewModel.Object );

			sut.Search( "one", new [] { ExplorerStrategyArtistAlbum.cSearchOptionDefault } );
			var found = sut.Search( "one", new [] { ExplorerStrategyArtistAlbum.cSearchOptionDefault } );

			Assert.IsTrue( found );
			Assert.IsTrue( node2.IsSelected );
		}

		[Test]
		public void ChangingSearchOptionsResetsSearch() {
			var testable = new TestableExplorerStrategyDecade();
			var viewModel = new Mock<ILibraryExplorerViewModel>();
			var node1 = new UiArtistTreeNode( new UiArtist { Name = "Carpal Tunnel Syndrome" }, null, null, null, null, null );
			var node2 = new UiArtistTreeNode( new UiArtist { Name = "The Rolling Stones" }, null, null, null, null, null );
			var artistNodes = new Collection<UiArtistTreeNode> { node1, node2 };
			var decadeNode = new UiDecadeTreeNode( new DbDecadeTag( "decade" ), null, null, null, null, null, null );
			decadeNode.SetChildren( artistNodes );

			viewModel.Setup( m => m.TreeData ).Returns( new Collection<UiTreeNode> { decadeNode });

			var sut = testable.ClassUnderTest;
			sut.Initialize( viewModel.Object );

			sut.Search( "roll", new [] { ExplorerStrategyArtistAlbum.cSearchIgnoreCase } );
			var found = sut.Search( "ones", new [] { ExplorerStrategyArtistAlbum.cSearchOptionDefault } );

			Assert.IsTrue( found );
			Assert.IsTrue( node2.IsSelected );
		}

		[Test]
		public void CanClearSearch() {
			var testable = new TestableExplorerStrategyDecade();
			var viewModel = new Mock<ILibraryExplorerViewModel>();
			var node1 = new UiArtistTreeNode( new UiArtist { Name = "Stone Roses" }, null, null, null, null, null );
			var node2 = new UiArtistTreeNode( new UiArtist { Name = "The Rolling Stones" }, null, null, null, null, null );
			var artistNodes = new Collection<UiArtistTreeNode> { node1, node2 };
			var decadeNode = new UiDecadeTreeNode( new DbDecadeTag( "decade" ), null, null, null, null, null, null );
			decadeNode.SetChildren( artistNodes );

			viewModel.Setup( m => m.TreeData ).Returns( new Collection<UiTreeNode> { decadeNode });

			var sut = testable.ClassUnderTest;
			sut.Initialize( viewModel.Object );

			sut.Search( "one", new [] { ExplorerStrategyArtistAlbum.cSearchOptionDefault } );
			sut.ClearCurrentSearch();
			var found = sut.Search( "one", new [] { ExplorerStrategyArtistAlbum.cSearchOptionDefault } );

			Assert.IsTrue( found );
			Assert.IsTrue( node1.IsSelected );
		}

		[Test]
		public void DatabaseChangeUpdatesArtistNode() {
			var testable = new TestableExplorerStrategyDecade();
			var viewModel = new Mock<ILibraryExplorerViewModel>();
			var artist = new DbArtist { Name = "artist name" };
			var node = new UiArtistTreeNode( new UiArtist { DbId = artist.DbId, Name = artist.Name }, null, null, null, null, null );
			var artistNodes = new Collection<UiArtistTreeNode> { node };
			var decadeNode = new UiDecadeTreeNode( new DbDecadeTag( "decade" ), null, null, null, null, null, null );
			decadeNode.SetChildren( artistNodes );

			viewModel.Setup( m => m.TreeData ).Returns( new Collection<UiTreeNode> { decadeNode });
			testable.Mock<IArtistProvider>().Setup( m => m.GetArtist( It.IsAny<long>())).Returns( artist );

			var sut = testable.ClassUnderTest;
			sut.Initialize( viewModel.Object );

			artist.Name = "updated name";
			sut.Handle( new Events.ArtistUserUpdate( artist.DbId ));

			node.Artist.Name.Should().Be( artist.Name );
		}

		[Test]
		public void DatabaseChangeUpdatesAlbumNode() {
			var testable = new TestableExplorerStrategyDecade();
			var viewModel = new Mock<ILibraryExplorerViewModel>();
			var artist = new DbArtist();
			var album = new DbAlbum { Name = "original name", Artist = artist.DbId };
			var albumNode = new UiAlbumTreeNode( new UiAlbum { DbId = album.DbId, Name = album.Name }, null, null );
			var artistNode = new UiArtistTreeNode( new UiArtist { DbId = artist.DbId, Name = artist.Name }, null, null, null, null, null );
			artistNode.SetChildren( new List<UiAlbumTreeNode> { albumNode });
			var artistNodes = new Collection<UiArtistTreeNode> { artistNode };
			var decadeNode = new UiDecadeTreeNode( new DbDecadeTag( "decade" ), null, null, null, null, null, null );
			decadeNode.SetChildren( artistNodes );

			viewModel.Setup( m => m.TreeData ).Returns( new Collection<UiTreeNode> { decadeNode });
			testable.Mock<IAlbumProvider>().Setup( m => m.GetAlbum( It.IsAny<long>())).Returns( album );

			var sut = testable.ClassUnderTest;
			sut.Initialize( viewModel.Object );

			album.Name = "changed name";
			sut.Handle( new Events.AlbumUserUpdate( album.DbId ));

			albumNode.Album.Name.Should().Be( album.Name );
		}
	}
}
