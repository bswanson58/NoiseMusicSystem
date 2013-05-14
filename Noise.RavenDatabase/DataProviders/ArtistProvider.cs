using System;
using System.Linq;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.RavenDatabase.Interfaces;
using Noise.RavenDatabase.Support;

namespace Noise.RavenDatabase.DataProviders {
	public class ArtistProvider : IArtistProvider {
		private readonly IDbFactory					mDbFactory;
		private readonly IRepository<DbArtist>		mDatabase;
		private readonly ITagAssociationProvider	mTagAssociationProvider;

		public ArtistProvider( IDbFactory databaseFactory, ITagAssociationProvider associationProvider ) {
			mDbFactory = databaseFactory;
			mTagAssociationProvider = associationProvider;

			mDatabase = new RavenRepository<DbArtist>( mDbFactory.GetLibraryDatabase(), artist => new object[] { artist.DbId });
		}

		public void AddArtist( DbArtist artist ) {
			mDatabase.Add( artist );
		}

		public void DeleteArtist( DbArtist artist ) {
			mDatabase.Delete( artist );
		}

		public DbArtist GetArtist( long dbid ) {
			return( mDatabase.Get( dbid ));
		}

		public DbArtist GetArtistForAlbum( DbAlbum album ) {
			return( mDatabase.Get( album.Artist ));
		}

		public DbArtist FindArtist( string artistName ) {
			return( mDatabase.Get( artist => artist.Name.Equals( artistName )));
		}

		public IDataProviderList<DbArtist> GetArtistList() {
			return( new RavenDataProviderList<DbArtist>( mDatabase.FindAll()));
		}

		public IDataProviderList<DbArtist> GetArtistList( IDatabaseFilter filter ) {
			return( new RavenFilteredProviderList<DbArtist>( mDatabase.FindAll(), filter ));
		}

		public IDataProviderList<DbArtist> GetChangedArtists( long changedSince ) {
			return( new RavenDataProviderList<DbArtist>( mDatabase.Find( artist => artist.LastChangeTicks > changedSince )));
		}

		public IDataProviderList<DbArtist> GetFavoriteArtists() {
			return( new RavenDataProviderList<DbArtist>( mDatabase.Find( artist => artist.IsFavorite )));
		}

		public IDataUpdateShell<DbArtist> GetArtistForUpdate( long artistId ) {
			return( new RavenDataUpdateShell<DbArtist>( artist => mDatabase.Update( artist ), mDatabase.Get( artistId )));
		}

		public void UpdateArtistLastChanged( long artistId ) {
			var artist = mDatabase.Get( artistId );

			if( artist != null ) {
				artist.UpdateLastChange();

				mDatabase.Update( artist );
			}
		}

		public IDataProviderList<long> GetArtistCategories( long artistId ) {
			IDataProviderList<long>	retValue = null;

			try {
				using( var tagList = mTagAssociationProvider.GetArtistTagList( artistId, eTagGroup.User )) {
					retValue = new RavenSimpleDataProviderList<long>( from DbTagAssociation assoc in tagList.List.ToList() select assoc.TagId );
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Exception - GetArtistCategories", ex );
			}

			return ( retValue );
		}

		public long GetItemCount() {
			return( mDatabase.Count());
		}
	}
}
