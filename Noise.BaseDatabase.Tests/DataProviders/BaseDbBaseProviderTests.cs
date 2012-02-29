using FluentAssertions;
using NUnit.Framework;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.BaseDatabase.Tests.DataProviders {
	public abstract class BaseDbBaseProviderTests : BaseProviderTest<IDbBaseProvider> {
		protected abstract ITrackProvider	TrackProvider { get; }

		[Test]
		public void CanGetDbBaseItem() {
			var track = new DbTrack();

			TrackProvider.AddTrack( track );

			var sut = CreateSut();

			var retrievedItem = sut.GetItem( track.DbId );

			track.ShouldHave().AllProperties().EqualTo( retrievedItem );
		}
	}
}
