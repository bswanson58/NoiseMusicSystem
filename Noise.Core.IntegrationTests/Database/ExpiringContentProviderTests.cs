using FluentAssertions;
using NUnit.Framework;
using Noise.Core.Database;
using Noise.EloqueraDatabase.DataProviders;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.IntegrationTests.Database {
	[TestFixture]
	public class ExpiringContentProviderTests : BaseDatabaseProviderTests {

		private IExpiringContentProvider CreateSut() {
			return( new ExpiringContentProvider( mDatabaseManager ));
		}

		[Test]
		public void CanGetContentList() {
			var dbArtwork1 = new DbArtwork( 1, ContentType.AlbumCover );
			var dbArtwork2 = new DbArtwork( 2, ContentType.AlbumCover );
			var artworkProvider = new ArtworkProvider( mDatabaseManager );

			artworkProvider.AddArtwork( dbArtwork1 );
			artworkProvider.AddArtwork( dbArtwork2 );

			var sut = CreateSut();

			using( var list = sut.GetContentList( 1, ContentType.AlbumCover )) {
				list.List.Should().HaveCount( 1 );
			}
		}

		[Test]
		public void CanGetAlbumContentList() {
			var album = new DbAlbum();
			var dbArtwork1 = new DbArtwork( 1, ContentType.AlbumCover ) { Album = album.DbId };
			var dbArtwork2 = new DbArtwork( 2, ContentType.AlbumArtwork) { Album = album.DbId + 1 };
			var artworkProvider = new ArtworkProvider( mDatabaseManager );

			artworkProvider.AddArtwork( dbArtwork1 );
			artworkProvider.AddArtwork( dbArtwork2 );
			
			var sut = CreateSut();

			using( var list = sut.GetAlbumContentList( album.DbId )) {
				list.List.Should().HaveCount( 1 );
			}
		}
	}
}
