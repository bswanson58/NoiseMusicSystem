﻿using Moq;
using Noise.EntityFrameworkDatabase.Logging;
using NUnit.Framework;
using Noise.BaseDatabase.Tests.DataProviders;
using Noise.EntityFrameworkDatabase.DataProviders;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.Tests.DataProviders {
	[TestFixture]
	public class ArtworkProviderTests : BaseArtworkProviderTests {
		private readonly ProviderTestSetup	        mTestSetup;
		private readonly Mock<ILogDatabase>	        mLog;

		public ArtworkProviderTests() {
			mTestSetup = new ProviderTestSetup();
			mLog = new Mock<ILogDatabase>();
		}

		[SetUp]
		public void Setup() {
			mTestSetup.Setup();
		}

		protected override IArtworkProvider CreateSut() {
			return( new ArtworkProvider( mTestSetup.ContextProvider, mLog.Object, null ));
		}
	}
}
