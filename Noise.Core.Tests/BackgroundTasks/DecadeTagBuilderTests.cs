using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Noise.Core.BackgroundTasks;
using Noise.Core.Support;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Tests.BackgroundTasks {
	internal class DecadeTagBuilderTests {
		public class TestDecadeTagBuilder : DecadeTagBuilder {
		public TestDecadeTagBuilder( ILifecycleManager lifecycleManager, ITagAssociationProvider tagAssociationProvider, ITimestampProvider timestampProvider,
									 IArtistProvider artistProvider, IAlbumProvider albumProvider, ITagManager tagManager ) :
			base( lifecycleManager, tagAssociationProvider, timestampProvider, artistProvider, albumProvider, tagManager ) { }
		}

		[TestFixture]
		public class ExecuteTask {
			private Mock<IArtistProvider>			mArtistProvider;
			private Mock<IAlbumProvider>			mAlbumProvider;
			private Mock<ILifecycleManager>			mLifecycleManager;
			private Mock<ITagAssociationProvider>	mTagAssociationProvider;
			private Mock<ITagManager>				mTagManager;
			private Mock<ITimestampProvider>		mTimestampProvider;

			protected void CreateMocks() {
				mArtistProvider = new Mock<IArtistProvider>();
				mAlbumProvider = new Mock<IAlbumProvider>();
				mLifecycleManager = new Mock<ILifecycleManager>();
				mTagAssociationProvider = new Mock<ITagAssociationProvider>();
				mTagManager = new Mock<ITagManager>();
				mTimestampProvider = new Mock<ITimestampProvider>();

				var	decadeTagList = new List<DbDecadeTag> { new DbDecadeTag( "One" ) { StartYear = 10, EndYear = 19 },
															new DbDecadeTag( "Two" ) { StartYear = 20, EndYear = 29 },
															new DbDecadeTag( "3rd" ) { StartYear = 30, EndYear = 39 }};

			}

			protected DecadeTagBuilder CreateSut() {
				return( new DecadeTagBuilder( mLifecycleManager.Object, mTagAssociationProvider.Object, mTimestampProvider.Object,
											  mArtistProvider.Object, mAlbumProvider.Object, mTagManager.Object ));
			}

			[Test]
			public void RemovesAllPreviousTags() {
				CreateMocks();

				var nowTicks = DateTime.Now.Ticks - 10; // in the past.
				var databaseShell = new Mock<IDatabaseShell>();

				mTimestampProvider.Setup( x => x.GetTimestamp( It.IsAny<string>())).Returns( nowTicks );

				var artist = new DbArtist();
				var artists = new List<DbArtist> { artist };
				mArtistProvider.Setup( m => m.GetArtistList()).Returns( new DataProviderList<DbArtist>( databaseShell.Object, artists ));
				mArtistProvider.Setup( m => m.GetArtist( It.IsAny<long>())).Returns( artist );

				var albumList = new DataProviderList<DbAlbum>( databaseShell.Object, new List<DbAlbum>());
				mAlbumProvider.Setup( m => m.GetAlbumList( It.IsAny<long>())).Returns( albumList );

				var association = new DbTagAssociation( eTagGroup.Decade, 1, 1, 1 );
				var associationList = new List<DbTagAssociation> { association };
				var providerList = new DataProviderList<DbTagAssociation>( databaseShell.Object, associationList );

				mTagAssociationProvider.Setup( p => p.GetArtistTagList( It.IsAny<long>(), It.IsAny<eTagGroup>())).Returns( providerList );
				mTagAssociationProvider.Setup( p => p.RemoveAssociation( It.Is<long>( l => l == association.DbId ))).Verifiable();

				var sut = CreateSut();
				sut.Initialize();
				sut.ExecuteTask();
				mTagAssociationProvider.Verify();
			}

			[Test]
			public void AddsNewTag() {
				
			}

			[Test]
			public void DoesNotAddUnTaggedYear() {
				
			}

			[Test]
			public void DoesNotRescanUnchangedArtist() {
				
			}
		}
	}
}
