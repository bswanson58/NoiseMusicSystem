using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Noise.EloqueraDatabase.DataProviders;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.IntegrationTests.Database {
	[TestFixture]
	public class TagProviderTests : BaseDatabaseProviderTests {

		private ITagProvider CreateSut() {
			return( new TagProvider( mDatabaseManager ));
		}

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
