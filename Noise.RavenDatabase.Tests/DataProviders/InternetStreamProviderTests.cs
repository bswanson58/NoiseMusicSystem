﻿using Moq;
using Noise.RavenDatabase.Logging;
using NUnit.Framework;
using Noise.BaseDatabase.Tests.DataProviders;
using Noise.Infrastructure.Interfaces;
using Noise.RavenDatabase.DataProviders;

namespace Noise.RavenDatabase.Tests.DataProviders {
	[TestFixture]
	public class InternetStreamProviderTests : BaseInternetStreamProviderTests {
		private CommonTestSetup	mCommon;
		private Mock<ILogRaven>	mLog;

		[TestFixtureSetUp]
		public void FixtureSetup() {
			mCommon = new CommonTestSetup();
			mCommon.FixtureSetup();

			mLog = new Mock<ILogRaven>();
		}

		[SetUp]
		public void Setup() {
			mCommon.Setup();
		}

		[TearDown]
		public void Teardown() {
			mCommon.Teardown();
		}

		protected override IInternetStreamProvider CreateSut() {
			return ( new InternetStreamProvider( mCommon.DatabaseFactory.Object, mLog.Object ));
		}
	}
}
