using FluentAssertions;
using NUnit.Framework;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.BaseDatabase.Tests.DataProviders {
	public abstract class BaseExpiringContentProviderTests : BaseProviderTest<IExpiringContentProvider> {
		protected abstract IArtworkProvider	ArtworkProvider { get; }

		[Test]
		public void CanGetContentList() {
			var dbArtwork1 = new DbArtwork( 1, ContentType.AlbumCover );
			var dbArtwork2 = new DbArtwork( 2, ContentType.AlbumCover );

			ArtworkProvider.AddArtwork( dbArtwork1 );
			ArtworkProvider.AddArtwork( dbArtwork2 );

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

			ArtworkProvider.AddArtwork( dbArtwork1 );
			ArtworkProvider.AddArtwork( dbArtwork2 );
			
			var sut = CreateSut();

			using( var list = sut.GetAlbumContentList( album.DbId )) {
				list.List.Should().HaveCount( 1 );
			}
		}
	}
}
