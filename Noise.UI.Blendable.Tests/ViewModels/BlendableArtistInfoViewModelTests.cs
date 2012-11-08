using FluentAssertions;
using NUnit.Framework;
using Noise.UI.Blendable.ViewModels;
using Noise.UI.ViewModels;

namespace Noise.UI.Blendable.Tests.ViewModels {
	[TestFixture]
	public class BlendableArtistInfoViewModelTests {
		private ArtistInfoViewModel CreateSut() {
			var factory = new BlendableArtistInfoViewModel();

			return( factory.CreateViewModel() as ArtistInfoViewModel );
		}

		[Test]
		public void CanCreateViewModelFactory() {
			var vm = new BlendableArtistInfoViewModel();

			Assert.IsNotNull( vm );
		}

		[Test]
		public void ViewModelIsForCorrectType() {
			var sut = new BlendableArtistInfoViewModel();

			sut.ViewModelType.Should().Be( typeof( ArtistInfoViewModel ));
		}

		[Test]
		public void CanCreateViewModel() {
			var sut = CreateSut();

			Assert.IsNotNull( sut );
		}

		[Test]
		public void ViewModelHasBiography() {
			var sut = CreateSut();

			Assert.IsNotNullOrEmpty( sut.ArtistBiography );
		}

		[Test]
		public void ViewModelHasSimilarArtists() {
			var sut = CreateSut();

			sut.SimilarArtist.Should().HaveCount( length => length > 0 );
		}

		[Test]
		public void ViewModelShouldHaveBandMembers() {
			var sut = CreateSut();

			sut.BandMembers.Should().HaveCount( length => length > 0 );
		}

		[Test]
		public void ViewModelShouldHaveTopAlbums() {
			var sut = CreateSut();

			sut.TopAlbums.Should().HaveCount( length => length > 0 );
		}
	}
}
