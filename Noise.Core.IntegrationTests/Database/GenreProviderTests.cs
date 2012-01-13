using System;
using FluentAssertions;
using NUnit.Framework;
using Noise.Core.Database;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.IntegrationTests.Database {
	[TestFixture]
	public class GenreProviderTests : BaseDatabaseProviderTests {

		private IGenreProvider CreateSut() {
			return( new GenreProvider( mDatabaseManager ));
		}

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
