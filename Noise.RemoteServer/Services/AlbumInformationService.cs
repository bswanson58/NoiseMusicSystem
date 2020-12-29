using System;
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
    class AlbumInformationService : AlbumInformation.AlbumInformationBase {
        private readonly IEventAggregator   mEventAggregator;
        private readonly IArtistProvider    mArtistProvider;
        private readonly IAlbumProvider     mAlbumProvider;
        private readonly ITagManager		mTagManager;
        private readonly INoiseLog          mLog;

        public AlbumInformationService( IArtistProvider artistProvider, IAlbumProvider albumProvider, ITagManager tagManager, INoiseLog log,
                                        IEventAggregator eventAggregator ) {
            mArtistProvider = artistProvider;
            mAlbumProvider = albumProvider;
            mTagManager = tagManager;
            mEventAggregator = eventAggregator;
            mLog = log;
        }

        private string RetrieveGenre( long genreId ) {
            var retValue = String.Empty;

            if( genreId != Constants.cDatabaseNullOid ) {
                var genre = mTagManager.GetGenre( genreId );

                if( genre != null ){
                    retValue = genre.Name;
                }
            }

            return( retValue );
        }

        private AlbumInfo TransformAlbum( DbArtist artist, DbAlbum dbAlbum ) {
            return new AlbumInfo {
                AlbumId = dbAlbum.DbId, ArtistId = dbAlbum.Artist,
                AlbumName = dbAlbum.Name, ArtistName = artist.Name,
                TrackCount = dbAlbum.TrackCount, PublishedYear = dbAlbum.PublishedYear,
                Rating = dbAlbum.Rating, IsFavorite = dbAlbum.IsFavorite,
                Genre = RetrieveGenre( dbAlbum.Genre )
            };
        }

        public override Task<AlbumListResponse> GetAlbumList( AlbumListRequest request, ServerCallContext context ) {
            return Task.Run( () => {
                var retValue = new AlbumListResponse { ArtistId = request.ArtistId };

                try {
                    var	artist = mArtistProvider.GetArtist( request.ArtistId );

                    if( artist != null ) {
                        using( var albumList = mAlbumProvider.GetAlbumList( request.ArtistId )) {
                            retValue.AlbumList.AddRange( albumList.List.Select( album => TransformAlbum( artist, album )));
                            retValue.Success = true;
                        }
                    }
                }
                catch( Exception ex ) {
                    mLog.LogException( $"GetAlbumList for {request.ArtistId}", ex );

                    retValue.ErrorMessage = ex.Message;
                }

                return retValue;
            });
        }

        public override Task<AlbumUpdateResponse> UpdateAlbumRatings( AlbumUpdateRequest request, ServerCallContext context ) {
            return Task.Run( () => {
                var retValue = new AlbumUpdateResponse();

                try {
                    using( var updater = mAlbumProvider.GetAlbumForUpdate( request.Album.AlbumId )) {
                        if( updater.Item != null ) {
                            updater.Item.IsFavorite = request.Album.IsFavorite;
                            updater.Item.Rating = (short)request.Album.Rating;

                            updater.Update();

                            retValue.Album = request.Album;
                            retValue.Success = true;

                            mEventAggregator.PublishOnUIThread( new Events.AlbumUserUpdate( updater.Item.DbId ));
                        }
                    }
                }
                catch( Exception ex ) {
                    mLog.LogException( "UpdateAlbum", ex );

                    retValue.ErrorMessage = ex.Message;
                }
                return retValue;
            });
        }

        public override Task<AlbumListResponse> GetFavoriteAlbums( AlbumInfoEmpty request, ServerCallContext context ) {
            return Task.Run( () => {
                var retValue = new AlbumListResponse();

                try {
                    using( var albums = mAlbumProvider.GetFavoriteAlbums()) {
                        albums.List.ForEach( album => {
                            var artist = mArtistProvider.GetArtist( album.Artist );

                            if( artist != null ) {
                                retValue.AlbumList.Add( TransformAlbum( artist, album ));
                            }
                        });
                    }

                    retValue.Success = true;
                }
                catch( Exception ex ) {
                    mLog.LogException( nameof( GetFavoriteAlbums ), ex );

                    retValue.ErrorMessage = ex.Message;
                }

                return retValue;
            });
        }
    }
}
