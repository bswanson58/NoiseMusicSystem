using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using CuttingEdge.Conditions;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Database {
	public class TagManager : ITagManager,
							  IHandle<Events.DatabaseOpened>, IHandle<Events.DatabaseClosing> {
		private readonly IEventAggregator			mEventAggregator;
		private readonly INoiseLog					mLog;
		private readonly IGenreProvider				mGenreProvider;
		private readonly ITagProvider				mTagProvider;
		private readonly ITagAssociationProvider	mTagAssociationProvider;
		private readonly Dictionary<long, DbGenre>	mGenreList;
		private readonly List<DbDecadeTag>			mDecadeList;

		public TagManager( IEventAggregator eventAggregator, IGenreProvider genreProvider,
						   ITagProvider tagProvider, ITagAssociationProvider tagAssociationProvider, INoiseLog log ) {
			mEventAggregator = eventAggregator;
			mLog = log;
			mGenreProvider = genreProvider;
			mTagProvider = tagProvider;
			mTagAssociationProvider = tagAssociationProvider;
			mGenreList = new Dictionary<long, DbGenre>();
			mDecadeList = new List<DbDecadeTag>();

			mEventAggregator.Subscribe( this );
		}

		public void Handle( Events.DatabaseOpened args ) {
			try {
				LoadGenreList();
				LoadDecadeList();
				if( mDecadeList.Count == 0 ) {
					InitializeDecadeList();
				}
			}
			catch( Exception ex ) {
				mLog.LogException( "Handle DatabaseOpened", ex );
			}
		}

		public void Handle( Events.DatabaseClosing args ) {
			mGenreList.Clear();
			mDecadeList.Clear();
		}

		public long ResolveGenre( string genreName ) {
			var retValue = Constants.cDatabaseNullOid;
			var conformedName = ConformGenreName( genreName );

			if(!string.IsNullOrWhiteSpace( conformedName )) {
				var genre = mGenreList.Values.FirstOrDefault( item => item.Name.Equals( conformedName ));

				if( genre == null ) {
					genre = new DbGenre { Name = conformedName };

					mGenreList.Add( genre.DbId, genre );
					mGenreProvider.AddGenre( genre );
				}

				Condition.Ensures( genre ).IsNotNull();

				retValue = genre.DbId;
			}

			return( retValue );
		}

		private static string ConformGenreName( string input ) {
			var retValue = "";

			if(!string.IsNullOrWhiteSpace( input )) {
				retValue = input.Trim();
				retValue = retValue.ToLower();
			}

			return( retValue );
		}

		public DbGenre GetGenre( long genreId ) {
			DbGenre	retValue = null;

			if( mGenreList.ContainsKey( genreId )) {
				retValue = mGenreList[genreId];
			}

			return( retValue );
		}

		public IEnumerable<DbDecadeTag> DecadeTagList {
			get{ return( mDecadeList ); }
		}

		public IEnumerable<DbGenre> GenreList {
			get{ return( mGenreList.Values ); }
		} 

		public IEnumerable<long> ArtistListForDecade( long decadeId ) {
			var	retValue = new List<long>();

			using( var tags = mTagAssociationProvider.GetTagList( eTagGroup.Decade, decadeId )) {
				retValue.AddRange( tags.List.Select( tag => tag.ArtistId ).Distinct());
			}

			return( retValue );
		}

		public IEnumerable<long> AlbumListForDecade( long artistId, long decadeId ) {
			var	retValue = new List<long>();

			using( var tags = mTagAssociationProvider.GetTagList( eTagGroup.Decade, decadeId )) {
				retValue.AddRange( tags.List.Where( tag => tag.ArtistId == artistId ).Select( tag => tag.AlbumId ).Distinct());
			}

			return( retValue );
		}

		public IEnumerable<long> ArtistListForGenre( long genreId ) {
			var	retValue = new List<long>();

			using( var tags = mTagAssociationProvider.GetTagList( eTagGroup.Genre, genreId )) {
				retValue.AddRange( tags.List.Select( tag => tag.ArtistId ).Distinct());
			}

			return ( retValue );
		}

		public IEnumerable<long> AlbumListForGenre( long artistId, long genreId ) {
			var	retValue = new List<long>();

			using( var tags = mTagAssociationProvider.GetTagList( eTagGroup.Genre, genreId ) ) {
				retValue.AddRange( tags.List.Where( tag => tag.ArtistId == artistId ).Select( tag => tag.AlbumId ).Distinct() );
			}

			return ( retValue );
		}

		private void LoadGenreList() {
			mGenreList.Clear();

			using( var genreList = mGenreProvider.GetGenreList()) {
				foreach( var genre in genreList.List ) {
					mGenreList.Add( genre.DbId, genre );
				}
			}

			if(!mGenreList.ContainsKey( Constants.cDatabaseNullOid )) {
				mGenreList.Add( Constants.cDatabaseNullOid, new DbGenre( Constants.cDatabaseNullOid ) { Description = "Unknown genre" });
			}
		}

		private void LoadDecadeList() {
			mDecadeList.Clear();

			using( var tagList = mTagProvider.GetTagList( eTagGroup.Decade )) {
				mDecadeList.AddRange( tagList.List.Where( tag => tag is DbDecadeTag ).Select( tag => tag as DbDecadeTag ));
			}
		}

		private void InitializeDecadeList() {
			var decadeTag = new DbDecadeTag( "Unknown" ) { Description = "Albums without a published year",
														   StartYear = Constants.cUnknownYear, EndYear = Constants.cUnknownYear };
			mTagProvider.AddTag( decadeTag );

			decadeTag = new DbDecadeTag( "Various" ) { Description = "Various release years",
													StartYear = Constants.cVariousYears, EndYear = Constants.cVariousYears };
			mTagProvider.AddTag( decadeTag );

			decadeTag = new DbDecadeTag( "Oldies" ) { Description = "The Oldies",
													StartYear = 1900, EndYear = 1959 };
			mTagProvider.AddTag( decadeTag );

			decadeTag = new DbDecadeTag( "60's" ) { Description = "The Roaring 60's",
													StartYear = 1960, EndYear = 1969 };
			mTagProvider.AddTag( decadeTag );

			decadeTag = new DbDecadeTag( "70's" ) { Description = "The Psychedelic 70's",
													StartYear = 1970, EndYear = 1979,
													Website = "http://www.inthe70s.com/" };
			mTagProvider.AddTag( decadeTag );

			decadeTag = new DbDecadeTag( "80's" ) { Description = "Like Totally",
													StartYear = 1980, EndYear = 1989,
													Website = "http://www.inthe80s.com/" };
			mTagProvider.AddTag( decadeTag );

			decadeTag = new DbDecadeTag( "90's" ) { Description = "Grunge goes mainstream",
													StartYear = 1990, EndYear = 1999,
													Website = "http://www.inthe90s.com/" };
			mTagProvider.AddTag( decadeTag );

			decadeTag = new DbDecadeTag( "Uh Oh's" ) { Description = "The Turn of the Century",
													StartYear = 2000, EndYear = 2009 };
			mTagProvider.AddTag( decadeTag );

			decadeTag = new DbDecadeTag( "10's" ) { Description = "Releases of this decade",
													StartYear = 2010, EndYear = 2019 };
			mTagProvider.AddTag( decadeTag );

			LoadDecadeList();
		}
	}
}
