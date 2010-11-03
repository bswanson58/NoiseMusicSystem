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
		private readonly IDataProvider				mDataProvider;
		private readonly Dictionary<long, DbGenre>	mGenreList;

		public TagManager( IUnityContainer container ) {
			mContainer = container;
			mGenreList = new Dictionary<long, DbGenre>();

			var manager = mContainer.Resolve<INoiseManager>();
			mDataProvider = manager.DataProvider;

			LoadGenreList();
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
	}
}
