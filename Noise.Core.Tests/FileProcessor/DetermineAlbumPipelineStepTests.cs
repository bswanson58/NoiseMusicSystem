using System.Collections.Generic;
using Caliburn.Micro;
using FluentAssertions;
using Moq;
using Noise.Core.Logging;
using NUnit.Framework;
using Noise.Core.DataProviders;
using Noise.Core.Database;
using Noise.Core.FileProcessor;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using ReusableBits.TestSupport.Mocking;

namespace Noise.Core.Tests.FileProcessor {
	[TestFixture]
	public class DetermineAlbumPipelineStepTests {
		internal class AlbumProvider : IMetadataInfoProvider {
			private readonly string	mAlbumName;

			public AlbumProvider( string albumName ) {
				mAlbumName = albumName;
			}

			public string Artist {
				get { throw new System.NotImplementedException(); }
			}

			public virtual string Album {
				get { return ( mAlbumName ); }
			}

			public string TrackName {
				get { throw new System.NotImplementedException(); }
			}

			public string VolumeName {
				get { throw new System.NotImplementedException(); }
			}

			public void AddAvailableMetaData( DbArtist artist, DbAlbum album, DbTrack track ) { }
		}

		internal class ExceptionAlbumProvider : AlbumProvider {
			public ExceptionAlbumProvider() : 
				base( string.Empty ) { }

			public override string Album {
				get { throw new System.NotImplementedException(); }
			}
		}

		internal class TestableDetermineAlbumPipelineStep : Testable<DetermineAlbumPipelineStep> { }

		private ILogLibraryClassification	mLog;
		private List<DbArtist>				mArtistCacheList;
		private List<DbAlbum>				mAlbumCacheList;
		private DatabaseChangeSummary		mSummary;

		[SetUp]
		public void Setup() {
			mArtistCacheList = new List<DbArtist>();
			mAlbumCacheList = new List<DbAlbum>();
			mSummary = new DatabaseChangeSummary();

			mLog = new Mock<ILogLibraryClassification>().Object;
		}

		[Test]
		public void CanDetermineAlbumName() {
			const string albumName = "the album's name";
			var	testable = new TestableDetermineAlbumPipelineStep();
			var context = new PipelineContext( new DatabaseCache<DbArtist>( mArtistCacheList ), new DatabaseCache<DbAlbum>( mAlbumCacheList ), null, mSummary, mLog ) {
				Artist = new DbArtist(), MetaDataProviders = new List<IMetadataInfoProvider> { new AlbumProvider( albumName ) }
			};

			var sut = testable.ClassUnderTest;

			sut.Should().NotBeNull();
			sut.ProcessStep( context );

			context.Album.Should().NotBeNull();
		}

		[Test]
		public void RetrievesAlbumFromCache() {
			const string albumName = "the artist's name";
			var artist = new DbArtist();
			var album = new DbAlbum { Name = albumName, Artist = artist.DbId };
			mAlbumCacheList.Add( album );
			var	testable = new TestableDetermineAlbumPipelineStep();
			var context = new PipelineContext( new DatabaseCache<DbArtist>( mArtistCacheList ), new DatabaseCache<DbAlbum>( mAlbumCacheList ), null, mSummary, mLog ) {
				Artist = artist, MetaDataProviders = new List<IMetadataInfoProvider> { new AlbumProvider( albumName ) }
			};

			var sut = testable.ClassUnderTest;

			sut.ProcessStep( context );

			context.Album.Should().Be( album );
		}

		[Test]
		public void AlbumNotInCacheCreatesNewAlbum() {
			const string albumName = "the album's name";
			var artist = new DbArtist();
			var album = new DbAlbum { Name = "another album", Artist = artist.DbId };
			mAlbumCacheList.Add( album );
			var	testable = new TestableDetermineAlbumPipelineStep();
			var context = new PipelineContext( new DatabaseCache<DbArtist>( mArtistCacheList ), new DatabaseCache<DbAlbum>( mAlbumCacheList ), null, mSummary, mLog ) {
				Artist = artist, MetaDataProviders = new List<IMetadataInfoProvider> { new AlbumProvider( albumName ) }
			};

			var sut = testable.ClassUnderTest;

			sut.ProcessStep( context );

			context.Album.Should().NotBe( album );
		}

		[Test]
		public void AlbumNotInCacheAddsAlbum() {
			const string albumName = "the album's name";
			var artist = new DbArtist();
			var album = new DbAlbum { Name = "another album" };
			mAlbumCacheList.Add( album );
			var	testable = new TestableDetermineAlbumPipelineStep();
			var context = new PipelineContext( new DatabaseCache<DbArtist>( mArtistCacheList ), new DatabaseCache<DbAlbum>( mAlbumCacheList ), null, mSummary, mLog ) {
				Artist = artist, MetaDataProviders = new List<IMetadataInfoProvider> { new AlbumProvider( albumName ) }
			};

			var sut = testable.ClassUnderTest;

			sut.ProcessStep( context );

			testable.Mock<IAlbumProvider>().Verify( m => m.AddAlbum( It.IsAny<DbAlbum>() ), Times.Once());
		}

