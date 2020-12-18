using System;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.RemoteServer.Protocol;

namespace Noise.RemoteServer.Services {
    class TagInformationService : TagInformation.TagInformationBase {
        private readonly IArtistProvider    mArtistProvider;
        private readonly IAlbumProvider     mAlbumProvider;
        private readonly ITrackProvider     mTrackProvider;
        private readonly IUserTagManager    mTagManager;
        private readonly INoiseLog          mLog;

        public TagInformationService( IUserTagManager tagManager, IArtistProvider artistProvider, IAlbumProvider albumProvider,
                                      ITrackProvider trackProvider, INoiseLog log ) {
            mTagManager = tagManager;
            mArtistProvider = artistProvider;
            mAlbumProvider = albumProvider;
            mTrackProvider = trackProvider;
            mLog = log;
        }

        private TagInfo TransformTag( DbTag fromTag ) {
            return new TagInfo { TagId = fromTag.DbId, TagName = fromTag.Name };
        }

        public override Task<TagListResponse> GetUserTags( TagInformationEmpty request, ServerCallContext context ) {
            return Task.Run( () => {
                var retValue = new TagListResponse();

                try {
                    var tagList = mTagManager.GetUserTagList();

                    retValue.TagList.AddRange( from t in tagList select TransformTag( t ));

                    retValue.Success = true;
                }
                catch( Exception ex ) {
                    mLog.LogException( "GetUserTags", ex );

                    retValue.ErrorMessage = ex.Message;
                }

                return retValue;
            });
        }

        private TagAssociationInfo TransformAssociation( DbArtist artist, DbAlbum album, DbTrack track) {
            return new TagAssociationInfo {
                ArtistId = artist.DbId, AlbumId = album.DbId, TrackId = track.DbId,
                ArtistName = artist.Name, AlbumName = album.Name, VolumeName = track.VolumeName, TrackName = track.Name,
                Duration = track.DurationMilliseconds, IsFavorite = track.IsFavorite, Rating = track.Rating
            };
        }

        public override Task<TagAssociationsResponse> GetTagAssociations( TagAssociationRequest request, ServerCallContext context ) {
            return Task.Run( () => {
                var retValue = new TagAssociationsResponse();

                try {
                    var associations = mTagManager.GetAssociations( request.TagId );

                    foreach( var association in associations ) {
                        var track = mTrackProvider.GetTrack( association.ArtistId );

                        if( track != null ) {
                            var artist = mArtistProvider.GetArtist( track.Artist );
                            var album = mAlbumProvider.GetAlbum( track.Album );

                            if(( artist != null ) &&
                               ( album != null )) {
                                retValue.TagAssociations.Add( TransformAssociation( artist, album, track ));
                            }
                        }
                    }

                    retValue.Success = true;
                }
                catch( Exception ex ) {
                    mLog.LogException( "GetTagAssociations", ex );

                    retValue.ErrorMessage = ex.Message;
                }

                return retValue;
            });
        }
    }
}
