using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Practices.Prism.Events;
using Moq;
using NUnit.Framework;
using Noise.AppSupport;
using Noise.Core.Database;
using Noise.EloqueraDatabase;
using Noise.EloqueraDatabase.BlobStore;
using Noise.EloqueraDatabase.Database;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.IntegrationTests.Database {
	[TestFixture]
	public class ArtistProviderTests {
		private Mock<ILog>							mDummyLog;
		private IIoc								mIocProvider;
		private DatabaseConfiguration				mDatabaseConfiguration;
		private IBlobStorageResolver				mBlobResolver;
		private IDatabaseFactory					mDatabaseFactory;
		private IDatabaseManager					mDatabaseManager;
		private Mock<IEventAggregator>				mEventAggregator;
		private Mock<IArtworkProvider>				mArtworkProvider;
		private Mock<ITextInfoProvider>				mTextInfoProvider;
		private Mock<ITagAssociationProvider>		mAssociationProvider;
		private Mock<IAssociatedItemListProvider>	mListProvider;

		[SetUp]
		public void Setup() {
			mDummyLog = new Mock<ILog>();
			NoiseLogger.Current = mDummyLog.Object;
				
			mEventAggregator = new Mock<IEventAggregator> { DefaultValue = DefaultValue.Mock };

			mArtworkProvider = new Mock<IArtworkProvider>();
			mTextInfoProvider = new Mock<ITextInfoProvider>();
			mAssociationProvider = new Mock<ITagAssociationProvider>();
			mListProvider = new Mock<IAssociatedItemListProvider>();

			mDatabaseConfiguration = new DatabaseConfiguration { DatabaseName = "Integration Test Database" };

			mIocProvider = new IocProvider();
			mBlobResolver = new BlobStorageResolver();
			mDatabaseFactory = new EloqueraDatabaseFactory( mBlobResolver, mEventAggregator.Object, mIocProvider, mDatabaseConfiguration );
			mDatabaseManager = new DatabaseManager( mDatabaseFactory );

			if( mDatabaseManager.Initialize()) {
				using( var database = mDatabaseManager.CreateDatabase()) {
					database.Database.DeleteDatabase();
				}

				using( var database = mDatabaseManager.CreateDatabase()) {
					database.Database.OpenWithCreateDatabase();
				}
			}
		}

		private ArtistProvider CreateSut() {
			return( new ArtistProvider( mDatabaseManager, mArtworkProvider.Object, mTextInfoProvider.Object, mAssociationProvider.Object, mListProvider.Object ));
		}

		[Test]
		public void CanAddArtist() {
			var artist = new DbArtist { Name = "test name" };

			var sut = CreateSut();
			sut.AddArtist( artist );
		}

		[Test]
		public void CanRetrieveArtist() {
			var	artist = new DbArtist { Name = "find me" };

			var sut = CreateSut();
			sut.AddArtist( artist );

			var returnedArtist = sut.GetArtist( artist.DbId );

			returnedArtist.ShouldHave().AllProperties().EqualTo( artist );
		}

		[Test]
		public void CannotAddExistingArtist() {
			var artist = new DbArtist { Name = "an artist" };

			var sut = CreateSut();
			sut.AddArtist( artist );
			sut.AddArtist( artist );

			using( var artistList = sut.GetArtistList()) {
				artistList.List.Should().HaveCount( 1 );
			}
		}

		[Test]
		[ExpectedException( typeof( ArgumentNullException ))]
		public void CannotAddNullArtist() {
			var sut = CreateSut();

			sut.AddArtist( null );
		}

		[Test]
		public void CanDeleteArtist() {
			var artist = new DbArtist { Name = "delete me if you can" };

			var sut = CreateSut();

			sut.AddArtist( artist );
			sut.DeleteArtist( artist );

			var deletedArtist = sut.GetArtist( artist.DbId );

			Assert.IsNull( deletedArtist );
		}

		[Test]
		public void CanGetArtistList() {
			var artist1 = new DbArtist { Name = "artist 1" };
			var artist2 = new DbArtist { Name = "artist 2" };

			var sut = CreateSut();

			sut.AddArtist( artist1 );
			sut.AddArtist( artist2 );

			using( var artistList = sut.GetArtistList()) {
				artistList.List.Should().HaveCount( 2 );
			}
		}

		[Test]
		[Ignore( "Test not implemented" )]
		public void CanGetArtistListMatchingFilter() {
			
		}

		[Test]
		public void CanGetArtistForAlbum() {
			var artist = new DbArtist();
			var album = new DbAlbum { Artist = artist.DbId };
			
			var sut = CreateSut();

			sut.AddArtist( artist );

			var returnedArtist = sut.GetArtistForAlbum( album );

			returnedArtist.ShouldHave().AllProperties().EqualTo( artist );
		}

		[Test]
		public void CanGetChangedArtists() {
			var artist1 = new DbArtist();
			var artist2 = new DbArtist();

			System.Threading.Thread.Sleep( 100 );
			var now = DateTime.Now.Ticks;
			System.Threading.Thread.Sleep( 100 );
			artist2.UpdateLastChange();

			var sut = CreateSut();

			sut.AddArtist( artist1 );
			sut.AddArtist( artist2 );

			using( var changedArtists = sut.GetChangedArtists( now )) {
				changedArtists.List.Should().HaveCount( 1 );
			}
		}

		[Test]
		public void CanGetFavoriteArtists() {
			var artist1 = new DbArtist();
			var artist2 = new DbArtist { IsFavorite = true };

			var sut = CreateSut();

			sut.AddArtist( artist1 );
			sut.AddArtist( artist2 );

			using( var changedArtists = sut.GetFavoriteArtists()) {
				changedArtists.List.Should().HaveCount( 1 );
			}
		}

		[Test]
		public void CanUpdateArtist() {
			var artist = new DbArtist { Name = "old name" };

			var sut = CreateSut();

			sut.AddArtist( artist );
			using( var updater = sut.GetArtistForUpdate( artist.DbId )) {
				updater.Item.Name = "new name";

				updater.Update();
			}

			var changedArtist = sut.GetArtist( artist.DbId );

			changedArtist.Name.Should().NotBe( artist.Name );
		}

		[Test]
		public void CanUpdateArtistLastChanged() {
			var artist = new DbArtist();

			var sut = CreateSut();

			sut.AddArtist( artist );

			System.Threading.Thread.Sleep( 100 );
			sut.UpdateArtistLastChanged( artist.DbId );

			var updatedArtist = sut.GetArtist( artist.DbId );

			updatedArtist.LastChangeTicks.Should().BeGreaterThan( artist.LastChangeTicks );
		}

		[Test]
		public void CanGetArtistCategories() {
			var artist = new DbArtist();

			var databaseShell = new Mock<IDatabaseShell>();
			var tags = new List<DbTagAssociation> { new DbTagAssociation( eTagGroup.User, 1, artist.DbId, 1 ),
													new DbTagAssociation( eTagGroup.User, 2, artist.DbId, 2 )};
			var tagList = new DataProviderList<DbTagAssociation>( databaseShell.Object, tags );
			mAssociationProvider.Setup( m => m.GetArtistTagList( It.IsAny<long>(), It.Is<eTagGroup>( p => p == eTagGroup.User ))).Returns( tagList );

			var sut = CreateSut();
			
			using( var categoryList = sut.GetArtistCategories( artist.DbId )) {
				categoryList.List.Should().HaveCount( 2 );
			}
		}

		[Test]
		public void CanGetArtistSupportInfo() {
			var artist = new DbArtist();

			var dbTextInfo = new DbTextInfo( artist.DbId, ContentType.Biography );
			var textInfo = new TextInfo( dbTextInfo ) { Text = "some text info" };
			mTextInfoProvider.Setup( m => m.GetArtistTextInfo( It.Is<long>( p => p == artist.DbId), 
															   It.Is<ContentType>( p => p == ContentType.Biography ))).Returns( textInfo );

			var dbArtwork = new DbArtwork( artist.DbId, ContentType.ArtistPrimaryImage );
			var artwork = new Artwork( dbArtwork );
			mArtworkProvider.Setup( m => m.GetArtistArtwork( It.Is<long>( p => p == artist.DbId ),
															 It.Is<ContentType>( p => p == ContentType.ArtistPrimaryImage ))).Returns( artwork );

			var similarArtists = new DbAssociatedItemList( artist.DbId, ContentType.SimilarArtists );
			mListProvider.Setup( m => m.GetAssociatedItems( It.Is<long>( p => p == artist.DbId ),
															It.Is<ContentType>( p => p == ContentType.SimilarArtists ))).Returns( similarArtists );

			var topAlbums = new DbAssociatedItemList( artist.DbId, ContentType.TopAlbums );
			mListProvider.Setup( m => m.GetAssociatedItems( It.Is<long>( p => p == artist.DbId ),
															It.Is<ContentType>( p => p == ContentType.TopAlbums ))).Returns( topAlbums );

			var bandMembers = new DbAssociatedItemList( artist.DbId, ContentType.BandMembers );
			mListProvider.Setup( m => m.GetAssociatedItems( It.Is<long>( p => p == artist.DbId ),
															It.Is<ContentType>( p => p == ContentType.BandMembers ))).Returns( bandMembers );

			var sut = CreateSut();

			var artistInfo = sut.GetArtistSupportInfo( artist.DbId );

			artistInfo.ArtistImage.ShouldHave().AllProperties().EqualTo( artwork );
			artistInfo.BandMembers.ShouldHave().AllProperties().EqualTo( bandMembers );
			artistInfo.Biography.ShouldHave().AllProperties().EqualTo( textInfo );
			artistInfo.SimilarArtist.ShouldHave().AllProperties().EqualTo( similarArtists );
			artistInfo.TopAlbums.ShouldHave().AllProperties().EqualTo( topAlbums );
		}

		[Test]
		public void CanGetItemCount() {
			var	artist1 = new DbArtist();
			var artist2 = new DbArtist();
			var artist3 = new DbArtist();

			var sut = CreateSut();

			sut.AddArtist( artist1 );
			sut.AddArtist( artist2 );
			sut.AddArtist( artist3 );

			var itemCount = sut.GetItemCount();

			itemCount.Should().Be( 3 );
		}
	}
}
