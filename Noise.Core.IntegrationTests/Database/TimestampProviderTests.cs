using FluentAssertions;
using NUnit.Framework;
using Noise.Core.Database;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.IntegrationTests.Database {
	[TestFixture]
	public class TimestampProviderTests : BaseDatabaseProviderTests {

		private ITimestampProvider CreateSut() {
			return( new TimestampProvider( mDatabaseManager ));
		}

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
