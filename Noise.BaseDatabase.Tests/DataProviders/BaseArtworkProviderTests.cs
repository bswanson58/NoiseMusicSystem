using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.BaseDatabase.Tests.DataProviders {
	public abstract class BaseArtworkProviderTests : BaseProviderTest<IArtworkProvider> {
		[Test]
		public void CanAddDbArtwork() {
			var artwork = new DbArtwork( 1, ContentType.AlbumCover );

			var sut = CreateSut();

			sut.AddArtwork( artwork );
		}

		[Test]
		public void CanAddArtwork() {
			var artwork = new DbArtwork( 1, ContentType.AlbumCover );
			var image = new byte[] { 0, 1, 2, 3 };

			var sut = CreateSut();

			sut.AddArtwork( artwork, image );
		}

		[Test]
		public void CanStoreAllArtworkProperties() {
			var dbArtwork = new DbArtwork( 7, ContentType.AlbumArtwork ) { Album = 1,
																			Artist = 2,
																			FolderLocation = 3,
																			IsContentAvailable = true,
																			IsUserSelection = true,
																			Name = "album cover",
																			Rotation = 4,
																			Source = InfoSource.Tag };
			var image = new byte[] { 1, 2, 3, 4 };
			var artwork = new Artwork( dbArtwork ) { Image = image };

			var sut = CreateSut();
			sut.AddArtwork( dbArtwork, image );

			var result = sut.GetArtworkForFolder( 3 );
			result.List.First().ShouldHave().AllProperties().EqualTo( artwork );
		}

		[Test]
		[ExpectedException( typeof( ArgumentNullException ))]
		public void CannotAddNullArtwork() {
			var sut = CreateSut();

			sut.AddArtwork( null );
		}

		[Test]
		[Ignore( "Add artwork from file requires external file to test." )]
		public void CanAddArtworkFromFile() {
		}

		[Test]
		public void CanRetrieveAlbumArtwork() {
			var album = new DbAlbum();
			var dbArtwork = new DbArtwork( 1, ContentType.AlbumArtwork ) { Album = album.DbId };
			var image = new byte[] { 1, 2, 3, 4 };
			var artwork = new Artwork( dbArtwork ) { Image = image };

			var sut = CreateSut();
			sut.AddArtwork( dbArtwork, image );

			var retrievedArtwork = sut.GetAlbumArtwork( album.DbId );

			artwork.ShouldHave().AllProperties().EqualTo( retrievedArtwork[0]);
		}

		[Test]
		public void CanGetAlbumArtworkContentType() {
			var album = new DbAlbum();
			var dbArtwork1 = new DbArtwork( 1, ContentType.AlbumArtwork ) { Album = album.DbId };
			var dbArtwork2 = new DbArtwork( 2, ContentType.AlbumCover ) { Album = album.DbId };
			var image = new byte[] { 1, 2, 3, 4 };

			var sut = CreateSut();
			sut.AddArtwork( dbArtwork1, image );
			sut.AddArtwork( dbArtwork2, image );

			var retrievedArtwork = sut.GetAlbumArtwork( album.DbId, ContentType.AlbumCover );

			retrievedArtwork.Should().HaveCount( 1 );
		}

		[Test]
		public void CanGetAlbumUserSelectedCover() {
			var album = new DbAlbum();
			var dbArtwork1 = new DbArtwork( 1, ContentType.AlbumArtwork ) { Album = album.DbId, IsUserSelection = true };
			var dbArtwork2 = new DbArtwork( 2, ContentType.AlbumCover ) { Album = album.DbId };
			var image = new byte[] { 1, 2, 3, 4 };

			var sut = CreateSut();
			sut.AddArtwork( dbArtwork1, image );
			sut.AddArtwork( dbArtwork2, image );

			var retrievedArtwork = sut.GetAlbumArtwork( album.DbId, ContentType.AlbumCover );

			retrievedArtwork.Should().HaveCount( 2 );
		}

		[Test]
		public void CanGetArtistArtwork() {
			var artist = new DbArtist();
			var dbArtwork1 = new DbArtwork( 1, ContentType.ArtistPrimaryImage ) { Artist = artist.DbId };
			var dbArtwork2 = new DbArtwork( 2, ContentType.AlbumArtwork ) { Artist = artist.DbId };
			var image = new byte[] { 1, 2, 3, 4 };
			var artwork = new Artwork( dbArtwork1 ) { Image = image };

			var sut = CreateSut();
			sut.AddArtwork( dbArtwork1, image );
			sut.AddArtwork( dbArtwork2, image );

			var retrievedArtwork = sut.GetArtistArtwork( artist.DbId, ContentType.ArtistPrimaryImage );

			retrievedArtwork.ShouldHave().AllProperties().EqualTo( artwork );
		}

		[Test]
		public void CanGetArtworkForFolder() {
			var artwork1 = new DbArtwork( 1, ContentType.AlbumCover ) { FolderLocation = 1 };
			var artwork2 = new DbArtwork( 2, ContentType.AlbumArtwork ) { FolderLocation = 2 };

			var sut = CreateSut();
			sut.AddArtwork( artwork1 );
			sut.AddArtwork( artwork2 );

			using( var artworkList = sut.GetArtworkForFolder( 2 )) {
				artworkList.List.Should().HaveCount( 1 );
			}
		}

		[Test]
		public void CanGetArtworkForUpdate() {
			var dbArtwork = new DbArtwork( 1, ContentType.AlbumArtwork );
			var image = new byte[] { 1, 2, 3, 4 };
			var artwork = new Artwork( dbArtwork ) { Image = image };

			var sut = CreateSut();
			sut.AddArtwork( dbArtwork, image );

			using( var updater = sut.GetArtworkForUpdate( artwork.DbId )) {
				artwork.ShouldHave().AllProperties().EqualTo( updater.Item );
			}
		}

		[Test]
		public void CanUpdateArtwork() {
			var dbArtist = new DbArtist();
			var dbArtwork = new DbArtwork( 1, ContentType.ArtistPrimaryImage ) { Artist = dbArtist.DbId, Name = "artwork name" };
			var image = new byte[] { 1, 2, 3, 4 };
			var artwork = new Artwork( dbArtwork ) { Image = image };

			var sut = CreateSut();
			sut.AddArtwork( dbArtwork, image );

			using( var updater = sut.GetArtworkForUpdate( artwork.DbId )) {
				updater.Item.Name = "new name";

				updater.Update();
			}

			var retrievedArtwork = sut.GetArtistArtwork( dbArtist.DbId, ContentType.ArtistPrimaryImage );

			retrievedArtwork.Name.Should().Be( "new name" );
		}

		[Test]
		public void CanDeleteArtwork() {
			var artist = new DbArtist();
			var artwork = new DbArtwork( 1, ContentType.ArtistPrimaryImage ) { Artist = artist.DbId };

			var sut = CreateSut();
			sut.AddArtwork( artwork );
			sut.DeleteArtwork( artwork );

			var retrievedArtwork = sut.GetArtistArtwork( artist.DbId, ContentType.ArtistPrimaryImage );

			Assert.IsNull( retrievedArtwork );
		}
	}
}
