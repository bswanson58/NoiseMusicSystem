using Caliburn.Micro;
using Moq;
using NUnit.Framework;
using Noise.BaseDatabase.Tests.DataProviders;
using Noise.EntityFrameworkDatabase.DataProviders;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.Tests.DataProviders {
	[TestFixture]
	public class PlayListProviderTests : BasePlayListProviderTests {
		private readonly ProviderTestSetup	mTestSetup;

		public PlayListProviderTests() {
			mTestSetup = new ProviderTestSetup();
		}

		[SetUp]
		public void Setup() {
			mTestSetup.Setup();
		}

		protected override IPlayListProvider CreateSut() {
			var eventAggregator = new Mock<IEventAggregator>();

			return( new PlayListProvider( eventAggregator.Object, mTestSetup.ContextProvider ));
		}
	}
}
