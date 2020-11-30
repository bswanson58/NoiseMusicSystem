using Noise.Metadata.Dto;

namespace Noise.Metadata.Interfaces {
    interface IArtistDiscographyProvider {
        void                Insert( DbArtistDiscography entity );
        bool                InsertOrUpdate( DbArtistDiscography entity );
        bool                Update( DbArtistDiscography entity );
        bool                Delete( DbArtistDiscography entity );

        DbArtistDiscography     GetDiscography( string forArtist );
    }
}
