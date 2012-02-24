using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Noise.Core.BackgroundTasks;
using Noise.Core.Support;
using Noise.Infrastructure;
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

			private void CreateMocks() {
				mArtistProvider = new Mock<IArtistProvider>();
				mAlbumProvider = new Mock<IAlbumProvider>();
				mLifecycleManager = new Mock<ILifecycleManager>();
				mTagAssociationProvider = new Mock<ITagAssociationProvider>();
				mTagManager = new Mock<ITagManager>();
				mTimestampProvider = new Mock<ITimestampProvider>();
			}

			private DecadeTagBuilder CreateSut() {
				return( new DecadeTagBuilder( mLifecycleManager.Object, mTagAssociationProvider.Object, mTimestampProvider.Object,
											  mArtistProvider.Object, mAlbumProvider.Object, mTagManager.Object ));
			}

			private void SetupTimestampProvider( long withTime ) {
				mTimestampProvider.Setup( x => x.GetTimestamp( It.IsAny<string>())).Returns( withTime );
			}

			private void SetupArtistProvider( DbArtist artist ) {
				var artists = new List<DbArtist> { artist };
				var providerList = new Mock<IDataProviderList<DbArtist>>();
 				providerList.Setup( m => m.List ).Returns( artists );
				mArtistProvider.Setup( m => m.GetArtistList()).Returns( providerList.Object );
				mArtistProvider.Setup( m => m.GetArtist( It.IsAny<long>())).Returns( artist );
			}

			private void SetupAlbumProvider( IEnumerable<DbAlbum> albumList ) {
				var provider = new Mock<IDataProviderList<DbAlbum>>();
 				provider.Setup( m => m.List ).Returns( albumList );
				mAlbumProvider.Setup( m => m.GetAlbumList( It.IsAny<long>())).Returns( provider.Object );
			}

			private void SetupTagProvider( IEnumerable<DbTagAssociation> tagList ) {
				var provider = new Mock<IDataProviderList<DbTagAssociation>>();
 				provider.Setup( m => m.List ).Returns( tagList );
				mTagAssociationProvider.Setup( p => p.GetArtistTagList( It.IsAny<long>(), It.IsAny<eTagGroup>())).Returns( provider.Object );
			} 
			
			[Test]
			public void RemovesAllPreviousTags() {
				CreateMocks();

				SetupTimestampProvider( DateTime.Now.Ticks - 10 ); // in the past.

				var artist = new DbArtist();
				SetupArtistProvider( artist );

				SetupAlbumProvider( new List<DbAlbum>());

				var association = new DbTagAssociation( eTagGroup.Decade, 1, 1, 1 );
				SetupTagProvider( new List<DbTagAssociation> { association });

				mTagAssociationProvider.Setup( p => p.RemoveAssociation( It.Is<long>( l => l == association.DbId ))).Verifiable();

				var sut = CreateSut();
				sut.Initialize();
				sut.ExecuteTask();

				mTagAssociationProvider.Verify();
			}

			[Test]
			public void AddsNewTag() {
				CreateMocks();

				SetupTimestampProvider( DateTime.Now.Ticks - 10 ); // in the past.

				var artist = new DbArtist();
				SetupArtistProvider( artist );

				var album = new DbAlbum { PublishedYear = 23 };
				SetupAlbumProvider( new List<DbAlbum>{ album });

				SetupTagProvider( new List<DbTagAssociation>());

				var decadeOne = new DbDecadeTag( "one" ) { StartYear = 10, EndYear = 19 };
				var decadeTwo = new DbDecadeTag( "Two" ) { StartYear = 20, EndYear = 29 };
				var decadeThr = new DbDecadeTag( "3rd" ) { StartYear = 30, EndYear = 39 };
				var	decadeTagList = new List<DbDecadeTag> { decadeTwo, decadeThr, decadeOne };
				mTagManager.Setup( m => m.DecadeTagList ).Returns( decadeTagList );

				mTagAssociationProvider.Setup( m => m.AddAssociation( It.Is<DbTagAssociation>( item => item.TagId == decadeTwo.DbId ))).Verifiable();

				var sut = CreateSut();
				sut.Initialize();
				sut.ExecuteTask();

				mTagAssociationProvider.Verify();
			}

			[Test]
			public void DoesNotAddUnTaggedYear() {
				CreateMocks();

				SetupTimestampProvider( DateTime.Now.Ticks - 10 ); // in the past.

				var artist = new DbArtist();
				SetupArtistProvider( artist );

				var album = new DbAlbum { PublishedYear = Constants.cUnknownYear };
				SetupAlbumProvider( new List<DbAlbum>{ album });

				SetupTagProvider( new List<DbTagAssociation>());

				var decadeOne = new DbDecadeTag( "one" ) { StartYear = 10, EndYear = 99 };
				var	decadeTagList = new List<DbDecadeTag> { decadeOne };
				mTagManager.Setup( m => m.DecadeTagList ).Returns( decadeTagList );

				var sut = CreateSut();
				sut.Initialize();
				sut.ExecuteTask();

				mTagAssociationProvider.Verify( m => m.AddAssociation( It.IsAny<DbTagAssociation>()), Times.Never());
			}

			[Test]
			public void DoesNotRescanUnchangedArtist() {
				CreateMocks();

				SetupTimestampProvider(( DateTime.Now + new TimeSpan( 1, 0 , 0 )).Ticks ); // one hour in the future

				var artist = new DbArtist();
				SetupArtistProvider( artist );

				SetupTagProvider( new List<DbTagAssociation>());

				var sut = CreateSut();
				sut.Initialize();
				sut.ExecuteTask();

				mAlbumProvider.Verify( m => m.GetAlbumList( It.IsAny<long>()), Times.Never());
			}
		}
	}
}
