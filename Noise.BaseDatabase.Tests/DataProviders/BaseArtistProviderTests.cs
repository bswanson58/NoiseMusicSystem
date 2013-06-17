using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.BaseDatabase.Tests.DataProviders {
	public abstract class BaseArtistProviderTests : BaseProviderTest<IArtistProvider> {
		protected Mock<ITagAssociationProvider>	mTagProvider;

		[Test]
		public void CanAddArtist() {
			var artist = new DbArtist { Name = "test name" };

			var sut = CreateSut();
			sut.AddArtist( artist );
		}

		[Test]
		public void CanRetrieveArtist() {
			var	artist = new DbArtist { Name = "find me" };

			var sut = CreateSut();
			sut.AddArtist( artist );

			var returnedArtist = sut.GetArtist( artist.DbId );

			returnedArtist.ShouldHave().AllProperties().EqualTo( artist );
		}

		[Test]
		public void CannotAddExistingArtist() {
			var artist = new DbArtist { Name = "an artist" };

			var sut = CreateSut();
			sut.AddArtist( artist );
			sut.AddArtist( artist );

			using( var artistList = sut.GetArtistList()) {
				artistList.List.Should().HaveCount( 1 );
			}
		}

		[Test]
		[ExpectedException( typeof( ArgumentNullException ))]
		public void CannotAddNullArtist() {
			var sut = CreateSut();

			sut.AddArtist( null );
		}

		[Test]
		public void CanDeleteArtist() {
			var artist = new DbArtist { Name = "delete me if you can" };

			var sut = CreateSut();

			sut.AddArtist( artist );
			sut.DeleteArtist( artist );

			var deletedArtist = sut.GetArtist( artist.DbId );

			Assert.IsNull( deletedArtist );
		}

		[Test]
		public void CanGetArtistList() {
			var artist1 = new DbArtist { Name = "artist 1" };
			var artist2 = new DbArtist { Name = "artist 2" };

			var sut = CreateSut();

			sut.AddArtist( artist1 );
			sut.AddArtist( artist2 );

			using( var artistList = sut.GetArtistList()) {
				artistList.List.Should().HaveCount( 2 );
			}
		}

		[Test]
		public void CanGetArtistForAlbum() {
			var artist = new DbArtist();
			var album = new DbAlbum { Artist = artist.DbId };
			
			var sut = CreateSut();

			sut.AddArtist( artist );

			var returnedArtist = sut.GetArtistForAlbum( album );

			returnedArtist.ShouldHave().AllProperties().EqualTo( artist );
		}

		[Test]
		public void CanGetChangedArtists() {
			var artist1 = new DbArtist();
			var artist2 = new DbArtist();

			System.Threading.Thread.Sleep( 100 );
			var now = DateTime.Now.Ticks;
			System.Threading.Thread.Sleep( 100 );
			artist2.UpdateLastChange();

			var sut = CreateSut();

			sut.AddArtist( artist1 );
			sut.AddArtist( artist2 );

			using( var changedArtists = sut.GetChangedArtists( now )) {
				changedArtists.List.Should().HaveCount( 1 );
			}
		}

		[Test]
		public void CanGetFavoriteArtists() {
			var artist1 = new DbArtist();
			var artist2 = new DbArtist { IsFavorite = true };

			var sut = CreateSut();

			sut.AddArtist( artist1 );
			sut.AddArtist( artist2 );

			using( var changedArtists = sut.GetFavoriteArtists()) {
				changedArtists.List.Should().HaveCount( 1 );
			}
		}

		[Test]
		public void CanUpdateArtist() {
			var artist = new DbArtist { Name = "old name" };

			var sut = CreateSut();

			sut.AddArtist( artist );
			using( var updater = sut.GetArtistForUpdate( artist.DbId )) {
				updater.Item.Name = "new name";

				updater.Update();
			}

			var changedArtist = sut.GetArtist( artist.DbId );

			changedArtist.Name.Should().NotBe( artist.Name );
		}

		[Test]
		public void CanUpdateArtistLastChanged() {
			var artist = new DbArtist();

			var sut = CreateSut();

			sut.AddArtist( artist );

			System.Threading.Thread.Sleep( 100 );
			sut.UpdateArtistLastChanged( artist.DbId );

			var updatedArtist = sut.GetArtist( artist.DbId );

			updatedArtist.LastChangeTicks.Should().BeGreaterThan( artist.LastChangeTicks );
		}

		[Test]
		public void CanGetArtistCategories() {
			var artist = new DbArtist();

			var tags = new List<DbTagAssociation> { new DbTagAssociation( eTagGroup.User, 1, artist.DbId, 1 ),
													new DbTagAssociation( eTagGroup.User, 2, artist.DbId, 2 )};
			var tagList = new Mock<IDataProviderList<DbTagAssociation>>();
			tagList.Setup( m => m.List ).Returns( tags );
			mTagProvider.Setup( o => o.GetArtistTagList( artist.DbId, eTagGroup.User )).Returns( tagList.Object );

			var sut = CreateSut();
			
			using( var categoryList = sut.GetArtistCategories( artist.DbId )) {
				categoryList.List.Should().HaveCount( 2 );
			}
		}

		[Test]
		public void CanGetItemCount() {
			var	artist1 = new DbArtist();
			var artist2 = new DbArtist();
			var artist3 = new DbArtist();

			var sut = CreateSut();

			sut.AddArtist( artist1 );
			sut.AddArtist( artist2 );
			sut.AddArtist( artist3 );

			var itemCount = sut.GetItemCount();

			itemCount.Should().Be( 3 );
		}
	}
}
