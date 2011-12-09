using System;
using System.Collections.Generic;
using System.Linq;
using CuttingEdge.Conditions;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Database {
	public class TagManager : ITagManager {
		private readonly IUnityContainer			mContainer;
		private readonly INoiseManager				mNoiseManager;
		private readonly IDataProvider				mDataProvider;
		private readonly Dictionary<long, DbGenre>	mGenreList;
		private readonly List<DbDecadeTag>			mDecadeList;

		public TagManager( IUnityContainer container ) {
			mContainer = container;
			mGenreList = new Dictionary<long, DbGenre>();
			mDecadeList = new List<DbDecadeTag>();

			mNoiseManager = mContainer.Resolve<INoiseManager>();
			mDataProvider = mNoiseManager.DataProvider;
		}

		public bool Initialize() {
			var retValue = false;

			try {
				LoadGenreList();
				LoadDecadeList();
				if( mDecadeList.Count == 0 ) {
					InitializeDecadeList();
				}

				retValue = true;
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Exception - TagManager.Initialize", ex );
			}

			return( retValue );
		}

		public long ResolveGenre( string genreName ) {
			var retValue = Constants.cDatabaseNullOid;
			var conformedName = ConformGenreName( genreName );

			if(!string.IsNullOrWhiteSpace( conformedName )) {
				var genre = mGenreList.Values.FirstOrDefault( item => item.Name.Equals( conformedName ));

				if( genre == null ) {
					genre = new DbGenre { Name = conformedName };

					mGenreList.Add( genre.DbId, genre );
					mDataProvider.InsertItem( genre );
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

		public IEnumerable<long> ArtistList( long tagId ) {
			var	retValue = new List<long>();

			using( var tags = mDataProvider.GetTagAssociations( tagId )) {
				retValue.AddRange( tags.List.Select( tag => tag.ArtistId ).Distinct());
			}

			return( retValue );
		}

		public IEnumerable<long> AlbumList( long artistId, long tagId ) {
			var	retValue = new List<long>();

			using( var tags = mDataProvider.GetTagAssociations( tagId )) {
				retValue.AddRange( tags.List.Where( tag => tag.ArtistId == artistId ).Select( tag => tag.AlbumId ).Distinct());
			}

			return( retValue );
		}

		private void LoadGenreList() {
			mGenreList.Clear();

			using( var genreList = mDataProvider.GetGenreList()) {
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

			using( var tagList = mDataProvider.GetTagList( eTagGroup.Decade )) {
				mDecadeList.AddRange( tagList.List.Where( tag => tag is DbDecadeTag ).Select( tag => tag as DbDecadeTag ));
			}
		}

		private void InitializeDecadeList() {
			var decadeTag = new DbDecadeTag( "Unknown" ) { Description = "Albums without a published year",
														   StartYear = Constants.cUnknownYear, EndYear = Constants.cUnknownYear };
			mDataProvider.InsertItem( decadeTag );

			decadeTag = new DbDecadeTag( "Various" ) { Description = "Various release years",
													StartYear = Constants.cVariousYears, EndYear = Constants.cVariousYears };
			mDataProvider.InsertItem( decadeTag );

			decadeTag = new DbDecadeTag( "Oldies" ) { Description = "The Oldies",
													StartYear = 1900, EndYear = 1959 };
			mDataProvider.InsertItem( decadeTag );

			decadeTag = new DbDecadeTag( "60's" ) { Description = "The Roaring 60's",
													StartYear = 1960, EndYear = 1969 };
			mDataProvider.InsertItem( decadeTag );

			decadeTag = new DbDecadeTag( "70's" ) { Description = "The Psychedelic 70's",
													StartYear = 1970, EndYear = 1979,
													Website = "http://www.inthe70s.com/" };
			mDataProvider.InsertItem( decadeTag );

			decadeTag = new DbDecadeTag( "80's" ) { Description = "Like Totally",
													StartYear = 1980, EndYear = 1989,
													Website = "http://www.inthe80s.com/" };
			mDataProvider.InsertItem( decadeTag );

			decadeTag = new DbDecadeTag( "90's" ) { Description = "Grunge goes mainstream",
													StartYear = 1990, EndYear = 1999,
													Website = "http://www.inthe90s.com/" };
			mDataProvider.InsertItem( decadeTag );

			decadeTag = new DbDecadeTag( "Uh Oh's" ) { Description = "The Turn of the Century",
													StartYear = 2000, EndYear = 2009 };
			mDataProvider.InsertItem( decadeTag );

			decadeTag = new DbDecadeTag( "10's" ) { Description = "Releases of this decade",
													StartYear = 2010, EndYear = 2019 };
			mDataProvider.InsertItem( decadeTag );

			LoadDecadeList();
		}
	}
}
