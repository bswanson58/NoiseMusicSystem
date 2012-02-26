using FluentAssertions;
using NUnit.Framework;
using Noise.EloqueraDatabase.DataProviders;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.IntegrationTests.Database {
	[TestFixture]
	public class DbBaseProviderTests : BaseDatabaseProviderTests {
		private IDbBaseProvider CreateSut() {
			return( new DbBaseProvider( mDatabaseManager ));
		}

		[Test]
		public void CanGetDbBaseItem() {
			var track = new DbTrack();
			var trackProvider = new TrackProvider( mDatabaseManager );

			trackProvider.AddTrack( track );

			var sut = CreateSut();

			var retrievedItem = sut.GetItem( track.DbId );

			track.ShouldHave().AllProperties().EqualTo( retrievedItem );
		}
	}
}
