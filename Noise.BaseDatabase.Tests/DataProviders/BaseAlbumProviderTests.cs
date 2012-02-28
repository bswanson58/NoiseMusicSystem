using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.BaseDatabase.Tests.DataProviders {
	public abstract class BaseAlbumProviderTests : BaseProviderTest<IAlbumProvider> {
		protected Mock<IArtworkProvider>			mArtworkProvider;
		protected Mock<ITextInfoProvider>			mTextInfoProvider;
		protected Mock<ITagAssociationProvider>		mAssociationProvider;
		[Test]
		public void CanAddAlbum() {
			var album = new DbAlbum();

			var sut = CreateSut();

			sut.AddAlbum( album );
		}

		[Test]
		[ExpectedException( typeof( ArgumentNullException ))]
		public void CannotAddNullAlbum() {
			var sut = CreateSut();

			sut.AddAlbum( null );
		}

		[Test]
		public void CannotAddExistingAlbum() {
			var album = new DbAlbum { Artist = 1 };

			var sut = CreateSut();

			sut.AddAlbum( album );
			sut.AddAlbum( album );

			using( var albumList = sut.GetAlbumList( album.Artist )) {
				albumList.List.Should().HaveCount( 1 );
			}
		}

		[Test]
		public void CanDeleteAlbum() {
			var album = new DbAlbum();

			var sut = CreateSut();

			sut.AddAlbum( album );
			sut.DeleteAlbum( album );

			var retrievedAlbum = sut.GetAlbum( album.DbId );

			Assert.IsNull( retrievedAlbum );
		}

		[Test]
		public void CanRetrieveAlbum() {
			var	album = new DbAlbum { Name = "album name" };

			var sut = CreateSut();

			sut.AddAlbum( album );
			var retrievedAlbum = sut.GetAlbum( album.DbId );

			album.ShouldHave().AllProperties().EqualTo( retrievedAlbum );
		}

		[Test]
		public void CanRetrieveAlbumForTrack() {
			var album = new DbAlbum();
			var track = new DbTrack { Album = album.DbId };

			var sut = CreateSut();

			sut.AddAlbum( album );

			var retrievedAlbum = sut.GetAlbumForTrack( track );

			album.ShouldHave().AllProperties().EqualTo( retrievedAlbum );
		}

		[Test]
		public void CanGetAllAlbums() {
			var album1 = new DbAlbum { Artist = 1 };
			var album2 = new DbAlbum { Artist = 2 };

			var sut = CreateSut();
			sut.AddAlbum( album1 );
			sut.AddAlbum( album2 );

			using( var albumList = sut.GetAllAlbums()) {
				albumList.List.Should().HaveCount( 2 );
			}
		}

		[Test]
		public void CanGetAlbumListForArtist() {
			var artist = new DbArtist();
			var album1 = new DbAlbum { Artist = artist.DbId };
			var album2 = new DbAlbum { Artist = artist.DbId + 1};

			var sut = CreateSut();
			sut.AddAlbum( album1 );
			sut.AddAlbum( album2 );

			using( var albumList = sut.GetAlbumList( artist )) {
				albumList.List.Should().HaveCount( 1 );
			}
		}

		[Test]
		public void CanGetAlbumListForArtistId() {
			var artist = new DbArtist();
			var album1 = new DbAlbum { Artist = artist.DbId };
			var album2 = new DbAlbum { Artist = artist.DbId + 1};

			var sut = CreateSut();
			sut.AddAlbum( album1 );
			sut.AddAlbum( album2 );

			using( var albumList = sut.GetAlbumList( artist.DbId )) {
				albumList.List.Should().HaveCount( 1 );
			}
		}

		[Test]
		public void CanGetFavoriteAlbumList() {
			var album1 = new DbAlbum { IsFavorite = true };
			var album2 = new DbAlbum { IsFavorite = false };

			var sut = CreateSut();
			sut.AddAlbum( album1 );
			sut.AddAlbum( album2 );

			using( var albumList = sut.GetFavoriteAlbums()) {
				albumList.List.Should().HaveCount( 1 );
			}
		}

		[Test]
		public void CanGetAlbumForUpdate() {
			var album = new DbAlbum { Name = "album Name" };

			var sut = CreateSut();
			sut.AddAlbum( album );

			using( var updater = sut.GetAlbumForUpdate( album.DbId )) {
				album.ShouldHave().AllProperties().EqualTo( updater.Item );
			}
		}

		[Test]
		public void CanUpdateAlbum() {
			var album = new DbAlbum { Name = "album Name" };

			var sut = CreateSut();
			sut.AddAlbum( album );

			using( var updater = sut.GetAlbumForUpdate( album.DbId )) {
				updater.Item.Name = "new name";

				updater.Update();
			}
			var retrievedAlbum = sut.GetAlbum( album.DbId );

			retrievedAlbum.Name.Should().Be( "new name" );
		}

		[Test]
		public void CanGetAlbumsInCategory() {
			var tags = new List<DbTagAssociation> { new DbTagAssociation( eTagGroup.User, 1, 1, 3 ),
													new DbTagAssociation( eTagGroup.User, 1, 2, 4 )};
			var provider = new Mock<IDataProviderList<DbTagAssociation>>();
 
			provider.Setup( m => m.List ).Returns( tags );
			mAssociationProvider.Setup( m => m.GetTagList( It.Is<eTagGroup>( p => p == eTagGroup.User ), It.IsAny<long>())).Returns( provider.Object );

			var sut = CreateSut();
			
			using( var categoryList = sut.GetAlbumsInCategory( 1 )) {
				categoryList.List.Should().HaveCount( 2 );
			}
		}

		[Test]
		public void CanGetCategoriesForAlbum() {
			var album = new DbAlbum();

			var tags = new List<DbTagAssociation> { new DbTagAssociation( eTagGroup.User, 1, 1, album.DbId ),
													new DbTagAssociation( eTagGroup.User, 2, 2, album.DbId )};
			var provider = new Mock<IDataProviderList<DbTagAssociation>>();
 
			provider.Setup( m => m.List ).Returns( tags );
			mAssociationProvider.Setup( m => m.GetAlbumTagList( It.IsAny<long>(), It.Is<eTagGroup>( p => p == eTagGroup.User ))).Returns( provider.Object );

			var sut = CreateSut();
			
			using( var categoryList = sut.GetAlbumCategories( album.DbId )) {
				categoryList.List.Should().HaveCount( 2 );
			}
		}

		[Test]
		public void CanSetCategoriesForAlbumRemovesOldCategories() {
			var album = new DbAlbum();

			var tag1 = new DbTagAssociation( eTagGroup.User, 1, 1, album.DbId );
			var	tag2 = new DbTagAssociation( eTagGroup.User, 2, 1, album.DbId );
			var tags = new List<DbTagAssociation> { tag1, tag2 };
			var provider = new Mock<IDataProviderList<DbTagAssociation>>();
 
			provider.Setup( m => m.List ).Returns( tags );
			mAssociationProvider.Setup( m => m.GetAlbumTagList( It.IsAny<long>(), It.Is<eTagGroup>( p => p == eTagGroup.User ))).Returns( provider.Object );
			mAssociationProvider.Setup( m => m.GetAlbumTagAssociation( It.IsAny<long>(), 1 )).Returns( tag1 );
			mAssociationProvider.Setup( m => m.GetAlbumTagAssociation( It.IsAny<long>(), 2 )).Returns( tag2 );
			mAssociationProvider.Setup( m => m.RemoveAssociation( It.Is<long>( p => p == tag2.DbId ))).Verifiable();

			var sut = CreateSut();

			var newCategories = new List<long> { 1 };
			sut.SetAlbumCategories( 1, album.DbId, newCategories );

			mAssociationProvider.Verify();
		}

		[Test]
		public void CanSetCategoriesForAlbumAddsNewCategories() {
			var album = new DbAlbum();

			var tags = new List<DbTagAssociation> { new DbTagAssociation( eTagGroup.User, 1, 1, album.DbId )};
			var provider = new Mock<IDataProviderList<DbTagAssociation>>();
 
			provider.Setup( m => m.List ).Returns( tags );
			mAssociationProvider.Setup( m => m.GetAlbumTagList( It.IsAny<long>(), It.Is<eTagGroup>( p => p == eTagGroup.User ))).Returns( provider.Object );
			mAssociationProvider.Setup( m => m.AddAssociation( It.Is<DbTagAssociation>( p => p.TagId == 2 ))).Verifiable();

			var sut = CreateSut();

			var newCategories = new List<long> { 1, 2 };
			sut.SetAlbumCategories( 1, album.DbId, newCategories );

			mAssociationProvider.Verify();
		}

		[Test]
		public void CanGetItemCount() {
			var album1 = new DbAlbum();
			var album2 = new DbAlbum();
			var album3 = new DbAlbum();

			var sut = CreateSut();

			sut.AddAlbum( album1 );
			sut.AddAlbum( album2 );
			sut.AddAlbum( album3 );

			var	albumCount = sut.GetItemCount();

			albumCount.Should().Be( 3 );
		}

		[Test]
		public void CanGetAlbumSupportInfo() {
			var album = new DbAlbum();

			var dbTextInfo = new DbTextInfo( album.DbId, ContentType.TextInfo );
			var textInfo = new TextInfo( dbTextInfo ) { Text = "some album info" };
			mTextInfoProvider.Setup( m => m.GetAlbumTextInfo( It.Is<long>( p => p == album.DbId))).Returns( new [] { textInfo });

			var dbArtwork1 = new DbArtwork( album.DbId, ContentType.AlbumCover );
			var artwork1 = new Artwork( dbArtwork1 );
			mArtworkProvider.Setup( m => m.GetAlbumArtwork( It.Is<long>( p => p == album.DbId ),
															 It.Is<ContentType>( p => p == ContentType.AlbumCover ))).Returns( new [] { artwork1 });

			var dbArtwork2 = new DbArtwork( album.DbId, ContentType.AlbumArtwork );
			var artwork2 = new Artwork( dbArtwork2 );
			mArtworkProvider.Setup( m => m.GetAlbumArtwork( It.Is<long>( p => p == album.DbId ),
															 It.Is<ContentType>( p => p == ContentType.AlbumArtwork ))).Returns( new [] { artwork2 });

			var sut = CreateSut();

			var albumInfo = sut.GetAlbumSupportInfo( album.DbId );

			albumInfo.AlbumCovers[0].ShouldHave().AllProperties().EqualTo( artwork1 );
			albumInfo.Artwork[0].ShouldHave().AllProperties().EqualTo( artwork2 );
			albumInfo.Info[0].ShouldHave().AllProperties().EqualTo( textInfo );
		}
	}
}
