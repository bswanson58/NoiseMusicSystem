using System;
using FluentAssertions;
using NUnit.Framework;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.BaseDatabase.Tests.DataProviders {
	public abstract class BaseDiscographyProviderTests : BaseProviderTest<IDiscographyProvider> {
		[Test]
		public void CanAddDiscography() {
			var disco = new DbDiscographyRelease( 1, "title", "format", "label", 1999, DiscographyReleaseType.Release );

			var sut = CreateSut();

			sut.AddDiscography( disco );
		}

		[Test]
		[ExpectedException( typeof( ArgumentNullException ))]
		public void CannotAddNullDiscography() {
			var sut = CreateSut();

			sut.AddDiscography( null );
		}

		[Test]
		public void CanRetrieveDiscography() {
			var artist = new DbArtist();
			var disco1 = new DbDiscographyRelease( 1, "title", "format", "label", 1999, DiscographyReleaseType.Release ) { Artist = artist.DbId };
			var disco2 = new DbDiscographyRelease( 1, "title", "format", "label", 1999, DiscographyReleaseType.Release ) { Artist = artist.DbId + 1 };

			var sut = CreateSut();

			sut.AddDiscography( disco1 );
			sut.AddDiscography( disco2 );

			using( var discoList = sut.GetDiscography( artist.DbId )) {
				discoList.List.Should().HaveCount( 1 );
			}
		}

		[Test]
		public void CanRemoveDiscography() {
			var artist = new DbArtist();
			var disco1 = new DbDiscographyRelease( 1, "title", "format", "label", 1999, DiscographyReleaseType.Release ) { Artist = artist.DbId };
			var disco2 = new DbDiscographyRelease( 2, "title", "format", "label", 1999, DiscographyReleaseType.Release ) { Artist = artist.DbId };

			var sut = CreateSut();

			sut.AddDiscography( disco1 );
			sut.AddDiscography( disco2 );
			sut.RemoveDiscography( disco2 );

			using( var discoList = sut.GetDiscography( artist.DbId )) {
				discoList.List.Should().HaveCount( 1 );
			}
		}
	}
}
