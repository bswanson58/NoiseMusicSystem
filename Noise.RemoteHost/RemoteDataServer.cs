using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using AutoMapper;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.RemoteDto;
using Noise.Infrastructure.RemoteHost;

namespace Noise.RemoteHost {
	[ServiceBehavior( InstanceContextMode = InstanceContextMode.Single )]
	public class RemoteDataServer : INoiseRemoteData {
		private	readonly IDataProvider	mDataProvider;
		private readonly ITagManager	mTagManager;

		public RemoteDataServer( IDataProvider dataProvider, ITagManager tagManager ) {
			mDataProvider = dataProvider;
			mTagManager = tagManager;
		}

		private string RetrieveGenre( long genreId ) {
			var retValue = "";

			if( genreId != Constants.cDatabaseNullOid ) {
				var genre = mTagManager.GetGenre( genreId );

				if( genre != null ){
					retValue = genre.Name;
				}
			}

			return( retValue );
		}

		private RoArtist TransformArtist( DbArtist dbArtist ) {
			var retValue = new RoArtist();

			Mapper.DynamicMap( dbArtist, retValue );
			retValue.Genre = RetrieveGenre( dbArtist.Genre );

			return( retValue );
		}

		public ArtistListResult GetArtistList() {
			var retValue = new ArtistListResult();

			try {
				using( var artistList = mDataProvider.GetArtistList()) {
					retValue.Artists = artistList.List.Select( TransformArtist ).ToArray();
					retValue.Success = true;
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "RemoteDataServer:GetArtistList", ex );

				retValue.ErrorMessage = ex.Message;
			}

			return( retValue );
		}

		private static RoArtistInfo TransformArtistInfo( DbArtist artist, ArtistSupportInfo supportInfo ) {
			var retValue = new RoArtistInfo();

			Mapper.DynamicMap( artist, retValue );
			Mapper.DynamicMap( supportInfo, retValue );

			if(( supportInfo.ArtistImage != null ) &&
			   ( supportInfo.ArtistImage.Image != null )) {
				retValue.ArtistImage = Convert.ToBase64String( supportInfo.ArtistImage.Image );
			}

			return( retValue );
		}

		public ArtistInfoResult GetArtistInfo( long artistId ) {
			var retValue = new ArtistInfoResult();

			try {
				var artist = mDataProvider.GetArtist( artistId );
				var artistInfo = mDataProvider.GetArtistSupportInfo( artistId );

				if(( artist != null ) &&
				   ( artistInfo != null )) {

					retValue.ArtistInfo = TransformArtistInfo( artist, artistInfo );
					retValue.Success = true;
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "RemoteDataServer:GetArtistInfo", ex );

				retValue.ErrorMessage = ex.Message;
			}

			return( retValue );
		}

		private RoAlbum TransformAlbum( DbAlbum dbAlbum ) {
			var retValue = new RoAlbum();

			Mapper.DynamicMap( dbAlbum, retValue );
			retValue.Genre = RetrieveGenre( dbAlbum.Genre );

			return( retValue );
		}

		public AlbumListResult GetAlbumList( long artistId ) {
			var retValue = new AlbumListResult { ArtistId = artistId };

			try {
				var	artist = mDataProvider.GetArtist( artistId );

				if( artist != null ) {
					using( var albumList = mDataProvider.GetAlbumList( artistId )) {
						retValue.Albums = albumList.List.Select( TransformAlbum ).ToArray();
						retValue.Success = true;
					}
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "RemoteDataServer:GetAlbumList", ex );

				retValue.ErrorMessage = ex.Message;
			}

			return( retValue );
		}

