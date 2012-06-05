using FluentAssertions;
using NUnit.Framework;
using ReusableBits.Interfaces;
using ReusableBits.Support;

namespace ReusableBits.Tests.Support {
	[TestFixture]
	public class IdentityProviderTests {
		private IIdentityProvider CreateSut() {
			return( new IdentityProvider());
		}

		[Test]
		public void CanCreateIdentityProvider() {
			var sut = CreateSut();

			Assert.IsNotNull( sut );
		}

		[Test]
		public void CanGenerateUniqueGuidIdentities() {
			var sut = CreateSut();

			sut.IdentityType = IdentityType.Guid;

			var guid1 = sut.NewIdentityAsGuid();
			var guid2 = sut.NewIdentityAsGuid();

			guid1.Should().NotBe( guid2 );
		}

		[Test]
		public void CanGenerateUniqueLongIdentities() {
			var sut = CreateSut();

			sut.IdentityType = IdentityType.Guid;

			var guid1 = sut.NewIdentityAsLong();
			var guid2 = sut.NewIdentityAsLong();

			guid1.Should().NotBe( guid2 );
		}
		[Test]
		public void CanGenerateUniqueStringIdentities() {
			var sut = CreateSut();

			sut.IdentityType = IdentityType.Guid;

			var guid1 = sut.NewIdentityAsString();
			var guid2 = sut.NewIdentityAsString();

			guid1.Should().NotBe( guid2 );
		}

		[Test]
		public void CanGenerateSequentialGuidIdentities() {
			var sut = CreateSut();

			sut.IdentityType = IdentityType.SequentialGuid;

			var guid1 = sut.NewIdentityAsGuid();
			var guid2 = sut.NewIdentityAsGuid();

			guid1.Should().NotBe( guid2 );
		}

		[Test]
		public void CanGenerateSequentialLongIdentities() {
			var sut = CreateSut();

			sut.IdentityType = IdentityType.SequentialGuid;

			var guid1 = sut.NewIdentityAsLong();
			var guid2 = sut.NewIdentityAsLong();

			guid2.Should().NotBe( guid1 );
		}

		[Test]
		public void CanGenerateSequentialStringIdentities() {
			var sut = CreateSut();

			sut.IdentityType = IdentityType.SequentialGuid;

			var guid1 = sut.NewIdentityAsString();
			var guid2 = sut.NewIdentityAsString();

			guid1.Should().NotMatch( guid2 );
		}

		[Test]
		public void CanGenerateSequentialEndingGuidIdentities() {
			var sut = CreateSut();

			sut.IdentityType = IdentityType.SequentialEndingGuid;

			var guid1 = sut.NewIdentityAsGuid();
			var guid2 = sut.NewIdentityAsGuid();

			guid1.Should().NotBe( guid2 );
		}

		[Test]
		public void CanGenerateSequentialEndingLongIdentities() {
			var sut = CreateSut();

			sut.IdentityType = IdentityType.SequentialEndingGuid;

			var guid1 = sut.NewIdentityAsLong();
			var guid2 = sut.NewIdentityAsLong();

			guid2.Should().NotBe( guid1 );
		}

		[Test]
		public void CanGenerateSequentialEndingStringIdentities() {
			var sut = CreateSut();

			sut.IdentityType = IdentityType.SequentialEndingGuid;

			var guid1 = sut.NewIdentityAsString();
			var guid2 = sut.NewIdentityAsString();

			guid1.Should().NotMatch( guid2 );
		}
	}
}
