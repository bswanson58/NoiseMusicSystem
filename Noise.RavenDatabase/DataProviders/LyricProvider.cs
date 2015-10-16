using System;
using System.Collections.Generic;
using System.Linq;
using CuttingEdge.Conditions;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.RavenDatabase.Interfaces;
using Noise.RavenDatabase.Logging;
using Noise.RavenDatabase.Support;

namespace Noise.RavenDatabase.DataProviders {
	internal class LyricProvider : BaseProvider<DbLyric>, ILyricProvider {
		public LyricProvider( IDbFactory databaseFactory, ILogRaven log ) :
			base( databaseFactory, entity => new object[] { entity.DbId }, log ) {
		}

		public void AddLyric( DbLyric lyric ) {
			Database.Add( lyric );
		}

		public IDataProviderList<DbLyric> GetPossibleLyrics( DbArtist artist, DbTrack track ) {
			IDataProviderList<DbLyric>	retValue = null;

			Condition.Requires( artist ).IsNotNull();
			Condition.Requires( track ).IsNotNull();

			var			lyricsList = new List<DbLyric>();

			try {
				var match = Database.Find( entry => (( entry.ArtistId == artist.DbId ) && ( entry.TrackId == track.DbId )));
				if( match != null ) {
					if( match.List.Any()) {
						lyricsList.Add( match.List.First() );
					}
				}

				var matchList = Database.Find( entry => (( entry.ArtistId == artist.DbId ) &&
														  ( entry.SongName.Equals( track.Name, StringComparison.CurrentCultureIgnoreCase ))));
				lyricsList.AddRange( matchList.List.Where( lyric => lyric.TrackId != track.DbId ) );

				matchList = Database.Find( entry => entry.SongName.Equals( track.Name, StringComparison.CurrentCultureIgnoreCase ));
				lyricsList.AddRange( matchList.List );

				var uniqueList = lyricsList.GroupBy( lyric => lyric.TrackId ).Select( g => g.First());

				retValue = new RavenDataProviderList<DbLyric>( uniqueList );
			}
			catch( Exception ex ) {
				Log.LogException( string.Format( "GetPossibleLyrics for {0}, {1}", artist, track ), ex );
			}

			return ( retValue );
		}

		public IDataProviderList<DbLyric> GetLyricsForArtist( DbArtist artist ) {
			return( Database.Find( entity => entity.ArtistId == artist.DbId ));
		}

		public IDataUpdateShell<DbLyric> GetLyricForUpdate( long lyricId ) {
			return( new RavenDataUpdateShell<DbLyric>( entity => Database.Update( entity ), Database.Get( lyricId )));
		}
	}
}
