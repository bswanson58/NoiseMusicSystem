using System;
using System.Collections.Generic;
using System.Linq;
using CuttingEdge.Conditions;
using Noise.EntityFrameworkDatabase.Interfaces;
using Noise.EntityFrameworkDatabase.Logging;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.DataProviders {
	internal class LyricProvider : BaseProvider<DbLyric>, ILyricProvider {
		public LyricProvider( IContextProvider contextProvider, ILogDatabase log ) :
			base( contextProvider, log ) { }

		public void AddLyric( DbLyric lyric ) {
			AddItem( lyric );
		}

		public IDataProviderList<DbLyric> GetPossibleLyrics( DbArtist artist, DbTrack track ) {
			IDataProviderList<DbLyric>	retValue = null;

			Condition.Requires( artist ).IsNotNull();
			Condition.Requires( track ).IsNotNull();

			var			lyricsList = new List<DbLyric>();
			IDbContext	context = null;

			try {
				context = CreateContext();

				var match = Set( context ).FirstOrDefault(entry => (( entry.ArtistId == artist.DbId ) && ( entry.TrackId == track.DbId )));
				if( match != null ) {
					lyricsList.Add( match );
				}

				var matchList = Set( context ).Where( entry => (( entry.ArtistId == artist.DbId ) &&
																( entry.SongName.Equals( track.Name, StringComparison.CurrentCultureIgnoreCase ))));
				lyricsList.AddRange( matchList.Where( lyric => lyric.TrackId != track.DbId ));

				matchList = Set( context ).Where( entry => entry.SongName.Equals( track.Name, StringComparison.CurrentCultureIgnoreCase ));
				lyricsList.AddRange( matchList );

				var uniqueList = lyricsList.GroupBy( lyric => lyric.TrackId ).Select( g => g.First());

				retValue = new EfProviderList<DbLyric>( null, uniqueList );
			}
			catch( Exception ex ) {
				Log.LogException( string.Format( "Get lyrics for {0}, {1}", artist, track ), ex );
			}
			finally {
				if( context != null ) {
					context.Dispose();
				}
			}

			return( retValue );
		}

		public IDataProviderList<DbLyric> GetLyricsForArtist( DbArtist artist ) {
			Condition.Requires( artist ).IsNotNull();

			var context = CreateContext();

			return( new EfProviderList<DbLyric>( context, from entry in Set( context ) where entry.ArtistId == artist.DbId select entry ));
		}

		public IDataUpdateShell<DbLyric> GetLyricForUpdate( long lyricId ) {
			return( GetUpdateShell( lyricId ));
		}
	}
}
