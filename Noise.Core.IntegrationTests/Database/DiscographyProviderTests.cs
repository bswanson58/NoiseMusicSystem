using System;
using FluentAssertions;
using Microsoft.Practices.Prism.Events;
using Moq;
using NUnit.Framework;
using Noise.AppSupport;
using Noise.Core.Database;
using Noise.EloqueraDatabase;
using Noise.EloqueraDatabase.BlobStore;
using Noise.EloqueraDatabase.Database;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.IntegrationTests.Database {
	[TestFixture]
	public class DiscographyProviderTests {
		private Mock<ILog>							mDummyLog;
		private IIoc								mIocProvider;
		private DatabaseConfiguration				mDatabaseConfiguration;
		private IBlobStorageResolver				mBlobResolver;
		private IDatabaseFactory					mDatabaseFactory;
		private IDatabaseManager					mDatabaseManager;
		private Mock<IEventAggregator>				mEventAggregator;

		[SetUp]
		public void Setup() {
			mDummyLog = new Mock<ILog>();
			NoiseLogger.Current = mDummyLog.Object;
				
			mEventAggregator = new Mock<IEventAggregator> { DefaultValue = DefaultValue.Mock };

			mDatabaseConfiguration = new DatabaseConfiguration { DatabaseName = "Integration Test Database" };

			mIocProvider = new IocProvider();
			mBlobResolver = new BlobStorageResolver();
			mDatabaseFactory = new EloqueraDatabaseFactory( mBlobResolver, mEventAggregator.Object, mIocProvider, mDatabaseConfiguration );
			mDatabaseManager = new DatabaseManager( mDatabaseFactory );

			if( mDatabaseManager.Initialize()) {
				using( var database = mDatabaseManager.CreateDatabase()) {
					database.Database.OpenWithCreateDatabase();
				}
			}
		}

		[TearDown]
		public void Teardown() {
			using( var database = mDatabaseManager.CreateDatabase()) {
				database.Database.DeleteDatabase();
			}
		}

		private IDiscographyProvider CreateSut() {
			return( new DbDiscographyProvider( mDatabaseManager ));
		}

		[Test]
		public void CanAddDiscography() {
			var disco = new DbDiscographyRelease( 1, "title", "format", "label", 1999, DiscographyReleaseType.Release );

			var sut = CreateSut();

			sut.AddDiscography( disco );
		}

		[Test]
		[ExpectedException( typeof( ArgumentNullException ))]
		public void CannotAddNullDiscography() {
			var sut = CreateSut();

			sut.AddDiscography( null );
		}

		[Test]
		public void CanRetrieveDiscography() {
			var artist = new DbArtist();
			var disco1 = new DbDiscographyRelease( 1, "title", "format", "label", 1999, DiscographyReleaseType.Release ) { Artist = artist.DbId };
			var disco2 = new DbDiscographyRelease( 1, "title", "format", "label", 1999, DiscographyReleaseType.Release ) { Artist = artist.DbId + 1 };

			var sut = CreateSut();

			sut.AddDiscography( disco1 );
			sut.AddDiscography( disco2 );

			using( var discoList = sut.GetDiscography( artist.DbId )) {
				discoList.List.Should().HaveCount( 1 );
			}
		}

		[Test]
		public void CanRemoveDiscography() {
			var artist = new DbArtist();
			var disco1 = new DbDiscographyRelease( 1, "title", "format", "label", 1999, DiscographyReleaseType.Release ) { Artist = artist.DbId };
			var disco2 = new DbDiscographyRelease( 2, "title", "format", "label", 1999, DiscographyReleaseType.Release ) { Artist = artist.DbId };

			var sut = CreateSut();

			sut.AddDiscography( disco1 );
			sut.AddDiscography( disco2 );
			sut.RemoveDiscography( disco2 );

			using( var discoList = sut.GetDiscography( artist.DbId )) {
				discoList.List.Should().HaveCount( 1 );
			}
		}
	}
}
