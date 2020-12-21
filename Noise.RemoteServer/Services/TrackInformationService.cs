using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using Grpc.Core;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.RemoteServer.Protocol;
using ReusableBits.ExtensionClasses.MoreLinq;

namespace Noise.RemoteServer.Services {
    class TrackInformationService : TrackInformation.TrackInformationBase {
        private readonly IEventAggregator   mEventAggregator;
        private readonly ITrackProvider     mTrackProvider;
        private readonly IAlbumProvider     mAlbumProvider;
        private readonly IArtistProvider    mArtistProvider;
        private readonly ISearchProvider    mSearchProvider;
        private readonly IUserTagManager    mTagManager;
        private readonly INoiseLog          mLog;

        public TrackInformationService( IArtistProvider artistProvider, IAlbumProvider albumProvider, ITrackProvider trackProvider,
                                        IUserTagManager userTagManager, ISearchProvider searchProvider, INoiseLog log,
                                        IEventAggregator eventAggregator ) {
            mArtistProvider = artistProvider;
            mAlbumProvider = albumProvider;
            mTrackProvider = trackProvider;
            mTagManager = userTagManager;
            mSearchProvider = searchProvider;
            mEventAggregator = eventAggregator;
            mLog = log;
        }

        private TrackInfo TransformTrack( DbArtist artist, DbAlbum album, DbTrack track ) { 
            var retValue = new TrackInfo {
                TrackId = track.DbId, AlbumId = album.DbId, ArtistId = artist.DbId,
                TrackName = track.Name, AlbumName = album.Name, ArtistName = artist.Name, VolumeName = track.VolumeName,
                TrackNumber = track.TrackNumber, Duration = track.DurationMilliseconds, Rating = track.Rating, IsFavorite = track.IsFavorite,
            };

            retValue.Tags.AddRange( GetTrackTags( track ));

            return retValue;
        }

        private IEnumerable<TrackTagInfo> GetTrackTags( DbTrack track ) {
            var retValue = new List<TrackTagInfo>();
            var tags = mTagManager.GetAssociatedTags( track.DbId );

            retValue.AddRange( from t in tags select new TrackTagInfo{ TagId = t.DbId, TagName = t.Name });

            return retValue;
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
                    mLog.LogException( $"GetTrackList for album: {request.AlbumId}", ex );

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

                                favoriteTracks.Where( t1 => retValue.TrackList.FirstOrDefault( t2 => t1.DbId.Equals( t2.TrackId )) == null ).ForEach( track => {
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
                    mLog.LogException( $"GetRatedTracks for artist: {request.ArtistId}", ex );

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

                        associatedTracks.AddRange( from a in associations let t = CreateTrackInfo( a ) where t != null select t );
                    });

                    // eliminate duplicate tracks
                    retValue.TrackList.AddRange( associatedTracks.GroupBy( t => t.TrackId ).Select( g => g.First()));
                    retValue.Success = true;
                }
                catch( Exception ex ) {
                    mLog.LogException( $"GetTaggedTracks for track: {request.TrackId}", ex );

                    retValue.ErrorMessage = ex.Message;
                }

