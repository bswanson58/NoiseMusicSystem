using FluentAssertions;
using NUnit.Framework;
using Noise.UI.Blendable.ViewModels;
using Noise.UI.ViewModels;

namespace Noise.UI.Blendable.Tests.ViewModels {
	[TestFixture]
	public class BlendableArtistViewModelTests {

		private ArtistViewModel CreateSut() {
			var factory = new BlendableArtistViewModel();

			return( factory.CreateViewModel() as ArtistViewModel );
		}

		[Test]
		public void CanCreateViewModelFactory() {
			var vm = new BlendableArtistViewModel();

			Assert.IsNotNull( vm );
		}

		[Test]
		public void ViewModelIsForCorrectType() {
			var sut = new BlendableArtistViewModel();

			sut.ViewModelType.Should().Be( typeof( ArtistViewModel ));
		}

		[Test]
		public void CanCreateViewModel() {
			var sut = CreateSut();

			Assert.IsNotNull( sut );
		}

		[Test]
		public void ViewModelCreatesValidArtist() {
			var sut = CreateSut();

			Assert.IsTrue( sut.ArtistValid );
			Assert.IsNotNull( sut.Artist );
		}

		[Test]
		public void ViewModelHasArtistImage() {
			var sut = CreateSut();

			Assert.IsNotNull( sut.ArtistImage );
			sut.ArtistImage.Should().HaveCount( length => length > 10 );
		}
	}
}
