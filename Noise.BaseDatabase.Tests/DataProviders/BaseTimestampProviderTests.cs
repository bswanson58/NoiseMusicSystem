using FluentAssertions;
using NUnit.Framework;
using Noise.Infrastructure.Interfaces;

namespace Noise.BaseDatabase.Tests.DataProviders {
	public abstract class BaseTimestampProviderTests : BaseProviderTest<ITimestampProvider> {
		[Test]
		public void CanSetNewTimestamp() {
			var sut = CreateSut();

			sut.SetTimestamp( "component", 1 );
		}

		[Test]
		public void CanRetrieveTimestamp() {
			var sut = CreateSut();

			sut.SetTimestamp( "component1", 17 );
			var retrievedTimestamp = sut.GetTimestamp( "component1" );

			retrievedTimestamp.Should().Be( 17 );
		}

		[Test]
		public void CanUpdateTimestamp() {
			var sut = CreateSut();

			sut.SetTimestamp( "component1", 17 );
			sut.SetTimestamp( "component1", 23 );
			var retrievedTimestamp = sut.GetTimestamp( "component1" );

			retrievedTimestamp.Should().Be( 23 );
		}
	}
}