                return retValue;
            });
        }

        public override Task<TrackListResponse> GetFavoriteTracks( TrackInfoEmpty request, ServerCallContext context ) {
            return Task.Run( () => {
                var retValue = new TrackListResponse { ArtistId = Constants.cDatabaseNullOid, AlbumId = Constants.cDatabaseNullOid };

                try {
                    using( var fileList = mTrackProvider.GetFavoriteTracks()) {
                        retValue.TrackList.AddRange( from file in fileList.List let track = TransformTrack( file ) where track != null select track );
                    }
                    retValue.Success = true;
                }
                catch( Exception ex ) {
                    mLog.LogException( "GetFavoriteTracks", ex );

                    retValue.Success = false;
                    retValue.ErrorMessage = ex.Message;
                }

                return retValue;
            });
        }

        private TrackInfo TransformTrack( DbTrack track ) {
            var retValue = default( TrackInfo );

            if( track != null ) {
                var artist = mArtistProvider.GetArtist( track.Artist );
                var album = mAlbumProvider.GetAlbum( track.Album );

                if(( artist != null ) &&
                   ( album != null )) {
                    retValue = TransformTrack( artist, album, track );
                }
            }

            return retValue;
        }

        private TrackInfo CreateTrackInfo( DbTagAssociation fromTag ) {
            var track = mTrackProvider.GetTrack( fromTag.ArtistId );

            if( track != null ) {
                var artist = mArtistProvider.GetArtist( track.Artist );
                var album = mAlbumProvider.GetAlbum( track.Album );

                if(( artist != null ) &&
                   ( album != null )) {
                    return TransformTrack( artist, album, track );
                }
            }

            return default;
        }

        public override Task<TrackListResponse> GetSimilarTracks( TrackSimilarRequest request, ServerCallContext context ) {
            return Task.Run( () => {
                var retValue = new TrackListResponse { ArtistId = Constants.cDatabaseNullOid, AlbumId = Constants.cDatabaseNullOid };

                try {
                    var track = mTrackProvider.GetTrack( request.TrackId );

                    if( track != null ) {
                        var searchItem = mSearchProvider.Search( eSearchItemType.Track, CreateSearchTerm( track.Name ), 1000 );

                        retValue.TrackList.AddRange( from item in searchItem let t = CreateTrackInfo( item ) where t != null select t );
                        retValue.Success = true;
                    }
                }
                catch( Exception ex ) {
                    mLog.LogException( $"GetSimilarTracks for track: {request.TrackId}", ex );

                    retValue.ErrorMessage = ex.Message;
                }

                return retValue;
            });
        }

        private TrackInfo CreateTrackInfo( SearchResultItem searchItem ) {
            if(( searchItem.Artist != null ) &&
               ( searchItem.Album != null ) &&
               ( searchItem.Track != null )) {
                return TransformTrack( searchItem.Artist, searchItem.Album, searchItem.Track );
            }

            return default;
        }

        // from PlaybackRelatedViewModel:
        private string CreateSearchTerm( string input ) {
            var retValue = DeleteText( input, '(', ')' );

            retValue = DeleteText( retValue, '[', ']' );
            retValue = retValue.Trim();
            retValue = $"\"{retValue}\"";

            return String.IsNullOrWhiteSpace( retValue ) ? input : retValue;
        }

        private string DeleteText( string source, char startCharacter, char endCharacter ) {
            var     retValue = source;
            bool    textDeleted;

            do {
                var startPosition = retValue.IndexOf( startCharacter );
                var endPosition = retValue.IndexOf( endCharacter );

                if(( startPosition >= 0 ) &&
                   ( endPosition > startPosition )) {
                    retValue = retValue.Remove( startPosition, endPosition - startPosition + 1 );

                    textDeleted = true;
                }
                else {
                    textDeleted = false;
                }
            } while( textDeleted );

            return retValue;
        }

        public override Task<TrackUpdateResponse> UpdateTrackTags( TrackUpdateRequest request, ServerCallContext context ) {
            return Task.Run( () => {
                var retValue = new TrackUpdateResponse();

                try {
                    var track = mTrackProvider.GetTrack( request.Track.TrackId );

                    if( track != null ) {
                        var tags = new List<DbTag>();
                        var tagList = mTagManager.GetUserTagList();

                        request.Track.Tags.ForEach( t => {
                            var dbTag = tagList.FirstOrDefault( tag => tag.DbId.Equals( t.TagId ));

                            if( dbTag != null ) {
                                tags.Add( dbTag );
                            }
                        });

                        mTagManager.UpdateAssociations( track, tags );

                        retValue.Success = true;
                    }
                }
                catch( Exception ex ) {
                    mLog.LogException( nameof( UpdateTrackTags ), ex );

                    retValue.ErrorMessage = ex.Message;
                }
                return retValue;
            });
        }

        public override Task<TrackUpdateResponse> UpdateTrackRatings( TrackUpdateRequest request, ServerCallContext context ) {
            return Task.Run( () => {
                var retValue = new TrackUpdateResponse();

                try {
                    using( var updater = mTrackProvider.GetTrackForUpdate( request.Track.TrackId )) {
                        if( updater.Item != null ) {
                            updater.Item.IsFavorite = request.Track.IsFavorite;
                            updater.Item.Rating = (short)request.Track.Rating;

                            updater.Update();

                            retValue.Track = request.Track;
                            retValue.Success = true;

                            mEventAggregator.PublishOnUIThread( new Events.UserTagsChanged());
                        }
                    }
                }
                catch( Exception ex ) {
                    mLog.LogException( nameof( UpdateTrackRatings ), ex );

                    retValue.ErrorMessage = ex.Message;
                }
                return retValue;
            });
        }
    }
}
