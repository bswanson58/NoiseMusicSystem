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
	public class DbBaseProviderTests {
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

		[Test]
		public void CanGetDatabaseId() {
			var sut = CreateSut();
			var databaseId = sut.DatabaseInstanceId();

			databaseId.Should().NotBe( 0 );
		}
	}
}
