using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using AutoMapper;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.RemoteDto;
using Noise.Infrastructure.RemoteHost;

namespace Noise.RemoteHost {
	[ServiceBehavior( InstanceContextMode = InstanceContextMode.Single )]
	public class RemoteDataServer : INoiseRemoteData {
		private readonly IArtistProvider		mArtistProvider;
		private readonly IAlbumProvider			mAlbumProvider;
		private readonly ITrackProvider			mTrackProvider;
		private readonly IPlayHistoryProvider	mPlayHistory;
		private readonly IMetadataManager		mMetadataManager;
		private readonly ITagManager			mTagManager;
		private readonly Random					mRandom;

		public RemoteDataServer( IArtistProvider artistProvider, IAlbumProvider albumProvider, ITrackProvider trackProvider,
								 IPlayHistoryProvider playHistory, ITagManager tagManager, IMetadataManager metadataManager ) {
			mArtistProvider = artistProvider;
			mAlbumProvider = albumProvider;
			mTrackProvider = trackProvider;
			mPlayHistory = playHistory;
			mMetadataManager = metadataManager;
			mTagManager = tagManager;
			mRandom = new Random( DateTime.Now.Millisecond );
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
				using( var artistList = mArtistProvider.GetArtistList()) {
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

		private static RoArtistInfo TransformArtistInfo( DbArtist artist, IArtistMetadata artistMetadata, Artwork artistImage ) {
			var retValue = new RoArtistInfo();

			Mapper.DynamicMap( artist, retValue );

			retValue.Biography = artistMetadata.GetMetadata( eMetadataType.Biography );
			retValue.BandMembers = artistMetadata.GetMetadataArray( eMetadataType.BandMembers ).ToArray();
			retValue.SimilarArtists = artistMetadata.GetMetadataArray( eMetadataType.SimilarArtists ).ToArray();
			retValue.TopAlbums = artistMetadata.GetMetadataArray( eMetadataType.TopAlbums ).ToArray();
			retValue.TopTracks = artistMetadata.GetMetadataArray( eMetadataType.TopTracks ).ToArray();
			retValue.Website = artistMetadata.GetMetadata( eMetadataType.WebSite );
			
			if( artistImage != null ) {
				retValue.ArtistImage = Convert.ToBase64String( artistImage.Image );
			}

			return( retValue );
		}

		protected int NextRandom( int maxValue ) {
			return( mRandom.Next( maxValue ));
		}

		public ArtistInfoResult GetArtistInfo( long artistId ) {
			var retValue = new ArtistInfoResult();

			try {
				var artist = mArtistProvider.GetArtist( artistId );

				if( artist != null ) {
					var artistMetadata = mMetadataManager.GetArtistMetadata( artist.Name );
					var artistImage = mMetadataManager.GetArtistArtwork( artist.Name );

					retValue.ArtistInfo = TransformArtistInfo( artist, artistMetadata, artistImage );

					if( retValue.ArtistInfo.TopTracks.Any()) {
						var allTracks = mTrackProvider.GetTrackList( artist );
						var trackIds = new List<long>();

						foreach( var trackName in retValue.ArtistInfo.TopTracks ) {
							string	name = trackName;
							var		trackList = allTracks.List.Where( t => t.Name.Equals( name, StringComparison.CurrentCultureIgnoreCase )).ToList();

							if( trackList.Any()) {
								var selectedTrack = trackList.Skip( NextRandom( trackList.Count - 1 )).Take( 1 ).FirstOrDefault();

								if( selectedTrack != null ) {
									trackIds.Add( selectedTrack.DbId );
								}
							}
						}

						retValue.ArtistInfo.TopTrackIds = trackIds.ToArray();
					}

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
				var	artist = mArtistProvider.GetArtist( artistId );

				if( artist != null ) {
					using( var albumList = mAlbumProvider.GetAlbumList( artistId )) {
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
				var album = mAlbumProvider.GetAlbum( albumId );
				var supportInfo = mAlbumProvider.GetAlbumSupportInfo( albumId );

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

		public TrackListResult GetTrackList( long albumId ) {
			var retValue = new TrackListResult();

			try {
				var album = mAlbumProvider.GetAlbum( albumId );

				if( album != null ) {
					var artist = mArtistProvider.GetArtist( album.Artist );

					using( var trackList = mTrackProvider.GetTrackList( albumId )) {
						retValue.Tracks = trackList.List.Select( track => new RoTrack( artist, album, track )).ToArray();

						retValue.Success = true;
					}
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
			using( var list = mArtistProvider.GetFavoriteArtists()) {
				favoritesList.AddRange( list.List.Select( artist => new RoFavorite( artist ) ) );
			}
			using( var list = mAlbumProvider.GetFavoriteAlbums()) {
				favoritesList.AddRange( from album in list.List
				                        let artist = mArtistProvider.GetArtistForAlbum( album )
				                        select new RoFavorite( artist, album ));
			}
			using( var list = mTrackProvider.GetFavoriteTracks()) {
				favoritesList.AddRange( from track in list.List
				                        let album = mAlbumProvider.GetAlbumForTrack( track )
				                        let artist = mArtistProvider.GetArtistForAlbum( album )
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

		public ArtistTracksResult GetArtistTrackList( long artistId ) {
			var retValue = new ArtistTracksResult { ArtistId = artistId };

			try {
				var artist = mArtistProvider.GetArtist( artistId );

				if( artist != null ) {
					var trackSet = new Dictionary<string, RoArtistTrack>();
					var albumSets = new Dictionary<string, List<RoTrackReference>>();
					
					using( var trackList = mTrackProvider.GetTrackList( artist )) {
						foreach( var track in trackList.List ) {
							var roTrack = new RoTrackReference { TrackId = track.DbId,
																 Duration = track.DurationMilliseconds,
																 TrackNumber = track.TrackNumber,
																 VolumeName = track.VolumeName,
																 AlbumId = track.Album };

							if( trackSet.ContainsKey( track.Name )) {
								var albums = albumSets[track.Name];

								albums.Add( roTrack );
							}
							else {
								trackSet.Add( track.Name, new RoArtistTrack { TrackName = track.Name });
								albumSets.Add( track.Name, new List<RoTrackReference>{ roTrack });
							}
						}
					}

					foreach( var track in trackSet.Values ) {
						var albumList = albumSets[track.TrackName];

						track.Tracks = albumList.ToArray();
					}

					retValue.Tracks = trackSet.Values.ToArray();
					retValue.Success = true;
				}
				else {
					retValue.ErrorMessage = "Artist could not be located.";
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "RemoteDataServer:GetArtistTrackList", ex );

				retValue.ErrorMessage = ex.Message;
			}

			return( retValue );
		}

		public LibraryAdditionsListResult GetNewTracks() {
			var retValue = new LibraryAdditionsListResult();

			try {
				var	horizonCount = 200;
				var	horizonTime = DateTime.Now - new TimeSpan( 90, 0, 0, 0 );
				var configuration = NoiseSystemConfiguration.Current.RetrieveConfiguration<ExplorerConfiguration>( ExplorerConfiguration.SectionName );

				if( configuration != null ) {
					horizonCount = (int)configuration.NewAdditionsHorizonCount;
					horizonTime = DateTime.Now - new TimeSpan( configuration.NewAdditionsHorizonDays, 0, 0, 0 );
				}

				var	trackList = new List<RoTrack>();

				using( var additions = mTrackProvider.GetNewlyAddedTracks()) {
					if(( additions != null ) &&
					   ( additions.List != null )) {
						UInt32	count = 0;

						foreach( var track in additions.List ) {
							if(( count < horizonCount ) &&
							   ( track.DateAdded > horizonTime )) {
								var artist = mArtistProvider.GetArtist( track.Artist );
								var album = mAlbumProvider.GetAlbum( track.Album );

								if(( artist != null ) &&
								   ( album != null )) {
									trackList.Add( new RoTrack( artist, album, track ));
								}
							}
							else {
								break;
							}

							count++;
						}
					}
				}

				if( trackList.Count > 0 ) {
					retValue.NewTracks = trackList.ToArray();
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "RemoteDataServer:GetNewTracks", ex );

				retValue.ErrorMessage = ex.Message;
			}

			return( retValue );
		}

		public PlayHistoryListResult GetPlayHistory() {
			var retValue = new PlayHistoryListResult();

			try {
				using( var history = mPlayHistory.GetPlayHistoryList()) {
					retValue.PlayHistory = ( from historyItem in history.List orderby historyItem.PlayedOn descending
											 let track = mTrackProvider.GetTrack( historyItem.TrackId ) 
											 where track != null 
												let album = mAlbumProvider.GetAlbum( track.Album ) 
												let artist = mArtistProvider.GetArtist( track.Artist ) 
											 where ( artist != null ) && ( album != null )
												select new RoPlayHistory( artist, album, track, historyItem.PlayedOnTicks ))
											.Take( 30 ).ToArray();
					retValue.Success = true;
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "RemoteDataServer:GetPlayHistory", ex );

				retValue.ErrorMessage = ex.Message;
			}

			return( retValue );
		}

		public BaseResult SetTrackRating( long trackId, int rating, bool isFavorite ) {
			var retValue = new BaseResult();

			try {
				using( var dbTrack = mTrackProvider.GetTrackForUpdate( trackId )) {
					if( dbTrack.Item != null ) {
						dbTrack.Item.Rating = CoerceRating( rating );
						dbTrack.Item.IsFavorite = isFavorite;

						dbTrack.Update();
						retValue.Success = true;
					}
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "RemoteDataServer:SetTrackRating", ex );

				retValue.ErrorMessage = ex.Message;
			}

			return( retValue );
		}

		public BaseResult SetAlbumRating( long albumId, int rating, bool isFavorite ) {
			var retValue = new BaseResult();

			try {
				using( var dbAlbum = mAlbumProvider.GetAlbumForUpdate( albumId )) {
					if( dbAlbum.Item != null ) {
						dbAlbum.Item.Rating = CoerceRating( rating );
						dbAlbum.Item.IsFavorite = isFavorite;

						dbAlbum.Update();
						retValue.Success = true;
					}
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "RemoteDataServer:SetAlbumRating", ex );

				retValue.ErrorMessage = ex.Message;
			}

			return( retValue );
		}

		public BaseResult SetArtistRating( long artistId, int rating, bool isFavorite ) {
			var retValue = new BaseResult();

			try {
				using( var dbArtist = mArtistProvider.GetArtistForUpdate( artistId )) {
					if( dbArtist.Item != null ) {
						dbArtist.Item.Rating = CoerceRating( rating );
						dbArtist.Item.IsFavorite = isFavorite;

						dbArtist.Update();
						retValue.Success = true;
					}
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "RemoteDataServer:SetArtistRating", ex );

				retValue.ErrorMessage = ex.Message;
			}

			return( retValue );
		}

		private Int16 CoerceRating( int rating ) {
			Int16	retValue = 0;

			if(( rating >= -1 ) &&
			   ( rating <= 5 )) {
				retValue = (Int16)rating;
			}

			return( retValue );
		}
	}
}