		[Test]
		public void AlbumNotInCacheAddsToCache() {
			const string albumName = "the album's name";
			var artist = new DbArtist();
			var album = new DbAlbum { Name = "another album" };
			mAlbumCacheList.Add( album );
			var albumCache = new DatabaseCache<DbAlbum>( mAlbumCacheList );
			var	testable = new TestableDetermineAlbumPipelineStep();
			var context = new PipelineContext( new DatabaseCache<DbArtist>( mArtistCacheList ), albumCache, null, mSummary, mLog ) {
				Artist = artist, MetaDataProviders = new List<IMetadataInfoProvider> { new AlbumProvider( albumName ) }
			};

			var sut = testable.ClassUnderTest;

			sut.ProcessStep( context );

			var cachedAlbum = albumCache.Find( a => a.Name == albumName );
			cachedAlbum.Should().NotBeNull();
		}

		[Test]
		public void AlbumNotInCacheAdjustsSummary() {
			const string albumName = "the album's name";
			var artist = new DbArtist();
			var album = new DbAlbum { Name = "another album" };
			mAlbumCacheList.Add( album );
			var	testable = new TestableDetermineAlbumPipelineStep();
			var summary = new DatabaseChangeSummary();
			var context = new PipelineContext( new DatabaseCache<DbArtist>( mArtistCacheList ), new DatabaseCache<DbAlbum>( mAlbumCacheList ), null, summary, mLog ) {
				Artist = artist, MetaDataProviders = new List<IMetadataInfoProvider> { new AlbumProvider( albumName ) }
			};

			var sut = testable.ClassUnderTest;

			sut.ProcessStep( context );

			summary.AlbumsAdded.Should().Be( 1 );
		}

		[Test]
		public void AlbumNotInCacheShouldPublishEvent() {
			const string albumName = "the album's name";
			var artist = new DbArtist();
			var album = new DbAlbum { Name = "another album" };
			mAlbumCacheList.Add( album );
			var	testable = new TestableDetermineAlbumPipelineStep();
			var context = new PipelineContext( new DatabaseCache<DbArtist>( mArtistCacheList ), new DatabaseCache<DbAlbum>( mAlbumCacheList ), null, mSummary, mLog ) {
				Artist = artist, MetaDataProviders = new List<IMetadataInfoProvider> { new AlbumProvider( albumName ) }
			};

			var sut = testable.ClassUnderTest;

			sut.ProcessStep( context );

			testable.Mock<IEventAggregator>().Verify( m => m.PublishOnUIThread( It.IsAny<object>() ), Times.Once());
		}

		[Test]
		public void AlbumInCacheShouldNotPublishEvent() {
			const string albumName = "the album's name";
			var artist = new DbArtist();
			var album = new DbAlbum { Name = albumName, Artist = artist.DbId };
			mAlbumCacheList.Add( album );
			var	testable = new TestableDetermineAlbumPipelineStep();
			var context = new PipelineContext( new DatabaseCache<DbArtist>( mArtistCacheList ), new DatabaseCache<DbAlbum>( mAlbumCacheList ), null, mSummary, mLog ) {
				Artist = artist, MetaDataProviders = new List<IMetadataInfoProvider> { new AlbumProvider( albumName ) }
			};

			var sut = testable.ClassUnderTest;

			sut.ProcessStep( context );

			testable.Mock<IEventAggregator>().Verify( m => m.PublishOnUIThread( It.IsAny<object>() ), Times.Never());
		}

		[Test]
		public void UndetermineAlbumShouldNotCreateAlbum() {
			var	testable = new TestableDetermineAlbumPipelineStep();
			var artist = new DbArtist();
			var context = new PipelineContext( new DatabaseCache<DbArtist>( mArtistCacheList ), new DatabaseCache<DbAlbum>( mAlbumCacheList ), null, mSummary, mLog ) {
				Artist = artist, MetaDataProviders = new List<IMetadataInfoProvider> { new AlbumProvider( string.Empty ) }
			};

			var sut = testable.ClassUnderTest;

			sut.ProcessStep( context );

			context.Album.Should().BeNull();
		}

		[Test]
		public void MissingArtistShouldNotDetermineAlbum() {
			var	testable = new TestableDetermineAlbumPipelineStep();
			var context = new PipelineContext( new DatabaseCache<DbArtist>( mArtistCacheList ), new DatabaseCache<DbAlbum>( mAlbumCacheList ), null, mSummary, mLog ) {
				MetaDataProviders = new List<IMetadataInfoProvider> { new ExceptionAlbumProvider() }
			};

			var sut = testable.ClassUnderTest;

			sut.ProcessStep( context );
			context.Album.Should().BeNull();
		}
	}
}
