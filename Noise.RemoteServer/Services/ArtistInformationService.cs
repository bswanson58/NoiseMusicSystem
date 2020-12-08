using System;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.RemoteServer.Protocol;

namespace Noise.RemoteServer.Services {
    class ArtistInformationService : ArtistInformation.ArtistInformationBase {
        private readonly IArtistProvider    mArtistProvider;
        private readonly ITagManager		mTagManager;
        private readonly INoiseLog          mLog;

        public ArtistInformationService( IArtistProvider artistProvider, ITagManager tagManager, INoiseLog log ) {
            mArtistProvider = artistProvider;
            mTagManager = tagManager;
            mLog = log;
        }

        private ArtistInfo TransformArtist( DbArtist artist ) {
            return new ArtistInfo{ DbId = artist.DbId, ArtistName = artist.Name, AlbumCount = artist.AlbumCount, 
                                   Rating = artist.Rating, Genre = RetrieveGenre( artist.Genre ), IsFavorite = artist.IsFavorite};
        }

        public override Task<ArtistListResponse> GetArtistList( ArtistInfoEmpty request, ServerCallContext context ) {
            return Task.Run( () => {
                var retValue = new ArtistListResponse();

                try {
                    using( var artistList = mArtistProvider.GetArtistList()) {
                        retValue.ArtistList.AddRange( artistList.List.Select( TransformArtist ));
                        retValue.Success = true;
                    }
                }
                catch( Exception ex ) {
                    mLog.LogException( "GetArtistList", ex );

                    retValue.ErrorMessage = ex.Message;
                }

                return retValue;
            });
        }

        private string RetrieveGenre( long genreId ) {
            var retValue = String.Empty;

            if( genreId != Constants.cDatabaseNullOid ) {
                var genre = mTagManager.GetGenre( genreId );

                if( genre != null ){
                    retValue = genre.Name;
                }
            }

            return retValue;
        }

    }
}
