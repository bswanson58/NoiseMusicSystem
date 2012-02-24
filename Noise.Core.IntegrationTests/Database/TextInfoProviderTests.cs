using System;
using FluentAssertions;
using NUnit.Framework;
using Noise.EloqueraDatabase.DataProviders;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.IntegrationTests.Database {
	[TestFixture]
	public class TextInfoProviderTests : BaseDatabaseProviderTests {

		private ITextInfoProvider CreateSut() {
			return( new TextInfoProvider( mDatabaseManager ));
		}

		[Test]
		public void CanAddTextInfo() {
			var textInfo = new DbTextInfo( 1, ContentType.Biography );
			var sut = CreateSut();

			sut.AddTextInfo( textInfo );
		}

		[Test]
		[ExpectedException( typeof( ArgumentNullException ))]
		public void CannotAddNullTextInfo() {
			var sut = CreateSut();

			sut.AddTextInfo( null );
		}

		[Test]
		public void CanDeleteTextInfo() {
			var artist = new DbArtist();
			var textInfo = new DbTextInfo( artist.DbId, ContentType.Biography ) { Artist = artist.DbId };
			var sut = CreateSut();

			sut.AddTextInfo( textInfo );
			sut.DeleteTextInfo( textInfo );

			var retrievedText = sut.GetArtistTextInfo( artist.DbId, ContentType.Biography );
			Assert.IsNull( retrievedText );
		}

		[Test]
		public void CanGetTextInfoForArtist() {
			var artist = new DbArtist();
			var textInfo = new DbTextInfo( artist.DbId, ContentType.Biography ) { Artist = artist.DbId };
			var sut = CreateSut();

			sut.AddTextInfo( textInfo );

			var retrievedText = sut.GetArtistTextInfo( artist.DbId, ContentType.Biography );
			Assert.IsNotNull( retrievedText );
			textInfo.ShouldHave().AllProperties().EqualTo( retrievedText );
		}

		[Test]
		public void CanGetTextInfoForAlbum() {
			var album = new DbAlbum();
			var textInfo = new DbTextInfo( album.DbId, ContentType.TextInfo ) { Album = album.DbId };
			var sut = CreateSut();

			sut.AddTextInfo( textInfo );

			var retrievedText = sut.GetAlbumTextInfo( album.DbId );
			Assert.IsNotNull( retrievedText );
			retrievedText.Should().HaveCount( 1 );
			textInfo.ShouldHave().AllProperties().EqualTo( retrievedText[0]);
		}

		[Test]
		public void CanGetTextInfoForUpdate() {
			var album = new DbAlbum();
			var textInfo = new DbTextInfo( album.DbId, ContentType.TextInfo ) { Album = album.DbId };
			var sut = CreateSut();

			sut.AddTextInfo( textInfo );

			using( var updater = sut.GetTextInfoForUpdate( textInfo.DbId )) {
				Assert.IsNotNull( updater );
				Assert.IsNotNull( updater.Item );

				textInfo.ShouldHave().AllProperties().EqualTo( updater.Item );
			}
		}

		[Test]
		public void CanUpdateTextInfo() {
			var album = new DbAlbum();
			var textInfo = new DbTextInfo( album.DbId, ContentType.TextInfo ) { Album = album.DbId, Name = "original name" };
			var sut = CreateSut();

			sut.AddTextInfo( textInfo );

			using( var updater = sut.GetTextInfoForUpdate( textInfo.DbId )) {
				updater.Item.Name = "updated name";

				updater.Update();
			}

			var retrievedText = sut.GetAlbumTextInfo( album.DbId );
			Assert.IsNotNull( retrievedText );
			retrievedText.Should().HaveCount( 1 );
			retrievedText[0].Name.Should().NotBe( textInfo.Name );
		}
	}
}
