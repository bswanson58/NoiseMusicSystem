using System;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.RemoteServer.Protocol;

namespace Noise.RemoteServer.Services {
    class TrackInformationService : TrackInformation.TrackInformationBase {
        private readonly ITrackProvider     mTrackProvider;
        private readonly IAlbumProvider     mAlbumProvider;
        private readonly IArtistProvider    mArtistProvider;
        private readonly INoiseLog          mLog;

        public TrackInformationService( IArtistProvider artistProvider, IAlbumProvider albumProvider, ITrackProvider trackProvider, INoiseLog log ) {
            mArtistProvider = artistProvider;
            mAlbumProvider = albumProvider;
            mTrackProvider = trackProvider;
            mLog = log;
        }

        private TrackInfo TransformTrack( DbArtist artist, DbAlbum album, DbTrack track ) {
            return new TrackInfo {
                TrackId = track.DbId, AlbumId = album.DbId, ArtistId = artist.DbId,
                TrackName = track.Name, AlbumName = album.Name, ArtistName = artist.Name, VolumeName = track.VolumeName,
                TrackNumber = track.TrackNumber, Duration = track.DurationMilliseconds, Rating = track.Rating, IsFavorite = track.IsFavorite
            };
        }

        public override Task<TrackListResponse> GetTrackList( TrackListRequest request, ServerCallContext context ) {
            return Task.Run( () => {
                var retValue = new TrackListResponse { AlbumId = request.AlbumId, ArtistId = request.AlbumId };

                try {
                    var album = mAlbumProvider.GetAlbum( request.AlbumId );

                    if( album != null ) {
                        var artist = mArtistProvider.GetArtist( album.Artist );

                        using( var trackList = mTrackProvider.GetTrackList( request.AlbumId )) {
                            retValue.TrackList.AddRange( trackList.List.Select( track => TransformTrack( artist, album, track )));

                            retValue.Success = true;
                        }
                    }
                }
                catch( Exception ex ) {
                    mLog.LogException( $"GetTrackList for {request.AlbumId}", ex );

                    retValue.ErrorMessage = ex.Message;
                }

                return retValue;
            });
        }
    }
}
