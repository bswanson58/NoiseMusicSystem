using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.BaseDatabase.Tests.DataProviders {
	public abstract class BasePlayHistoryProviderTests : BaseProviderTest<IPlayHistoryProvider> {
		[Test]
		public void CanAddPlayHistory() {
			var file = new StorageFile( "file name", 1, 100, DateTime.Now );
			var playHistory = new DbPlayHistory( file );

			var sut = CreateSut();

			sut.AddPlayHistory( playHistory );
		}

		[Test]
		[ExpectedException( typeof( ArgumentNullException ))]
		public void CannotAddNullPlayHistory() {
			var sut = CreateSut();

			sut.AddPlayHistory( null );
		}

		[Test]
		public void CanStoreAllHistoryProperties() {
			var file = new StorageFile( "file name", 1, 100, DateTime.Now ) { MetaDataPointer = 2 };
			var playHistory = new DbPlayHistory( file );

			var sut = CreateSut();
			sut.AddPlayHistory( playHistory );

			var result = sut.GetPlayHistoryForUpdate( playHistory.DbId );
			result.Item.ShouldHave().AllProperties().EqualTo( playHistory );
		}

		[Test]
		public void CanRetrievePlayHistoryList() {
			var file = new StorageFile( "file name", 1, 100, DateTime.Now );
			var playHistory = new DbPlayHistory( file );

			var sut = CreateSut();
			sut.AddPlayHistory( playHistory );

			using( var historyList = sut.GetPlayHistoryList()) {
				Assert.IsNotNull( historyList );
				Assert.IsNotNull( historyList.List );

				historyList.List.Should().HaveCount( 1 );
			}
		}

		[Test]
		public void CanDeletePlayHistory() {
			var file = new StorageFile( "file name", 1, 100, DateTime.Now );
			var playHistory = new DbPlayHistory( file );

			var sut = CreateSut();
			sut.AddPlayHistory( playHistory );
			sut.DeletePlayHistory( playHistory );

			using( var historyList = sut.GetPlayHistoryList()) {
				historyList.List.Should().HaveCount( 0 );
			}
		}

		[Test]
		public void CanRetrievePlayHistoryForUpdate() {
			var file = new StorageFile( "file name", 1, 100, DateTime.Now );
			var playHistory = new DbPlayHistory( file );

			var sut = CreateSut();

			sut.AddPlayHistory( playHistory );

			using( var updater = sut.GetPlayHistoryForUpdate( playHistory.DbId )) {
				Assert.IsNotNull( updater.Item );

				playHistory.ShouldHave().AllProperties().EqualTo( updater.Item );
			}
		}

		[Test]
		public void CanUpdatePlayHistory() {
			var file = new StorageFile( "file name", 1, 100, DateTime.Now );
			var playHistory = new DbPlayHistory( file );

			var sut = CreateSut();

			sut.AddPlayHistory( playHistory );

			using( var updater = sut.GetPlayHistoryForUpdate( playHistory.DbId )) {
				updater.Item.PlayedOnTicks = DateTime.Now.Ticks + 1000;

				updater.Update();
			}

			using( var historyList = sut.GetPlayHistoryList()) {
				var retrievedHistory = historyList.List.First();

				playHistory.PlayedOnTicks.Should().NotBe( retrievedHistory.PlayedOnTicks );
			}
		}
	}
}
