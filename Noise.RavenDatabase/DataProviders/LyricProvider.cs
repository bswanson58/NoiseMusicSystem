using System;
using System.Collections.Generic;
using System.Linq;
using CuttingEdge.Conditions;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.RavenDatabase.Interfaces;
using Noise.RavenDatabase.Support;

namespace Noise.RavenDatabase.DataProviders {
	public class LyricProvider : BaseProvider<DbLyric>, ILyricProvider {
		public LyricProvider( IDbFactory databaseFactory ) :
			base( databaseFactory, entity => new object[] { entity.DbId }) {
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
					if( match.Query().Any()) {
						lyricsList.Add( match.Query().First() );
					}
				}

				var matchList = Database.Find( entry => (( entry.ArtistId == artist.DbId ) &&
														  ( entry.SongName.Equals( track.Name, StringComparison.CurrentCultureIgnoreCase ))));
				lyricsList.AddRange( matchList.Query().Where( lyric => lyric.TrackId != track.DbId ) );

				matchList = Database.Find( entry => entry.SongName.Equals( track.Name, StringComparison.CurrentCultureIgnoreCase ));
				lyricsList.AddRange( matchList.Query());

				var uniqueList = lyricsList.GroupBy( lyric => lyric.TrackId ).Select( g => g.First());

				retValue = new RavenDataProviderList<DbLyric>( uniqueList );
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Exception - GetPossibleLyrics", ex );
			}

			return ( retValue );
		}

		public IDataProviderList<DbLyric> GetLyricsForArtist( DbArtist artist ) {
			return( new RavenDataProviderList<DbLyric>( Database.Find( entity => entity.ArtistId == artist.DbId )));
		}

		public IDataUpdateShell<DbLyric> GetLyricForUpdate( long lyricId ) {
			return( new RavenDataUpdateShell<DbLyric>( entity => Database.Update( entity ), Database.Get( lyricId )));
		}
	}
}
