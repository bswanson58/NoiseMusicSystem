using Moq;
using Noise.EntityFrameworkDatabase.Logging;
using NUnit.Framework;
using Noise.BaseDatabase.Tests.DataProviders;
using Noise.EntityFrameworkDatabase.DataProviders;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.Tests.DataProviders {
	[TestFixture]
	public class DbBaseProviderTests : BaseDbBaseProviderTests {
		private readonly ProviderTestSetup	mTestSetup;
		private ITrackProvider				mTrackProvider;
		private Mock<ILogDatabase>			mLog;

		public DbBaseProviderTests() {
			mTestSetup = new ProviderTestSetup();
			mLog = new Mock<ILogDatabase>();
		}

		[SetUp]
		public void Setup() {
			mTestSetup.Setup();
			mTrackProvider = new TrackProvider( mTestSetup.ContextProvider, null, mLog.Object );
		}

		protected override IDbBaseProvider CreateSut() {
			return( new DbBaseProvider( mTestSetup.ContextProvider ));
		}

		protected override ITrackProvider TrackProvider {
			get { return( mTrackProvider ); }
		}
	}
}
