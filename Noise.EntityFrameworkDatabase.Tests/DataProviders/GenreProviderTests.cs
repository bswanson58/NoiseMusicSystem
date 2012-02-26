using NUnit.Framework;
using Noise.BaseDatabase.Tests.DataProviders;
using Noise.EntityFrameworkDatabase.DataProviders;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.Tests.DataProviders {
	[TestFixture]
	public class GenreProviderTests : BaseGenreProviderTests {
		private readonly ProviderTestSetup	mTestSetup;

		public GenreProviderTests() {
			mTestSetup = new ProviderTestSetup();
		}

		[SetUp]
		public void Setup() {
			mTestSetup.Setup();
		}

		protected override IGenreProvider CreateSut() {
			return( new GenreProvider( mTestSetup.ContextProvider ));
		}
	}
}
