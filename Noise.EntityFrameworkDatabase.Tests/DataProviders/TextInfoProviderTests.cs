using NUnit.Framework;
using Noise.BaseDatabase.Tests.DataProviders;
using Noise.EntityFrameworkDatabase.DataProviders;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.Tests.DataProviders {
	[TestFixture]
	public class TextInfoProviderTests : BaseTextInfoProviderTests {
		private readonly ProviderTestSetup	mTestSetup;

		public TextInfoProviderTests() {
			mTestSetup = new ProviderTestSetup();
		}

		[SetUp]
		public void Setup() {
			mTestSetup.Setup();
		}

		protected override ITextInfoProvider CreateSut() {
			return( new TextInfoProvider( mTestSetup.ContextProvider ));
		}
	}
}
