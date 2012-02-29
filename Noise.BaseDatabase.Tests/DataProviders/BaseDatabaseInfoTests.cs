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

			sut.InitializeDatabaseVersion( 1, 2 );
		}

		[Test]
		public void CanReadDatabaseVersion() {
			var sut = CreateSut();

			sut.InitializeDatabaseVersion( 1, 2 );

			var version = sut.DatabaseVersion;

			Assert.IsNotNull( version );
			version.MajorVersion.Should().Be( 1 );
			version.MinorVersion.Should().Be( 2 );
		}
	}
}
