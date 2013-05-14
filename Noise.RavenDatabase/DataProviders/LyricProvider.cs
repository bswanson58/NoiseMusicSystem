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
	public class LyricProvider : ILyricProvider {
		private readonly IDbFactory				mDbFactory;
		private readonly IRepository<DbLyric>	mDatabase;

		public LyricProvider( IDbFactory databaseFactory ) {
			mDbFactory = databaseFactory;

			mDatabase = new RavenRepository<DbLyric>( mDbFactory.GetLibraryDatabase(), entity => new object[] { entity.DbId });
		}

		public void AddLyric( DbLyric lyric ) {
			mDatabase.Add( lyric );
		}

		public IDataProviderList<DbLyric> GetPossibleLyrics( DbArtist artist, DbTrack track ) {
			IDataProviderList<DbLyric>	retValue = null;

			Condition.Requires( artist ).IsNotNull();
			Condition.Requires( track ).IsNotNull();

			var			lyricsList = new List<DbLyric>();

			try {
				var match = mDatabase.Find( entry => (( entry.ArtistId == artist.DbId ) && ( entry.TrackId == track.DbId )));
				if( match != null ) {
					if( match.Query().Any()) {
						lyricsList.Add( match.Query().First() );
					}
				}

				var matchList = mDatabase.Find( entry => (( entry.ArtistId == artist.DbId ) &&
														  ( entry.SongName.Equals( track.Name, StringComparison.CurrentCultureIgnoreCase ))));
				lyricsList.AddRange( matchList.Query().Where( lyric => lyric.TrackId != track.DbId ) );

				matchList = mDatabase.Find( entry => entry.SongName.Equals( track.Name, StringComparison.CurrentCultureIgnoreCase ));
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
			return( new RavenDataProviderList<DbLyric>( mDatabase.Find( entity => entity.ArtistId == artist.DbId )));
		}

		public IDataUpdateShell<DbLyric> GetLyricForUpdate( long lyricId ) {
			return( new RavenDataUpdateShell<DbLyric>( entity => mDatabase.Update( entity ), mDatabase.Get( lyricId )));
		}
	}
}
