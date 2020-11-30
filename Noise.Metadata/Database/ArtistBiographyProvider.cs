using LiteDB;
using Noise.Metadata.Dto;
using Noise.Metadata.Interfaces;

namespace Noise.Metadata.Database {
    class ArtistBiographyProvider : EntityProvider<DbArtistBiography>, IArtistBiographyProvider {
        public ArtistBiographyProvider( IDatabaseProvider databaseProvider ) :
            base( databaseProvider, EntityCollection.BiographyCollection ) { }

        protected override void InitializeDatabase( LiteDatabase db ) {
            base.InitializeDatabase( db );

            db.GetCollection<DbArtistBiography>( EntityCollection.BiographyCollection ).EnsureIndex( b => b.ArtistName );
        }

        public void Insert( DbArtistBiography entity ) {
            InsertEntity( entity );
        }

        public bool Update( DbArtistBiography entity ) {
            return UpdateEntity( entity );
        }

        public bool InsertOrUpdate( DbArtistBiography entity ) {
            if(!Update( entity )) {
                Insert( entity );
            }

            return true;
        }

        public bool Delete( DbArtistBiography entity ) {
            return DeleteEntity( entity );
        }

        public DbArtistBiography GetBiography( string forArtist ) {
            var retValue = default( DbArtistBiography );
            var collection = CreateCollection();

            if( collection != null ) {
                retValue = collection.FindOne( Query.EQ( nameof( DbArtistBiography.ArtistName ), forArtist ));
            }

            return retValue;
        }
    }
}
