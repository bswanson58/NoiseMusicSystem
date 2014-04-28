using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.BaseDatabase.Tests.DataProviders {
	public abstract class BaseTagProviderTests : BaseProviderTest<ITagProvider> {
		[Test]
		public void CanAddTag() {
			var tag = new DbTag( eTagGroup.User, "tag name" );
			var sut = CreateSut();

			sut.AddTag( tag );
		}

		[Test]
		[ExpectedException( typeof( ArgumentNullException ))]
		public void CannotAddNullTag() {
			var sut = CreateSut();

			sut.AddTag( null );
		}

		[Test]
		public void CanStoreAllTagProperties() {
			var tag = new DbTag( eTagGroup.Genre, "tag name" ) { Description = "description", Rating = 1, IsFavorite = true };

			var sut = CreateSut();
			sut.AddTag( tag );

			var results = sut.GetTagList( eTagGroup.Genre );
			results.List.First().ShouldHave().AllProperties().EqualTo( tag );
		}

		[Test]
		public void CanGetTagList() {
			var tag1 = new DbTag( eTagGroup.User, "user tag" );
			var tag2 = new DbTag( eTagGroup.Decade, "decade tag" );
			var sut = CreateSut();
			sut.AddTag( tag1 );
			sut.AddTag( tag2 );

			using( var tagList = sut.GetTagList( eTagGroup.User )) {
				Assert.IsNotNull( tagList );
				Assert.IsNotNull( tagList.List );

				tagList.List.Should().HaveCount( 1 );

				var retrievedTag = tagList.List.First();

				retrievedTag.ShouldHave().AllProperties().EqualTo( tag1 );
			}
		}
	}
}
