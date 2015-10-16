using Moq;
using Noise.EntityFrameworkDatabase.Logging;
using NUnit.Framework;
using Noise.BaseDatabase.Tests.DataProviders;
using Noise.EntityFrameworkDatabase.DataProviders;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.Tests.DataProviders {
	[TestFixture]
	public class InternetStreamProviderTests : BaseInternetStreamProviderTests {
		private readonly ProviderTestSetup	mTestSetup;
		private readonly Mock<ILogDatabase>	mLog;

		public InternetStreamProviderTests() {
			mTestSetup = new ProviderTestSetup();
			mLog = new Mock<ILogDatabase>();
		}

		[SetUp]
		public void Setup() {
			mTestSetup.Setup();
		}

		protected override IInternetStreamProvider CreateSut() {
			return( new InternetStreamProvider( mTestSetup.ContextProvider, mLog.Object ));
		}
	}
}
