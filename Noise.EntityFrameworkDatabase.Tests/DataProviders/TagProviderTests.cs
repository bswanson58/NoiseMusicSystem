﻿using NUnit.Framework;
using Noise.BaseDatabase.Tests.DataProviders;
using Noise.EntityFrameworkDatabase.DataProviders;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.Tests.DataProviders {
	[TestFixture]
	public class TagProviderTests : BaseTagProviderTests {
		private readonly ProviderTestSetup	mTestSetup;

		public TagProviderTests() {
			mTestSetup = new ProviderTestSetup();
		}

		[SetUp]
		public void Setup() {
			mTestSetup.Setup();
		}

		protected override ITagProvider CreateSut() {
			return( new TagProvider( mTestSetup.ContextProvider ));
		}
	}
}
