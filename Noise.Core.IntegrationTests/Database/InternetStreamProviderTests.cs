using System;
using FluentAssertions;
using NUnit.Framework;
using Noise.EloqueraDatabase.DataProviders;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.IntegrationTests.Database {
	[TestFixture]
	public class InternetStreamProviderTests : BaseDatabaseProviderTests {

		private IInternetStreamProvider CreateSut() {
			return( new InternetStreamProvider( mDatabaseManager ));
		}

		[Test]
		public void CanAddStream() {
			var stream = new DbInternetStream();

			var sut = CreateSut();

			sut.AddStream( stream );
		}

		[Test]
		[ExpectedException( typeof( ArgumentNullException ))]
		public void CannotAddNullStream() {
			var sut = CreateSut();

			sut.AddStream( null );
		}

		[Test]
		public void CannotAddDuplicateStream() {
			var stream = new DbInternetStream();

			var sut = CreateSut();

			sut.AddStream( stream );
			sut.AddStream( stream );

			using( var streamList = sut.GetStreamList()) {
				streamList.List.Should().HaveCount( 1 );
			}
		}

		[Test]
		public void CanRetrieveStream() {
			var stream = new DbInternetStream();

			var sut = CreateSut();
			sut.AddStream( stream );

			var retrieved = sut.GetStream( stream.DbId );

			stream.ShouldHave().AllProperties().EqualTo( retrieved );
		}

		[Test]
		public void CanRetrieveStreamList() {
			var stream1 = new DbInternetStream { Name = "stream 1" };
			var stream2 = new DbInternetStream { Name = "stream 2" };

			var sut = CreateSut();
			sut.AddStream( stream1 );
			sut.AddStream( stream2 );

			using( var streamList = sut.GetStreamList()) {
				Assert.IsNotNull( streamList.List );
				
				streamList.List.Should().HaveCount( 2 );
			}
		}

		[Test]
		public void CanDeleteStream() {
			var stream = new DbInternetStream();

			var sut = CreateSut();
			sut.AddStream( stream );

			sut.DeleteStream( stream );

			var retrievedStream = sut.GetStream( stream.DbId );

			Assert.IsNull( retrievedStream );
		}

		[Test]
		public void CanUpdateStream() {
			var stream = new DbInternetStream { Name = "stream name" };

			var sut = CreateSut();
			sut.AddStream( stream );

			using( var updater = sut.GetStreamForUpdate( stream.DbId )) {
				Assert.IsNotNull( updater.Item );

				updater.Item.Name = "updated name";

				updater.Update();
			}

			var updatedStream = sut.GetStream( stream.DbId );

			updatedStream.Name.Should().NotBe( stream.Name );
		}
	}
}
