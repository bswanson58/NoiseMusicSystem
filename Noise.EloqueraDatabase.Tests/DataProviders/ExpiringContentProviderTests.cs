using NUnit.Framework;
using Noise.BaseDatabase.Tests.DataProviders;
using Noise.EloqueraDatabase.DataProviders;
using Noise.Infrastructure.Interfaces;

namespace Noise.EloqueraDatabase.Tests.DataProviders {
	[TestFixture]
	public class ExpiringContentProviderTests : BaseExpiringContentProviderTests {
		private readonly ProviderTestSetup	mTestSetup;
		private IArtworkProvider			mArtworkProvider;

		public ExpiringContentProviderTests() {
			mTestSetup = new ProviderTestSetup();
		}

		[SetUp]
		public void Setup() {
			mTestSetup.Setup();
			mArtworkProvider = new ArtworkProvider( mTestSetup.DatabaseManager );
		}

		protected override IExpiringContentProvider CreateSut() {
			return( new ExpiringContentProvider( mTestSetup.DatabaseManager ));
		}

		[TearDown]
		public void TearDown() {
			mTestSetup.Teardown();
		}

		protected override IArtworkProvider ArtworkProvider {
			get { return( mArtworkProvider ); }
		}
	}
}
