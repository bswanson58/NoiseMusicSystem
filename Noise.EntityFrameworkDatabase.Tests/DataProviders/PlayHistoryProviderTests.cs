using NUnit.Framework;
using Noise.BaseDatabase.Tests.DataProviders;
using Noise.EntityFrameworkDatabase.DataProviders;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.Tests.DataProviders {
	[TestFixture]
	public class PlayHistoryProviderTests : BasePlayHistoryProviderTests {
		private readonly ProviderTestSetup	mTestSetup;

		public PlayHistoryProviderTests() {
			mTestSetup = new ProviderTestSetup();
		}

		[SetUp]
		public void Setup() {
			mTestSetup.Setup();
		}

		protected override IPlayHistoryProvider CreateSut() {
			return( new PlayHistoryProvider( mTestSetup.ContextProvider ));
		}
	}
}
