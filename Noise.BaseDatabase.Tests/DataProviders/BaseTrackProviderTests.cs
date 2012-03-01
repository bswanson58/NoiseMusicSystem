using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.BaseDatabase.Tests.DataProviders {
	public abstract class BaseTrackProviderTests : BaseProviderTest<ITrackProvider> {
		[Test]
		public void CanAddTrack() {
			var track = new DbTrack();

			var sut = CreateSut();

			sut.AddTrack( track );
		}

		[Test]
		[ExpectedException( typeof( ArgumentNullException ))]
		public void CannotAddNullTrack() {
			var sut = CreateSut();

			sut.AddTrack( null );
		}

		[Test]
		public void CanRetrieveTrack() {
			var track = new DbTrack();

			var sut = CreateSut();

			sut.AddTrack( track );

			var retrievedTrack = sut.GetTrack( track.DbId );

			track.ShouldHave().AllProperties().EqualTo( retrievedTrack );
		}

		[Test]
		public void CanGetTrackListForAlbum() {
			var album = new DbAlbum();
			var track1 = new DbTrack { Album = album.DbId };
			var track2 = new DbTrack { Album = album.DbId };
			var track3 = new DbTrack { Album = album.DbId + 1 };

			var sut = CreateSut();

			sut.AddTrack( track1 );
			sut.AddTrack( track2 );
			sut.AddTrack( track3 );

			using( var trackList = sut.GetTrackList( album )) {
				trackList.List.Should().HaveCount( 2 );
			}
		}

		[Test]
		public void CanGetTrackListForAlbumId() {
			var album = new DbAlbum();
			var track1 = new DbTrack { Album = album.DbId };
			var track2 = new DbTrack { Album = album.DbId };

			var sut = CreateSut();

			sut.AddTrack( track1 );
			sut.AddTrack( track2 );

			using( var trackList = sut.GetTrackList( album.DbId )) {
				trackList.List.Should().HaveCount( 2 );
			}
		}

		[Test]
		public void CanGetTrackListForGenre() {
			var track1 = new DbTrack { UserGenre = 1 };
			var track2 = new DbTrack { UserGenre = 2 };

			var sut = CreateSut();

			sut.AddTrack( track1 );
			sut.AddTrack( track2 );

			using( var trackList = sut.GetTrackListForGenre( 1 )) {
				trackList.List.Should().HaveCount( 1 );
			}
		}

		[Test]
		public void CanGetFavoriteTracks() {
			var track1 = new DbTrack { IsFavorite = true };
			var track2 = new DbTrack { IsFavorite = false };

			var sut = CreateSut();

			sut.AddTrack( track1 );
			sut.AddTrack( track2 );

			using( var trackList = sut.GetFavoriteTracks()) {
				trackList.List.Should().HaveCount( 1 );
			}
		}

		[Test]
		[Ignore( "Newly added tracks are identified by day only." )]
		public void CanGetNewlyAddedTracks() {
			var track1 = new DbTrack { Name = "old track" };
			System.Threading.Thread.Sleep( 100 );
			var track2 = new DbTrack { Name = "new track" };

			var sut = CreateSut();

			sut.AddTrack( track2 );
			sut.AddTrack( track1 );

			using( var trackList = sut.GetNewlyAddedTracks()) {
				var firstTrack = trackList.List.First();

				firstTrack.ShouldHave().AllProperties().EqualTo( track2 );
			}
		}

		[Test]
		public void CanGetTracksForPlayList() {
			var track1 = new DbTrack { Name = "track 1" };
			var track2 = new DbTrack { Name = "track 2" };
			var track3 = new DbTrack { Name = "track 3" };
			var playList = new DbPlayList( "play list", "description", new List<long> { track2.DbId, track1.DbId });

			var sut = CreateSut();

			sut.AddTrack( track1 );
			sut.AddTrack( track2 );
			sut.AddTrack( track3 );

			var trackList = sut.GetTrackListForPlayList( playList );

			trackList.Should().HaveCount( 2 );
		}

		[Test]
		public void CanGetTrackForUpdate() {
			var track = new DbTrack { Name = "track name" };

			var sut = CreateSut();
			sut.AddTrack( track );

			using( var updater = sut.GetTrackForUpdate( track.DbId )) {
				track.ShouldHave().AllProperties().EqualTo( updater.Item );
			}
		}

		[Test]
		public void CanUpdateTrack() {
			var track = new DbTrack { Name = "original name" };

			var sut = CreateSut();
			sut.AddTrack( track );

			using( var updater = sut.GetTrackForUpdate( track.DbId )) {
				updater.Item.Name = "new name";

				updater.Update();
			}

			var retrievedTrack = sut.GetTrack( track.DbId );

			retrievedTrack.Name.Should().Be( "new name" );
		}

		[Test]
		public void CanGetTrackItemCount() {
			var track1 = new DbTrack();
			var track2 = new DbTrack();
			var track3 = new DbTrack();

			var sut = CreateSut();

			sut.AddTrack( track1 );
			sut.AddTrack( track2 );
			sut.AddTrack( track3 );

			var	trackCount = sut.GetItemCount();

			trackCount.Should().Be( 3 );
		}
	}
}
