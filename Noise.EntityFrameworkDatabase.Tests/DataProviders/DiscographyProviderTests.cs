using NUnit.Framework;
using Noise.BaseDatabase.Tests.DataProviders;
using Noise.EntityFrameworkDatabase.DataProviders;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.Tests.DataProviders {
	[TestFixture]
	public class DiscographyProviderTests : BaseDiscographyProviderTests {
		private readonly ProviderTestSetup	mTestSetup;

		public DiscographyProviderTests() {
			mTestSetup = new ProviderTestSetup();
		}

		[SetUp]
		public void Setup() {
			mTestSetup.Setup();
		}

		protected override IDiscographyProvider CreateSut() {
			return( new DiscographyProvider( mTestSetup.ContextProvider ));
		}
	}
}
