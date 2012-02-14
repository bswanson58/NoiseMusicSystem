using System;
using FluentAssertions;
using NUnit.Framework;
using Noise.Core.Database;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.IntegrationTests.Database {
	[TestFixture]
	public class TagAssociationProviderTests : BaseDatabaseProviderTests {

		private ITagAssociationProvider CreateSut() {
			return( new TagAssociationProvider( mDatabaseManager ));
		}

		[Test]
		public void CanAddAssociation() {
			var artist = new DbArtist();
			var album = new DbAlbum();
			var tag = new DbTag( eTagGroup.Decade, "tag name" );
			var association = new DbTagAssociation( eTagGroup.Decade, tag.DbId, artist.DbId, album.DbId );
			var sut = CreateSut();

			sut.AddAssociation( association );
		}

		[Test]
		[ExpectedException( typeof( ArgumentNullException ))]
		public void CannotAddNullAssociation() {
			var sut = CreateSut();

			sut.AddAssociation( null );
		}

		[Test]
		public void CanRemoveAssociation() {
			var album = new DbAlbum();
			var association = new DbTagAssociation( eTagGroup.Decade, album.DbId, album.DbId, album.DbId );
			var sut = CreateSut();

			sut.AddAssociation( association );
			sut.RemoveAssociation( association.DbId );

			var retrievedTag = sut.GetAlbumTagAssociation( album.DbId, album.DbId );
			Assert.IsNull( retrievedTag );
		}

		[Test]
		public void CanGetTagListForArtist() {
			var artist = new DbArtist();
			var association1 = new DbTagAssociation( eTagGroup.User, 1, artist.DbId, 1 );
			var association2 = new DbTagAssociation( eTagGroup.Decade, 2, artist.DbId, 2 );
			var association3 = new DbTagAssociation( eTagGroup.User, 3, artist.DbId + 1, 3 );
			var sut = CreateSut();
			sut.AddAssociation( association1 );
			sut.AddAssociation( association2 );
			sut.AddAssociation( association3 );

			using( var list = sut.GetArtistTagList( artist.DbId, eTagGroup.User )) {
				Assert.IsNotNull( list );
				Assert.IsNotNull( list.List );

				list.List.Should().HaveCount( 1 );
			}
		}

		[Test]
		public void CanGetTagListForAlbum() {
			var album = new DbAlbum();
			var association1 = new DbTagAssociation( eTagGroup.User, 1, 1, album.DbId );
			var association2 = new DbTagAssociation( eTagGroup.Decade, 2, 2, album.DbId );
			var association3 = new DbTagAssociation( eTagGroup.User, 3, 3, album.DbId + 1 );
			var sut = CreateSut();
			sut.AddAssociation( association1 );
			sut.AddAssociation( association2 );
			sut.AddAssociation( association3 );

			using( var list = sut.GetAlbumTagList( album.DbId, eTagGroup.User )) {
				Assert.IsNotNull( list );
				Assert.IsNotNull( list.List );

				list.List.Should().HaveCount( 1 );
			}
		}

		[Test]
		public void CanGetAssociationsForTag() {
			var tag = new DbTag( eTagGroup.Genre, "tag name" );
			var association1 = new DbTagAssociation( eTagGroup.User, tag.DbId, 1, 1 );
			var association2 = new DbTagAssociation( eTagGroup.Decade, tag.DbId, 2, 2 );
			var association3 = new DbTagAssociation( eTagGroup.User, tag.DbId + 1, 3, 3 );
			var sut = CreateSut();
			sut.AddAssociation( association1 );
			sut.AddAssociation( association2 );
			sut.AddAssociation( association3 );

			using( var list = sut.GetTagList( eTagGroup.User, tag.DbId )) {
				Assert.IsNotNull( list );
				Assert.IsNotNull( list.List );

				list.List.Should().HaveCount( 1 );
			}
		}
	}
}
