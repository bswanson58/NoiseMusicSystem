using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using AutoMapper;
using Microsoft.Practices.Unity;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.RemoteDto;
using Noise.Infrastructure.RemoteHost;

namespace Noise.RemoteHost {
	[ServiceBehavior( InstanceContextMode = InstanceContextMode.Single )]
	public class RemoteDataServer : INoiseRemoteData {
		private readonly IUnityContainer	mContainer;
		private readonly ILog				mLog;
		private	readonly INoiseManager		mNoiseManager;

		public RemoteDataServer( IUnityContainer container ) {
			mContainer = container;
			mLog = mContainer.Resolve<ILog>();
			mNoiseManager = mContainer.Resolve<INoiseManager>();
		}

		private static RoArtist TransformArtist( DbArtist dbArtist ) {
			var retValue = new RoArtist();

			Mapper.DynamicMap( dbArtist, retValue );

			return( retValue );
		}

		public ArtistListResult GetArtistList() {
			var retValue = new ArtistListResult();

			try {
				using( var artistList = mNoiseManager.DataProvider.GetArtistList()) {
					retValue.Artists = artistList.List.Select( TransformArtist ).ToArray();
					retValue.Success = true;
				}
			}
			catch( Exception ex ) {
				mLog.LogException( "RemoteDataServer:GetArtistList", ex );

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
				var artist = mNoiseManager.DataProvider.GetArtist( artistId );
				var artistInfo = mNoiseManager.DataProvider.GetArtistSupportInfo( artistId );

				if(( artist != null ) &&
				   ( artistInfo != null )) {

					retValue.ArtistInfo = TransformArtistInfo( artist, artistInfo );
					retValue.Success = true;
				}
			}
			catch( Exception ex ) {
				mLog.LogException( "RemoteDataServer:GetArtistInfo", ex );

				retValue.ErrorMessage = ex.Message;
			}

			return( retValue );
		}

		private static RoAlbum TransformAlbum( DbAlbum dbAlbum ) {
			var retValue = new RoAlbum();

			Mapper.DynamicMap( dbAlbum, retValue );

			return( retValue );
		}

		public AlbumListResult GetAlbumList( long artistId ) {
			var retValue = new AlbumListResult { ArtistId = artistId };

			try {
				var	artist = mNoiseManager.DataProvider.GetArtist( artistId );

				if( artist != null ) {
					using( var albumList = mNoiseManager.DataProvider.GetAlbumList( artistId )) {
						retValue.Albums = albumList.List.Select( TransformAlbum ).ToArray();
						retValue.Success = true;
					}
				}
			}
			catch( Exception ex ) {
				mLog.LogException( "RemoteDataServer:GetAlbumList", ex );

				retValue.ErrorMessage = ex.Message;
			}

			return( retValue );
		}

		private static DbArtwork SelectAlbumCover( AlbumSupportInfo info ) {
			DbArtwork	retValue = null;

			if( info != null ) {
				if(( info.AlbumCovers != null ) &&
				   ( info.AlbumCovers.GetLength( 0 ) > 0 )) {
					retValue = (( from DbArtwork artwork in info.AlbumCovers where artwork.IsUserSelection select artwork ).FirstOrDefault() ??
							    ( from DbArtwork artwork in info.AlbumCovers where artwork.Source == InfoSource.File select artwork ).FirstOrDefault() ??
							    ( from DbArtwork artwork in info.AlbumCovers where artwork.Source == InfoSource.Tag select artwork ).FirstOrDefault()) ??
								info.AlbumCovers[0];
				}

				if(( retValue == null ) &&
				   ( info.Artwork != null ) &&
				   ( info.Artwork.GetLength( 0 ) > 0 )) {
					retValue = ( from DbArtwork artwork in info.Artwork
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
				var album = mNoiseManager.DataProvider.GetAlbum( albumId );
				var supportInfo = mNoiseManager.DataProvider.GetAlbumSupportInfo( albumId );

				if(( album != null ) &&
				   ( supportInfo != null )) {
					retValue.AlbumInfo = TransformAlbumInfo( album, supportInfo );

					retValue.Success = true;
				}
			}
			catch( Exception ex ) {
				mLog.LogException( "RemoteDataServer:GetAlbumInfo", ex );

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
				var album = mNoiseManager.DataProvider.GetAlbum( albumId );

				retValue.ArtistId = album.Artist;
				retValue.AlbumId = album.DbId;

				using( var trackList = mNoiseManager.DataProvider.GetTrackList( albumId )) {
					retValue.Tracks = trackList.List.Select( TransformTrack ).ToArray();
					foreach( var track in retValue.Tracks ) {
						track.ArtistId = retValue.ArtistId;
					}
					retValue.Success = true;
				}
			}
			catch( Exception ex ) {
				mLog.LogException( "RemoteDataServer:GetTrackList", ex );

				retValue.ErrorMessage = ex.Message;
			}

			return( retValue );
		}

		public FavoriteListResult GetFavoriteList() {
			var retValue = new FavoriteListResult();
			var favoritesList = new List<RoFavorite>();

			try {
			using( var list = mNoiseManager.DataProvider.GetFavoriteArtists()) {
				favoritesList.AddRange( list.List.Select( artist => new RoFavorite( artist ) ) );
			}
			using( var list = mNoiseManager.DataProvider.GetFavoriteAlbums()) {
				favoritesList.AddRange( from album in list.List
				                        let artist = mNoiseManager.DataProvider.GetArtistForAlbum( album )
				                        select new RoFavorite( artist, album ));
			}
			using( var list = mNoiseManager.DataProvider.GetFavoriteTracks()) {
				favoritesList.AddRange( from track in list.List
				                        let album = mNoiseManager.DataProvider.GetAlbumForTrack( track )
				                        let artist = mNoiseManager.DataProvider.GetArtistForAlbum( album )
				                        select new RoFavorite( artist, album, track ));
			}

			retValue.Favorites = favoritesList.ToArray();
			retValue.Success = true;
				
			}
			catch( Exception ex ) {
				mLog.LogException( "RemoteDataServer:GetFavoriteList", ex );

				retValue.ErrorMessage = ex.Message;
			}

			return( retValue );
		}
	}
}
