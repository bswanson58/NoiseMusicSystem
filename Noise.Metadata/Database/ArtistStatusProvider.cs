using System.IO;
using LiteDB;
using Noise.Infrastructure;
using Noise.Metadata.Dto;
using Noise.Metadata.Interfaces;

namespace Noise.Metadata.Database {
    class ArtistStatusProvider : EntityProvider<DbArtistStatus>, IArtistStatusProvider {
        public ArtistStatusProvider( IDatabaseProvider databaseProvider ) :
            base( databaseProvider, EntityCollection.ArtistStatusCollection ) { }

        protected override void InitializeDatabase( LiteDatabase db ) {
            base.InitializeDatabase( db );

            db.GetCollection<DbArtistStatus>( EntityCollection.ArtistStatusCollection ).EnsureIndex( b => b.ArtistName );
        }

        public void Insert( DbArtistStatus entity ) {
            InsertEntity( entity );
        }

        public bool Update( DbArtistStatus entity ) {
            return UpdateEntity( entity );
        }

        public bool Delete( DbArtistStatus entity ) {
            return DeleteEntity( entity );
        }

        public DbArtistStatus GetStatus( string forArtist ) {
            var retValue = default( DbArtistStatus );
            var collection = CreateCollection();

            if( collection != null ) {
                retValue = collection.FindOne( Query.EQ( nameof( DbArtistStatus.ArtistName ), forArtist ));
            }

            return retValue;
        }

        public void GetArtistArtwork( string forArtist, Stream toStream ) {
            if( HasArtistImage( forArtist )) {
                var storage = CreateConnection().GetStorage<string>( Constants.ArtistArtworkStorageName );

                storage.Download( forArtist, toStream );
            }
        }

        public bool HasArtistImage( string forArtist ) {
            var storage = CreateConnection().GetStorage<string>( Constants.ArtistArtworkStorageName );

            return storage.Exists( forArtist );
        }

        public void PutArtistArtwork( string forArtist, Stream fromStream ) {
            var storage = CreateConnection().GetStorage<string>( Constants.ArtistArtworkStorageName );

            storage.Upload( forArtist, forArtist, fromStream );
        }
    }
}
