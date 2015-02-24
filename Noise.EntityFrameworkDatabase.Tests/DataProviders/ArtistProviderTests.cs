using Moq;
using Noise.EntityFrameworkDatabase.Logging;
using NUnit.Framework;
using Noise.BaseDatabase.Tests.DataProviders;
using Noise.EntityFrameworkDatabase.DataProviders;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.Tests.DataProviders {
	[TestFixture]
	public class ArtistProviderTests : BaseArtistProviderTests {
		private readonly ProviderTestSetup		mTestSetup;
		private readonly Mock<ILogDatabase>	mLog;

		public ArtistProviderTests() {
			mTestSetup = new ProviderTestSetup();
			mLog = new Mock<ILogDatabase>();
		}

		[SetUp]
		public void Setup() {
			mTestSetup.Setup();

			mTagProvider = new Mock<ITagAssociationProvider>();
		}

		protected override IArtistProvider CreateSut() {
			return( new ArtistProvider( mTestSetup.ContextProvider, mTagProvider.Object, mLog.Object ));
		}
	}
}
