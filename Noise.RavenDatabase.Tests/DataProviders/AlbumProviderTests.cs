﻿using Moq;
using NUnit.Framework;
using Noise.BaseDatabase.Tests.DataProviders;
using Noise.Infrastructure.Interfaces;
using Noise.RavenDatabase.DataProviders;

namespace Noise.RavenDatabase.Tests.DataProviders {
	[TestFixture]
	public class AlbumProviderTests  : BaseAlbumProviderTests {
		private CommonTestSetup	mCommon;

		[TestFixtureSetUp]
		public void FixtureSetup() {
			mCommon = new CommonTestSetup();
			mCommon.FixtureSetup();
		}

		[SetUp]
		public void Setup() {
			mCommon.Setup();

			mArtworkProvider = new Mock<IArtworkProvider>();
			mTextInfoProvider = new Mock<ITextInfoProvider>();
			mAssociationProvider = new Mock<ITagAssociationProvider>();
		}

		[TearDown]
		public void Teardown() {
			mCommon.Teardown();
		}

		protected override IAlbumProvider CreateSut() {
			return( new AlbumProvider( mCommon.DatabaseFactory.Object, mArtworkProvider.Object, mTextInfoProvider.Object, mAssociationProvider.Object ));
		}
	}
}