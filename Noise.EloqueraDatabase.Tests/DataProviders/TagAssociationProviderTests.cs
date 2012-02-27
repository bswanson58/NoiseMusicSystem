using NUnit.Framework;
using Noise.BaseDatabase.Tests.DataProviders;
using Noise.EloqueraDatabase.DataProviders;
using Noise.Infrastructure.Interfaces;

namespace Noise.EloqueraDatabase.Tests.DataProviders {
	[TestFixture]
	public class TagAssociationProviderTests : BaseTagAssociationProviderTests {
		private readonly ProviderTestSetup	mTestSetup;

		public TagAssociationProviderTests() {
			mTestSetup = new ProviderTestSetup();
		}

		[SetUp]
		public void Setup() {
			mTestSetup.Setup();
		}

		protected override ITagAssociationProvider CreateSut() {
			return( new TagAssociationProvider( mTestSetup.DatabaseManager ));
		}

		[TearDown]
		public void TearDown() {
			mTestSetup.Teardown();
		}
	}
}
