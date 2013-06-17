using System;
using System.Linq;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.RavenDatabase.Interfaces;
using Noise.RavenDatabase.Support;
using Raven.Client.Indexes;

namespace Noise.RavenDatabase.DataProviders {
	public class ArtistByLastChangedTicks : AbstractIndexCreationTask<DbArtist> {
		public ArtistByLastChangedTicks() {
			Map = artists => from artist in artists select new { artist.LastChangeTicks };
		}
	}

	public class ArtistProvider : BaseProvider<DbArtist>, IArtistProvider {
		private readonly ITagAssociationProvider	mTagAssociationProvider;

		public ArtistProvider( IDbFactory databaseFactory, ITagAssociationProvider associationProvider ) :
			base( databaseFactory, entity => new object[] { entity.DbId } ) {
			mTagAssociationProvider = associationProvider;
		}

		public void AddArtist( DbArtist artist ) {
			Database.Add( artist );
		}

		public void DeleteArtist( DbArtist artist ) {
			Database.Delete( artist );
		}

		public DbArtist GetArtist( long dbid ) {
			return( Database.Get( dbid ));
		}

		public DbArtist GetArtistForAlbum( DbAlbum album ) {
			return( Database.Get( album.Artist ));
		}

		public DbArtist FindArtist( string artistName ) {
			return( Database.Get( artist => artist.Name.Equals( artistName )));
		}

		public IDataProviderList<DbArtist> GetArtistList() {
			return( Database.FindAll());
		}

		public IDataProviderList<DbArtist> GetChangedArtists( long changedSince ) {
			return( Database.Find( artist => artist.LastChangeTicks > changedSince ));
		}

		public IDataProviderList<DbArtist> GetFavoriteArtists() {
			return( Database.Find( artist => artist.IsFavorite ));
		}

		public IDataUpdateShell<DbArtist> GetArtistForUpdate( long artistId ) {
			return( new RavenDataUpdateShell<DbArtist>( artist => Database.Update( artist ), Database.Get( artistId )));
		}

		public void UpdateArtistLastChanged( long artistId ) {
			var artist = Database.Get( artistId );

			if( artist != null ) {
				artist.UpdateLastChange();

				Database.Update( artist );
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
			return( Database.Count());
		}
	}
}
