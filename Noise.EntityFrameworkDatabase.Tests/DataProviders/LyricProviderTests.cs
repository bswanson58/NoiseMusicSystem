using NUnit.Framework;
using Noise.BaseDatabase.Tests.DataProviders;
using Noise.EntityFrameworkDatabase.DataProviders;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.Tests.DataProviders {
	[TestFixture]
	public class LyricProviderTests : BaseLyricProviderTests {
		private readonly ProviderTestSetup	mTestSetup;

		public LyricProviderTests() {
			mTestSetup = new ProviderTestSetup();
		}

		[SetUp]
		public void Setup() {
			mTestSetup.Setup();
		}

		protected override ILyricProvider CreateSut() {
			return( new LyricProvider( mTestSetup.ContextProvider ));
		}
	}
}
