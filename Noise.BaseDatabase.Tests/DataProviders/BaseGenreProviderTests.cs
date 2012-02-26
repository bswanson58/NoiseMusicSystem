using System;
using FluentAssertions;
using NUnit.Framework;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.BaseDatabase.Tests.DataProviders {
	public abstract class BaseGenreProviderTests : BaseProviderTest<IGenreProvider> {
		[Test]
		public void CanAddGenre() {
			var genre = new DbGenre();

			var sut = CreateSut();

			sut.AddGenre( genre );
		}

		[Test]
		[ExpectedException( typeof( ArgumentNullException ))]
		public void CannotAddNullGenre() {
			var sut = CreateSut();

			sut.AddGenre( null );
		}

		[Test]
		public void CanGetGenreList() {
			var genre = new DbGenre();

			var sut = CreateSut();
			sut.AddGenre( genre );

			using( var list = sut.GetGenreList()) {
				list.List.Should().HaveCount( 1 );
			}
		}
	}
}
