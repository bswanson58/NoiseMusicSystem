using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.RemoteServer.Protocol;
using ReusableBits.ExtensionClasses.MoreLinq;

namespace Noise.RemoteServer.Services {
    class TrackInformationService : TrackInformation.TrackInformationBase {
        private readonly ITrackProvider     mTrackProvider;
        private readonly IAlbumProvider     mAlbumProvider;
        private readonly IArtistProvider    mArtistProvider;
        private readonly IUserTagManager    mTagManager;
        private readonly INoiseLog          mLog;

        public TrackInformationService( IArtistProvider artistProvider, IAlbumProvider albumProvider, ITrackProvider trackProvider,
                                        IUserTagManager userTagManager, INoiseLog log ) {
            mArtistProvider = artistProvider;
            mAlbumProvider = albumProvider;
            mTrackProvider = trackProvider;
            mTagManager = userTagManager;
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

        public override Task<TrackListResponse> GetRatedTracks( TrackRatingRequest request, ServerCallContext context ) {
            return Task.Run( () => {
                var retValue = new TrackListResponse { AlbumId = Constants.cDatabaseNullOid, ArtistId = request.ArtistId };

                try {
                    var artist = mArtistProvider.GetArtist( request.ArtistId );

                    if( artist != null ) {
                        using( var ratedList = mTrackProvider.GetRatedTracks( artist )) {
                            var ratedTracks = from t in ratedList.List where t.Rating > request.IncludeRatingsOver select t;

                            ratedTracks.ForEach( track => {
                                var album = mAlbumProvider.GetAlbum( track.Album );

                                if( album != null ) {
                                    retValue.TrackList.Add( TransformTrack( artist, album, track ));
                                }
                            });
                        }

                        if( request.IncludeFavorites ) {
                            using( var favoriteList = mTrackProvider.GetFavoriteTracks()) {
                                var favoriteTracks = from t in favoriteList.List where t.Artist.Equals( request.ArtistId ) select t;

                                favoriteTracks.ForEach( track => {
                                    var album = mAlbumProvider.GetAlbum( track.Album );

                                    if( album != null ) {
                                        retValue.TrackList.Add( TransformTrack( artist, album, track ));
                                    }
                                });
                            }
                        }

                        retValue.Success = true;
                    }
                }
                catch( Exception ex ) {
                    mLog.LogException( $"GetRatedTracks for {request.ArtistId}", ex );

                    retValue.ErrorMessage = ex.Message;
                }

                return retValue;
            });
        }

        public override Task<TrackListResponse> GetTaggedTracks( TrackTagsRequest request, ServerCallContext context ) {
            return Task.Run( () => {
                var retValue = new TrackListResponse { ArtistId = Constants.cDatabaseNullOid, AlbumId = Constants.cDatabaseNullOid };

                try {
                    var tags = mTagManager.GetAssociatedTags( request.TrackId );
                    var associatedTracks = new List<TrackInfo>();

                    tags.ForEach( tag => {
                        var associations = mTagManager.GetAssociations( tag.DbId );

                        associatedTracks.AddRange( from a in associations select CreateTrackInfo( a ));
                    });

                    // eliminate duplicate tracks
                    retValue.TrackList.AddRange( associatedTracks.GroupBy( t => t.TrackId ).Select( g => g.First()));
                    retValue.Success = true;
                }
                catch( Exception ex ) {
                    mLog.LogException( $"GetTaggedTracks for {request.TrackId}", ex );

                    retValue.ErrorMessage = ex.Message;
                }
                return retValue;
            });
        }

        private TrackInfo CreateTrackInfo( DbTagAssociation fromTag ) {
            var track = mTrackProvider.GetTrack( fromTag.ArtistId );
            var artist = mArtistProvider.GetArtist( track.Artist );
            var album = mAlbumProvider.GetAlbum( track.Album );

            return TransformTrack( artist, album, track );
        }
    }
}
