using FluentAssertions;
using NUnit.Framework;
using Noise.BaseDatabase.Tests.DataProviders;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.RavenDatabase.DataProviders;

namespace Noise.RavenDatabase.Tests.DataProviders {
	[TestFixture]
	public class LyricProviderTests : BaseLyricProviderTests {
		private CommonTestSetup	mCommon;

		[TestFixtureSetUp]
		public void FixtureSetup() {
			mCommon = new CommonTestSetup();
			mCommon.FixtureSetup();
		}

		[SetUp]
		public void Setup() {
			mCommon.Setup();
		}

		[TearDown]
		public void Teardown() {
			mCommon.Teardown();
		}

		protected override ILyricProvider CreateSut() {
			return ( new LyricProvider( mCommon.DatabaseFactory.Object ) );
		}
	}
}
