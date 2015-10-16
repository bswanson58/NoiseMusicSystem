using FluentAssertions;
using NUnit.Framework;
using Noise.Infrastructure.Interfaces;

namespace Noise.BaseDatabase.Tests.DataProviders {
	public abstract class BaseDatabaseInfoTests : BaseProviderTest<IDatabaseInfo> {
		[Test]
		public void CreatedDatabaseHasNoVersion() {
			var sut = CreateSut();

			Assert.IsNull( sut.DatabaseVersion );
		}

		[Test]
		public void CanAddDatabaseVersion() {
			var sut = CreateSut();

			sut.InitializeDatabaseVersion( 1 );
		}

		[Test]
		public void CanReadDatabaseVersion() {
			var sut = CreateSut();

			sut.InitializeDatabaseVersion( 3 );

			var version = sut.DatabaseVersion;

			Assert.IsNotNull( version );
			version.DatabaseVersion.Should().Be( 3 );
		}
	}
}
