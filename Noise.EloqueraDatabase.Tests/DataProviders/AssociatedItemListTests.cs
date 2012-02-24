using NUnit.Framework;
using Noise.BaseDatabase.Tests.DataProviders;
using Noise.EloqueraDatabase.DataProviders;
using Noise.Infrastructure.Interfaces;

namespace Noise.EloqueraDatabase.Tests.DataProviders {
	[TestFixture]
	public class AssociatedItemListTests : BaseAssociatedItemListTests {
		private readonly ProviderTestSetup	mTestSetup;

		public AssociatedItemListTests() {
			mTestSetup = new ProviderTestSetup();
		}

		[SetUp]
		public void Setup() {
			mTestSetup.Setup();
		}

		protected override IAssociatedItemListProvider CreateSut() {
			return( new AssociatedItemListProvider( mTestSetup.DatabaseManager ));
		}

		[TearDown]
		public void TearDown() {
			mTestSetup.Teardown();
		}
	}
}
