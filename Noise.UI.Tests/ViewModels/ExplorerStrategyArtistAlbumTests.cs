using System.Collections.Generic;
using System.Linq;
using System.Windows;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Dto;
using Noise.UI.ViewModels;
using ReusableBits.TestSupport.Mocking;

namespace Noise.UI.Tests.ViewModels {
	internal class TestableStrategyArtistAlbum : Testable<ExplorerStrategyArtistAlbum> {
		private readonly DataTemplate	mDataTemplate;

		public TestableStrategyArtistAlbum() {
			mDataTemplate = new DataTemplate();

			Mock<IResourceProvider>().Setup( m => m.RetrieveTemplate( It.IsAny<string>())).Returns( mDataTemplate );
			Mock<ITagManager>().Setup(  m => m.GetGenre( It.IsAny<long>())).Returns( new DbGenre( 1 ) { Name = "test genre" });
		}
	}

	[TestFixture]
	public class ExplorerStrategyArtistAlbumTests {
		[SetUp]
		public void Setup() {
			NoiseLogger.Current = new Mock<ILog>().Object;

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

			testable.Mock<IArtistProvider>().Setup( m => m.GetArtistList( filter.Object )).Returns( new DataProviderList<DbArtist>( null, artistList ));

			var sut = testable.ClassUnderTest;

			sut.Initialize( viewModel.Object );

			var uiArtistList = sut.BuildTree( filter.Object );

			Assert.IsNotNull( uiArtistList );
			uiArtistList.Should().HaveCount( 3 );
		}

		[Test]
		public void ShouldFormatNamesBasedOnSortPrefixes() {
			var testable = new TestableStrategyArtistAlbum();
			var viewModel = new Mock<ILibraryExplorerViewModel>();
			var filter = new Mock<IDatabaseFilter>();
			var artistList = new List<DbArtist> { new DbArtist() { Name = "The Rolling Stones" }};

			testable.Mock<IArtistProvider>().Setup( m => m.GetArtistList( filter.Object )).Returns( new DataProviderList<DbArtist>( null, artistList ));

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
			var artistList = new List<DbArtist> { new DbArtist() { Name = "Joan Jett and The Blackhearts" }};

			testable.Mock<IArtistProvider>().Setup( m => m.GetArtistList( filter.Object )).Returns( new DataProviderList<DbArtist>( null, artistList ));

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
			var artistList = new List<DbArtist> { new DbArtist() { Name = "Joan Jett and The Blackhearts" },
												  new DbArtist() { Name = "Jethro Tull" },
												  new DbArtist() { Name = "The Rolling Stones" }};

			testable.Mock<IArtistProvider>().Setup( m => m.GetArtistList( filter.Object )).Returns( new DataProviderList<DbArtist>( null, artistList ));

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
			var artistList = new List<DbArtist> { new DbArtist() { Name = "The Rolling Stones" }};

			testable.Mock<IArtistProvider>().Setup( m => m.GetArtistList( filter.Object )).Returns( new DataProviderList<DbArtist>( null, artistList ));

			var sut = testable.ClassUnderTest;

			sut.Initialize( viewModel.Object );
			sut.UseSortPrefixes( true, new [] { "the" });

			var uiArtistList = sut.BuildTree( filter.Object );
			var indexList = sut.BuildIndex( uiArtistList );
			var indexNode = indexList.First();

			Assert.IsNotNull( indexNode );

			indexNode.DisplayText.Should().Be( "R" );
		}
	}
}
