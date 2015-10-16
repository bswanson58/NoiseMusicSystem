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
	internal class ArtistProvider : IMetaDataProvider {
		private readonly string	mArtistName;

		public ArtistProvider( string artistName ) {
			mArtistName = artistName;
		}

		public string Artist {
			get { return( mArtistName ); }
		}

		public string Album {
			get { throw new System.NotImplementedException(); }
		}

		public string TrackName {
			get { throw new System.NotImplementedException(); }
		}

		public string VolumeName {
			get { throw new System.NotImplementedException(); }
		}

		public void AddAvailableMetaData( DbArtist artist, DbAlbum album, DbTrack track ) { }
	}

	internal class TestableDetermineArtistPipelineStep : Testable<DetermineArtistPipelineStep> { }

	[TestFixture]
	public class DetermineArtistPipelineStepTests {
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
		public void CanDetermineArtistName() {
			const string artistName = "the artist's name";
			var	testable = new TestableDetermineArtistPipelineStep();
			var context = new PipelineContext( new DatabaseCache<DbArtist>( mArtistCacheList ), new DatabaseCache<DbAlbum>( mAlbumCacheList ), null, mSummary, mLog ) {
													MetaDataProviders = new List<IMetaDataProvider> { new ArtistProvider( artistName ) }};

			var sut = testable.ClassUnderTest;

			sut.Should().NotBeNull();
			sut.ProcessStep( context );

			context.Artist.Should().NotBeNull();
		}

		[Test]
		public void RetrievesArtistFromCache() {
			const string artistName = "the artist's name";
			var artist = new DbArtist { Name = artistName };
			mArtistCacheList.Add( artist );
			var	testable = new TestableDetermineArtistPipelineStep();
			var context = new PipelineContext( new DatabaseCache<DbArtist>( mArtistCacheList ), new DatabaseCache<DbAlbum>( mAlbumCacheList ), null, mSummary, mLog ) {
													MetaDataProviders = new List<IMetaDataProvider> { new ArtistProvider( artistName ) }};

			var sut = testable.ClassUnderTest;

			sut.ProcessStep( context );

			context.Artist.Should().Be( artist );
		}

		[Test]
		public void ArtistNotInCacheCreatesNewArtist() {
			const string artistName = "the artist's name";
			var artist = new DbArtist { Name = "another artist" };
			mArtistCacheList.Add( artist );
			var	testable = new TestableDetermineArtistPipelineStep();
			var context = new PipelineContext( new DatabaseCache<DbArtist>( mArtistCacheList ), new DatabaseCache<DbAlbum>( mAlbumCacheList ), null, mSummary, mLog ) {
													MetaDataProviders = new List<IMetaDataProvider> { new ArtistProvider( artistName ) }};

			var sut = testable.ClassUnderTest;

			sut.ProcessStep( context );

			context.Artist.Should().NotBe( artist );
		}

		[Test]
		public void ArtistNotInCacheAddsArtist() {
			const string artistName = "the artist's name";
			var artist = new DbArtist { Name = "another artist" };
			mArtistCacheList.Add( artist );
			var	testable = new TestableDetermineArtistPipelineStep();
			var context = new PipelineContext( new DatabaseCache<DbArtist>( mArtistCacheList ), new DatabaseCache<DbAlbum>( mAlbumCacheList ), null, mSummary, mLog ) {
													MetaDataProviders = new List<IMetaDataProvider> { new ArtistProvider( artistName ) }};

			var sut = testable.ClassUnderTest;

			sut.ProcessStep( context );

			testable.Mock<IArtistProvider>().Verify( m => m.AddArtist( It.IsAny<DbArtist>()), Times.Once());
		}

		[Test]
		public void ArtistNotInCacheAddsToCache() {
			const string artistName = "the artist's name";
			var artist = new DbArtist { Name = "another artist" };
			mArtistCacheList.Add( artist );
			var artistCache = new DatabaseCache<DbArtist>( mArtistCacheList );
			var	testable = new TestableDetermineArtistPipelineStep();
			var context = new PipelineContext( artistCache, new DatabaseCache<DbAlbum>( mAlbumCacheList ), null, mSummary, mLog ) {
													MetaDataProviders = new List<IMetaDataProvider> { new ArtistProvider( artistName ) }};

			var sut = testable.ClassUnderTest;

			sut.ProcessStep( context );

			var cacheArtist = artistCache.Find( a => a.Name == artistName );
			cacheArtist.Should().NotBeNull();
		}

		[Test]
		public void ArtistNotInCacheAdjustsSummary() {
			const string artistName = "the artist's name";
			var artist = new DbArtist { Name = "another artist" };
			mArtistCacheList.Add( artist );
			var artistCache = new DatabaseCache<DbArtist>( mArtistCacheList );
			var summary = new DatabaseChangeSummary();
			var	testable = new TestableDetermineArtistPipelineStep();
			var context = new PipelineContext( artistCache, new DatabaseCache<DbAlbum>( mAlbumCacheList ), null, summary, mLog ) {
													MetaDataProviders = new List<IMetaDataProvider> { new ArtistProvider( artistName ) }};

			var sut = testable.ClassUnderTest;

			sut.ProcessStep( context );

			summary.ArtistsAdded.Should().Be( 1 );
		}

		[Test]
		public void ArtistNotInCacheShouldPublishEvent() {
			const string artistName = "the artist's name";
			var artist = new DbArtist { Name = "another artist" };
			mArtistCacheList.Add( artist );
			var artistCache = new DatabaseCache<DbArtist>( mArtistCacheList );
			var summary = new DatabaseChangeSummary();
			var	testable = new TestableDetermineArtistPipelineStep();
			var context = new PipelineContext( artistCache, new DatabaseCache<DbAlbum>( mAlbumCacheList ), null, summary, mLog ) {
													MetaDataProviders = new List<IMetaDataProvider> { new ArtistProvider( artistName ) }};

			var sut = testable.ClassUnderTest;

			sut.ProcessStep( context );

			testable.Mock<IEventAggregator>().Verify( m => m.Publish( It.IsAny<object>()), Times.Once());
		}

		[Test]
		public void ArtistInCacheShouldNotPublishEvent() {
			const string artistName = "the artist's name";
			var artist = new DbArtist { Name = artistName };
			mArtistCacheList.Add( artist );
			var artistCache = new DatabaseCache<DbArtist>( mArtistCacheList );
			var summary = new DatabaseChangeSummary();
			var	testable = new TestableDetermineArtistPipelineStep();
			var context = new PipelineContext( artistCache, new DatabaseCache<DbAlbum>( mAlbumCacheList ), null, summary, mLog ) {
													MetaDataProviders = new List<IMetaDataProvider> { new ArtistProvider( artistName ) }};

			var sut = testable.ClassUnderTest;

			sut.ProcessStep( context );

			testable.Mock<IEventAggregator>().Verify( m => m.Publish( It.IsAny<object>()), Times.Never());
		}

		[Test]
		public void UndetermineArtistShouldNotCreateArtist() {
			var	testable = new TestableDetermineArtistPipelineStep();
			var context = new PipelineContext( new DatabaseCache<DbArtist>( mArtistCacheList ), new DatabaseCache<DbAlbum>( mAlbumCacheList ), null, mSummary, mLog ) {
													MetaDataProviders = new List<IMetaDataProvider> { new ArtistProvider( string.Empty ) }};

			var sut = testable.ClassUnderTest;

			sut.Should().NotBeNull();
			sut.ProcessStep( context );

			context.Artist.Should().BeNull();
		}
	}
}
