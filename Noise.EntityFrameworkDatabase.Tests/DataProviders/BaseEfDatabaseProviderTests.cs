using Moq;
using NUnit.Framework;
using Noise.EntityFrameworkDatabase.DatabaseManager;
using Noise.EntityFrameworkDatabase.Interfaces;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.Tests.DataProviders {
	public abstract class BaseEfDatabaseProviderTests {
		protected Mock<ILog>					mDummyLog;
		protected DatabaseConfiguration			mDatabaseConfiguration;
		protected IDatabaseManager				mDatabaseManager;
		protected IDatabaseInitializeStrategy	mInitializeStrategy;
		protected IContextProvider				mContextProvider;

		[SetUp]
		public virtual void Setup() {
			mDummyLog = new Mock<ILog>();
			NoiseLogger.Current = mDummyLog.Object;
				
			mDatabaseConfiguration = new DatabaseConfiguration { DatabaseName = "Integration Test Database" };
			mInitializeStrategy = new TestDatabaseInitializer();
			mContextProvider = new ContextProvider();

			mDatabaseManager = new EntityFrameworkDatabaseManager( mInitializeStrategy, mContextProvider );

			var	initialized = mDatabaseManager.Initialize();
			Assert.IsTrue( initialized );
		}
	}
}
