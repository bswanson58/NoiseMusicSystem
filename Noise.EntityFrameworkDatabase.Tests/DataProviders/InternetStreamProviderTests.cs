using NUnit.Framework;
using Noise.BaseDatabase.Tests.DataProviders;
using Noise.EntityFrameworkDatabase.DataProviders;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.Tests.DataProviders {
	[TestFixture]
	public class InternetStreamProviderTests : BaseInternetStreamProviderTests {
		private readonly ProviderTestSetup	mTestSetup;

		public InternetStreamProviderTests() {
			mTestSetup = new ProviderTestSetup();
		}

		[SetUp]
		public void Setup() {
			mTestSetup.Setup();
		}

		protected override IInternetStreamProvider CreateSut() {
			return( new InternetStreamProvider( mTestSetup.ContextProvider ));
		}
	}
}
