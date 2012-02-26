using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.BaseDatabase.Tests.DataProviders {
	public abstract class BasePlayListProviderTests : BaseProviderTest<IPlayListProvider> {
		[Test]
		public void CanAddPlayList() {
			var playList = new DbPlayList();
			var sut = CreateSut();

			sut.AddPlayList( playList );
		}

		[Test]
		[ExpectedException( typeof( ArgumentNullException ))]
		public void CannotAddNullPlayList() {
			var sut = CreateSut();

			sut.AddPlayList( null );
		}

		[Test]
		public void CanRetrievePlayList() {
			var playList = new DbPlayList();

			var sut = CreateSut();
			sut.AddPlayList( playList );

			var retrievedPlayList = sut.GetPlayList( playList.DbId );

			playList.ShouldHave().AllProperties().EqualTo( retrievedPlayList );
		}

		[Test]
		public void CanDeletePlayList() {
			var playList = new DbPlayList();

			var sut = CreateSut();
			sut.AddPlayList( playList );

			sut.DeletePlayList( playList );

			var retrievedPlayList = sut.GetPlayList( playList.DbId );

			Assert.IsNull( retrievedPlayList );
		}

		[Test]
		public void CanRetrieveAllPlayLists() {
			var playList1 = new DbPlayList( "list one", "description", new List<long> { 0, 1, 2 } );
			var playList2 = new DbPlayList( "list two", "description", new List<long> { 1, 2, 3 } );

			var sut = CreateSut();
			sut.AddPlayList( playList1 );
			sut.AddPlayList( playList2 );

			using( var playLists = sut.GetPlayLists()) {
				Assert.IsNotNull( playLists );
				Assert.IsNotNull( playLists.List );

				playLists.List.Should().HaveCount( 2 );
			}
		}

		[Test]
		public void CanRetrievePlayListForUpdate() {
			var playList = new DbPlayList();

			var sut = CreateSut();
			sut.AddPlayList( playList );

			using( var updater = sut.GetPlayListForUpdate( playList.DbId )) {
				Assert.IsNotNull( updater );
				Assert.IsNotNull( updater.Item );

				playList.ShouldHave().AllProperties().EqualTo( updater.Item );
			}
		}

		[Test]
		public void CanUpdatePlayList() {
			var playList = new DbPlayList( "list name", "list description", new List<long> { 1, 2, 3, 4 });

			var sut = CreateSut();
			sut.AddPlayList( playList );

			using( var updater = sut.GetPlayListForUpdate( playList.DbId )) {
				updater.Item.Name = "new list name";

				updater.Update();
			}

			var retrievedItem = sut.GetPlayList( playList.DbId );

			playList.Name.Should().NotBe( retrievedItem.Name );
		}
	}
}
