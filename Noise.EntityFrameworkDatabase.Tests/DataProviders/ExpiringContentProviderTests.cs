using NUnit.Framework;
using Noise.BaseDatabase.Tests.DataProviders;
using Noise.EntityFrameworkDatabase.DataProviders;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.Tests.DataProviders {
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
			mArtworkProvider = new ArtworkProvider( mTestSetup.ContextProvider );
		}

		protected override IExpiringContentProvider CreateSut() {
			return( new ExpiringContentProvider( mTestSetup.ContextProvider ));
		}

		protected override IArtworkProvider ArtworkProvider {
			get { return( mArtworkProvider ); }
		}
	}
}
