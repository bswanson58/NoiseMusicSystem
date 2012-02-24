using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Noise.EloqueraDatabase.DataProviders;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.IntegrationTests.Database {
	[TestFixture]
	public class LyricProviderTests : BaseDatabaseProviderTests {

		private ILyricProvider CreateSut() {
			return( new LyricProvider( mDatabaseManager ));	
		}

		[Test]
		public void CanAddLyric() {
			var lyric = new DbLyric( 1, 2, "Track Name" );

			var sut = CreateSut();

			sut.AddLyric( lyric );
		}

		[Test]
		public void CanGetLyricsForArtist() {
			var artist = new DbArtist();
			var lyric1 = new DbLyric( artist.DbId, 1, "" );
			var lyric2 = new DbLyric( artist.DbId + 1, 3, "" );

			var sut = CreateSut();
			sut.AddLyric( lyric1 );
			sut.AddLyric( lyric2 );

			using( var lyricList = sut.GetLyricsForArtist( artist )) {
				Assert.IsNotNull( lyricList.List );

				lyricList.List.Should().HaveCount( 1 );
				
				var retrievedLyric = lyricList.List.First();
				retrievedLyric.ArtistId.Should().Be( artist.DbId );
			}
		}

		[Test]
		public void CanGetLyricsForTrack() {
			var artist = new DbArtist();
			var track = new DbTrack { Name = "track name" };
			var lyric1 = new DbLyric( artist.DbId, track.DbId, track.Name );

			var sut = CreateSut();
			sut.AddLyric( lyric1 );

			using( var lyricsList = sut.GetPossibleLyrics( artist, track )) {
				Assert.IsNotNull( lyricsList.List );

				lyricsList.List.Should().HaveCount( 1 );
			}
		}

		[Test]
		public void CanGetLyricsForSameArtistSongName() {
			var artist = new DbArtist();
			var track = new DbTrack { Name = "track name" };
			var lyrics1 = new DbLyric( artist.DbId, track.DbId + 1, track.Name );
			var lyrics2 = new DbLyric( artist.DbId + 1, track.DbId + 2, "another song name" );

			var sut = CreateSut();
			sut.AddLyric( lyrics1 );
			sut.AddLyric( lyrics2 );

			using( var lyricsList = sut.GetPossibleLyrics( artist, track )) {
				lyricsList.List.Should().HaveCount( 1 );
			}
		}

		[Test]
		public void CanGetLyricsForTrackName() {
			var artist = new DbArtist();
			var track = new DbTrack { Name = "track name" };
			var lyric1 = new DbLyric( artist.DbId + 1, track.DbId + 1, track.Name );
			var lyric2 = new DbLyric( artist.DbId + 2, track.DbId + 2, track.Name );

			var sut = CreateSut();
			sut.AddLyric( lyric1 );
			sut.AddLyric( lyric2 );

			using( var lyricsList = sut.GetPossibleLyrics( artist, track )) {
				lyricsList.List.Should().HaveCount( 2 );
			}
		}

		[Test]
		public void CanGetLyricForUpdate() {
			var lyric = new DbLyric( 1, 2, "song name" );

			var sut = CreateSut();
			sut.AddLyric( lyric );

			using( var updater = sut.GetLyricForUpdate( lyric.DbId )) {
				lyric.ShouldHave().AllProperties().EqualTo( updater.Item );
			}
		}

		[Test]
		public void CanUpdateLyric() {
			var artist = new DbArtist();
			var track = new DbTrack { Name = "song name" };
			var lyric = new DbLyric( artist.DbId, track.DbId, track.Name ) { Lyrics = "blah, blah, blah" };

			var sut = CreateSut();
			sut.AddLyric( lyric );

			using( var updater = sut.GetLyricForUpdate( lyric.DbId )) {
				updater.Item.Lyrics = "fah, lah, lah";

				updater.Update();
			}

			using( var lyricsList = sut.GetPossibleLyrics( artist, track )) {
				var retrievedLyric = lyricsList.List.First();

				retrievedLyric.Lyrics.Should().NotBe( lyric.Lyrics );
			}
		}
	}
}
