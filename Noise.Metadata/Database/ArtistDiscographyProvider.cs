using LiteDB;
using Noise.Metadata.Dto;
using Noise.Metadata.Interfaces;

namespace Noise.Metadata.Database {
    class ArtistDiscographyProvider : EntityProvider<DbArtistDiscography>, IArtistDiscographyProvider {
        public ArtistDiscographyProvider( IDatabaseProvider databaseProvider ) :
            base( databaseProvider, EntityCollection.DiscographyCollection ) { }

        protected override void InitializeDatabase( LiteDatabase db ) {
            base.InitializeDatabase( db );

            db.GetCollection<DbArtistDiscography>( EntityCollection.DiscographyCollection ).EnsureIndex( b => b.ArtistName );
        }

        public void Insert( DbArtistDiscography entity ) {
            InsertEntity( entity );
        }

        public bool Update( DbArtistDiscography entity ) {
            return UpdateEntity( entity );
        }

        public bool InsertOrUpdate( DbArtistDiscography entity ) {
            if(!Update( entity )) {
                Insert( entity );
            }

            return true;
        }

        public bool Delete( DbArtistDiscography entity ) {
            return DeleteEntity( entity );
        }

        public DbArtistDiscography GetDiscography( string forArtist ) {
            var retValue = default( DbArtistDiscography );
            var collection = CreateCollection();

            if( collection != null ) {
                retValue = collection.FindOne( Query.EQ( nameof( DbArtistDiscography.ArtistName ), forArtist ));
            }

            return retValue;
        }
    }
}
