using System;
using System.Collections.Generic;
using System.Linq;
using CuttingEdge.Conditions;
using Noise.EloqueraDatabase.Interfaces;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.EloqueraDatabase.DataProviders {
	internal class LyricProvider : BaseDataProvider<DbLyric>, ILyricProvider {
		public LyricProvider( IEloqueraManager databaseManager ) :
			base( databaseManager ) { }

		public void AddLyric( DbLyric lyric ) {
			InsertItem( lyric );
		}

		public IDataProviderList<DbLyric> GetPossibleLyrics( DbArtist artist, DbTrack track ) {
			IDataProviderList<DbLyric>	retValue = null;

			Condition.Requires( artist ).IsNotNull();
			Condition.Requires( track ).IsNotNull();

			var lyricsList = new List<DbLyric>();

			try {
				var match = GetItem( "SELECT DbLyric WHERE ArtistId = @artistId AND TrackId = @trackId",
										new Dictionary<string, object> {{ "artistId", artist.DbId }, { "trackId", track.DbId }});
				if( match != null ) {
					lyricsList.Add( match );
				}

				using( var matchList = GetList( "SELECT DbLyric WHERE ArtistId = @artistId AND SongName = @songName",
												new Dictionary<string, object> {{ "artistId", artist.DbId }, { "songName", track.Name }})) {
					if( matchList != null ) {
						lyricsList.AddRange( matchList.List.Where( lyric => lyric.TrackId != track.DbId ));
					}
				}

				using( var matchList = GetList( "SELECT DbLyric WHERE SongName = @songName", new Dictionary<string, object> {{ "songName", track.Name }})) {
					if( matchList != null ) {
						lyricsList.AddRange( matchList.List );
					}
				}

				var uniqueList = lyricsList.GroupBy( lyric => lyric.TrackId ).Select( g => g.First());

				retValue = new EloqueraProviderList<DbLyric>( null, uniqueList );
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Exception - GetPossibleLyrics", ex );
			}

			return( retValue );
		}

		public IDataProviderList<DbLyric> GetLyricsForArtist( DbArtist artist ) {
			Condition.Requires( artist ).IsNotNull();

			return( TryGetList( "SELECT DbLyric Where ArtistId = @artistId", new Dictionary<string, object> {{ "artistId", artist.DbId }}, "GetLyricsForArtist" ));
		}

		public IDataUpdateShell<DbLyric> GetLyricForUpdate( long lyricId ) {
			return( GetUpdateShell( "SELECT DbLyric Where DbId = @lyricId", new Dictionary<string, object> {{ "lyricId", lyricId }} ));
		}
	}
}
