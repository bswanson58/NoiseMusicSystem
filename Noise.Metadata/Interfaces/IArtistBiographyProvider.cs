using Noise.Metadata.Dto;

namespace Noise.Metadata.Interfaces {
    interface IArtistBiographyProvider {
        void                Insert( DbArtistBiography entity );
        bool                InsertOrUpdate( DbArtistBiography entity );
        bool                Update( DbArtistBiography entity );
        bool                Delete( DbArtistBiography entity );

        DbArtistBiography   GetBiography( string forArtist );
    }
}
