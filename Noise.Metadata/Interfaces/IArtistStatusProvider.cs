using System.IO;
using Noise.Metadata.Dto;

namespace Noise.Metadata.Interfaces {
    interface IArtistStatusProvider {
        void            Insert( DbArtistStatus status );
        bool            Update( DbArtistStatus status );
        bool            Delete( DbArtistStatus status );
        
        DbArtistStatus  GetStatus( string forArtist );

        void            GetArtistArtwork( string forArtist, Stream toStream );
        void            PutArtistArtwork( string forArtist, Stream fromStream );

    }
}