		private static Artwork SelectAlbumCover( AlbumSupportInfo info ) {
			Artwork	retValue = null;

			if( info != null ) {
				if(( info.AlbumCovers != null ) &&
				   ( info.AlbumCovers.GetLength( 0 ) > 0 )) {
					retValue = (( from Artwork artwork in info.AlbumCovers where artwork.IsUserSelection select artwork ).FirstOrDefault() ??
							    ( from Artwork artwork in info.AlbumCovers where artwork.Source == InfoSource.File select artwork ).FirstOrDefault() ??
							    ( from Artwork artwork in info.AlbumCovers where artwork.Source == InfoSource.Tag select artwork ).FirstOrDefault()) ??
								info.AlbumCovers[0];
				}

				if(( retValue == null ) &&
				   ( info.Artwork != null ) &&
				   ( info.Artwork.GetLength( 0 ) > 0 )) {
					retValue = ( from Artwork artwork in info.Artwork
								 where artwork.Name.IndexOf( "front", StringComparison.InvariantCultureIgnoreCase ) >= 0 select artwork ).FirstOrDefault();

					if(( retValue == null ) &&
					   ( info.Artwork.GetLength( 0 ) == 1 )) {
						retValue = info.Artwork[0];
					}
				}
			}

			return( retValue );
		}

		private static RoAlbumInfo TransformAlbumInfo( DbAlbum album, AlbumSupportInfo supportInfo ) {
			var retValue = new RoAlbumInfo();

			Mapper.DynamicMap( album, retValue );
			Mapper.DynamicMap( supportInfo, retValue );

			var	artwork = SelectAlbumCover( supportInfo );
			if( artwork != null ) {
				retValue.AlbumCover = Convert.ToBase64String( artwork.Image );
			}

			return( retValue );
		}

		public AlbumInfoResult GetAlbumInfo( long albumId ) {
			var retValue = new AlbumInfoResult();

			try {
				var album = mDataProvider.GetAlbum( albumId );
				var supportInfo = mDataProvider.GetAlbumSupportInfo( albumId );

				if(( album != null ) &&
				   ( supportInfo != null )) {
					retValue.AlbumInfo = TransformAlbumInfo( album, supportInfo );

					retValue.Success = true;
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "RemoteDataServer:GetAlbumInfo", ex );

				retValue.ErrorMessage = ex.Message;
			}

			return( retValue );
		}

		private static RoTrack TransformTrack( DbTrack dbTrack ) {
			var retValue = new RoTrack();

			Mapper.DynamicMap( dbTrack, retValue );

			return( retValue );
		}

		public TrackListResult GetTrackList( long albumId ) {
			var retValue = new TrackListResult();

			try {
				var album = mDataProvider.GetAlbum( albumId );

				retValue.ArtistId = album.Artist;
				retValue.AlbumId = album.DbId;

				using( var trackList = mDataProvider.GetTrackList( albumId )) {
					retValue.Tracks = trackList.List.Select( TransformTrack ).ToArray();
					foreach( var track in retValue.Tracks ) {
						track.ArtistId = retValue.ArtistId;
					}
					retValue.Success = true;
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "RemoteDataServer:GetTrackList", ex );

				retValue.ErrorMessage = ex.Message;
			}

			return( retValue );
		}

		public FavoriteListResult GetFavoriteList() {
			var retValue = new FavoriteListResult();
			var favoritesList = new List<RoFavorite>();

			try {
			using( var list = mDataProvider.GetFavoriteArtists()) {
				favoritesList.AddRange( list.List.Select( artist => new RoFavorite( artist ) ) );
			}
			using( var list = mDataProvider.GetFavoriteAlbums()) {
				favoritesList.AddRange( from album in list.List
				                        let artist = mDataProvider.GetArtistForAlbum( album )
				                        select new RoFavorite( artist, album ));
			}
			using( var list = mDataProvider.GetFavoriteTracks()) {
				favoritesList.AddRange( from track in list.List
				                        let album = mDataProvider.GetAlbumForTrack( track )
				                        let artist = mDataProvider.GetArtistForAlbum( album )
				                        select new RoFavorite( artist, album, track ));
			}

			retValue.Favorites = favoritesList.ToArray();
			retValue.Success = true;
				
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "RemoteDataServer:GetFavoriteList", ex );

				retValue.ErrorMessage = ex.Message;
			}

			return( retValue );
		}
	}
}
