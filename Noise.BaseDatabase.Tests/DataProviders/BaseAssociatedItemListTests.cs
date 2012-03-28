﻿using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.BaseDatabase.Tests.DataProviders {
	public abstract class BaseAssociatedItemListTests : BaseProviderTest<IAssociatedItemListProvider> {
		[Test]
		public void CanAddAssociationList() {
			var associationList = new DbAssociatedItemList( 1, ContentType.BandMembers );
			var sut = CreateSut();

			sut.AddAssociationList( associationList );
		}

		[Test]
		[ExpectedException( typeof( ArgumentNullException ))]
		public void CannotAddNullAssociationList() {
			var sut = CreateSut();

			sut.AddAssociationList( null );
		}

		[Test]
		public void CannotAddExistingItem() {
			var associationList = new DbAssociatedItemList( 1, ContentType.BandMembers );
			var sut = CreateSut();

			sut.AddAssociationList( associationList );
			sut.AddAssociationList( associationList );

			using( var list = sut.GetAssociatedItemLists( ContentType.BandMembers )) {
				list.List.Should().HaveCount( 1 );
			}
		}

		[Test]
		public void CanRetrieveArtistAssociatedItems() {
			var artist = new DbArtist();
			var associationList = new DbAssociatedItemList( 1, ContentType.BandMembers ) { Artist = artist.DbId };

			var sut = CreateSut();
			sut.AddAssociationList( associationList );

			var retrievedAssociationList = sut.GetAssociatedItems( artist.DbId, ContentType.BandMembers );

			associationList.ShouldHave().AllProperties().EqualTo( retrievedAssociationList );
		}

		[Test]
		public void CanRetrieveAssociatedItemListItems() {
			var artist = new DbArtist();
			var list = new DbAssociatedItemList( 2, ContentType.TopAlbums ) { Artist = artist.DbId };
			var strings = new List<string> { "one", "2", "three" };
			list.SetItems( strings );

			var sut = CreateSut();
			sut.AddAssociationList( list );

			var retrievedAssociationList = sut.GetAssociatedItems( artist.DbId, ContentType.TopAlbums );
			retrievedAssociationList.Items.Should().HaveCount( 3 );
		}

		[Test]
		public void CanGetAssociationItemListForContentType() {
			var list1 = new DbAssociatedItemList( 1, ContentType.BandMembers );
			var list2 = new DbAssociatedItemList( 2, ContentType.Discography);

			var sut = CreateSut();

			sut.AddAssociationList( list1 );
			sut.AddAssociationList( list2 );

			using( var retrievedList = sut.GetAssociatedItemLists( ContentType.Discography )) {
				retrievedList.List.Should().HaveCount( 1 );
			}
		}

		[Test]
		public void CanGetAssociationItemListForArtist() {
			var artist = new DbArtist();
			var list1 = new DbAssociatedItemList( 1, ContentType.BandMembers ) { Artist = artist.DbId };
			var list2 = new DbAssociatedItemList( 2, ContentType.Discography) { Artist = artist.DbId + 1 };

			var sut = CreateSut();

			sut.AddAssociationList( list1 );
			sut.AddAssociationList( list2 );

			using( var retrievedList = sut.GetAssociatedItemLists( artist.DbId )) {
				retrievedList.List.Should().HaveCount( 1 );
			}
		}

		[Test]
		public void CanGetAssociationListForUpdate() {
			var list = new DbAssociatedItemList( 2, ContentType.TopAlbums );

			var sut = CreateSut();
			sut.AddAssociationList( list );

			using( var updater = sut.GetAssociationForUpdate( list.DbId )) {
				list.ShouldHave().AllProperties().EqualTo( updater.Item );
			}
		}

		[Test]
		public void CanUpdateAssociationItemList() {
			var artist = new DbArtist();
			var list = new DbAssociatedItemList( 2, ContentType.TopAlbums ) { Artist = artist.DbId };
			var strings = new List<string> { "one", "2", "three" };
			list.SetItems( strings );

			var sut = CreateSut();
			sut.AddAssociationList( list );

			using( var updater = sut.GetAssociationForUpdate( list.DbId )) {
				strings.Add( "four" );

				updater.Item.SetItems( strings );
				updater.Update();
			}

			var retrievedList = sut.GetAssociatedItems( artist.DbId, ContentType.TopAlbums );
			
			retrievedList.Items.Should().HaveSameCount( strings );
		}
	}
}