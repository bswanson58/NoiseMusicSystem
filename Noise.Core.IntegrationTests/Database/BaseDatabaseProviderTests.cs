using Caliburn.Micro;
using FluentAssertions;
using Microsoft.Practices.Prism.Events;
using Moq;
using NUnit.Framework;
using Noise.AppSupport;
using Noise.EloqueraDatabase;
using Noise.EloqueraDatabase.BlobStore;
using Noise.EloqueraDatabase.Database;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Interfaces;
using IEventAggregator = Caliburn.Micro.IEventAggregator;

namespace Noise.Core.IntegrationTests.Database {
	public class BaseDatabaseProviderTests {
		protected Mock<ILog>						mDummyLog;
		protected IIoc								mIocProvider;
		protected DatabaseConfiguration				mDatabaseConfiguration;
		protected IBlobStorageResolver				mBlobResolver;
		protected IDatabaseFactory					mDatabaseFactory;
		protected IDatabaseManager					mDatabaseManager;
		protected Mock<IEventAggregator>			mEventAggregator;

		[SetUp]
		public virtual void Setup() {
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
		public virtual void Teardown() {
			mDatabaseManager.ReservedDatabaseCount.Should().Be( 0 );

			using( var database = mDatabaseManager.CreateDatabase()) {
				database.Database.DeleteDatabase();
			}
		}
	}
}
