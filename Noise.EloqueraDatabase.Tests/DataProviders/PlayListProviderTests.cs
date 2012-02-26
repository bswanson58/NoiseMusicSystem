using Caliburn.Micro;
using Moq;
using NUnit.Framework;
using Noise.BaseDatabase.Tests.DataProviders;
using Noise.EloqueraDatabase.DataProviders;
using Noise.Infrastructure.Interfaces;

namespace Noise.EloqueraDatabase.Tests.DataProviders {
	[TestFixture]
	public class PlayListProviderTests : BasePlayListProviderTests {
		private readonly ProviderTestSetup	mTestSetup;
		private Mock<IEventAggregator>		mEventAggregator;

		public PlayListProviderTests() {
			mTestSetup = new ProviderTestSetup();
		}

		[SetUp]
		public void Setup() {
			mTestSetup.Setup();
		}

		protected override IPlayListProvider CreateSut() {
			mEventAggregator = new Mock<IEventAggregator>();

			return( new PlayListProvider( mEventAggregator.Object, mTestSetup.DatabaseManager ));
		}

		[TearDown]
		public void TearDown() {
			mTestSetup.Teardown();
		}
	}
}
