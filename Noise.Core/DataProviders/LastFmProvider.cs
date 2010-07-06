using System;
using System.Linq;
using CuttingEdge.Conditions;
using Lastfm.Services;
using Noise.Core.Database;
using Noise.Infrastructure.Dto;

namespace Noise.Core.DataProviders {
	public class LastFmProvider {
		private const string		cApiKey		= "2cc6cebb071ba39a2d6fa71fc60255e8";
		private const string		cApiSecret	= "e01705ce5fa579cc070811ebfe5206f0";
		private const int			cMaximumQueries = 5;

		private readonly IDatabaseManager	mDatabase;

		public LastFmProvider( IDatabaseManager databaseManager ) {
			mDatabase = databaseManager;
		}

		public void BuildMetaData() {
			try {
				var session = new Session( cApiKey, cApiSecret );

				UpdateArtistInfo( session );
			}
			catch( Exception ex ) {

			}
		}

		private void UpdateArtistInfo( Session session ) {
			var artists = from DbArtist artist in mDatabase.Database select artist;
			var queryCount = 0;

			foreach( var artist in artists ) {
				try {
					var parm = mDatabase.Database.CreateParameters();
					var artistId = mDatabase.Database.GetUid( artist );
					parm["artist"] = artistId;

					var bio = mDatabase.Database.ExecuteScalar( "SELECT DbBiography WHERE Artist = @artist", parm ) as DbBiography;
					var similarArtists = mDatabase.Database.ExecuteScalar( "SELECT DbSimilarItems WHERE AssociatedItem = @artist", parm ) as DbSimilarItems;
					var topAlbums = mDatabase.Database.ExecuteScalar( "SELECT DbTopItems WHERE AssociatedItem = @artist", parm ) as DbTopItems;

					if(( bio == null ) ||
					   ( similarArtists == null ) ||
					   ( topAlbums == null )) {
						var	artistSearch = Artist.Search( artist.Name, session );
						var	artistMatch = artistSearch.GetFirstMatch();

						if( artistMatch != null ) {
							var	tags = artistMatch.GetTopTags( 3 );
							if( tags.GetLength( 0 ) > 0 ) {
								artist.Genre = tags[0].Item.Name;
								mDatabase.Database.Store( artist );
							}

							if( bio == null ) {
								bio = new DbBiography( artistId );

								bio.Biography = artistMatch.Bio.getContent();
								bio.PublishedDate = artistMatch.Bio.GetPublishedDate();
								bio.ExpireDate = DateTime.Now.Date + new TimeSpan( 30, 0, 0, 0 );

								mDatabase.Database.Store( bio );

								if(!string.IsNullOrWhiteSpace( artistMatch.GetImageURL())) {
									new ImageDownloader( artistMatch.GetImageURL(), bio, ArtistImageDownloadComplete );
								}

								queryCount++;
							}

							if( similarArtists == null ) {
								var	sim = artistMatch.GetSimilar( 5 );
								similarArtists = new DbSimilarItems( artistId );

								similarArtists.SimilarItems = new string[sim.GetLength( 0 )];
								for( int index = 0; index < sim.GetLength( 0 ); index++ ) {
									similarArtists.SimilarItems[index] = sim[index].Name;
								}

								mDatabase.Database.Store( similarArtists );

								queryCount++;
							}

							if( topAlbums == null ) {
								var top = artistMatch.GetTopAlbums();
								topAlbums = new DbTopItems( artistId );

								topAlbums.TopItems = new string[top.GetLength( 0 ) > 5 ? 5 : top.GetLength( 0 )];
								for( int index = 0; index < topAlbums.TopItems.GetLength( 0 ); index++ ) {
									topAlbums.TopItems[index] = top[index].Item.Name;
								}

								mDatabase.Database.Store( topAlbums );

								queryCount++;
							}

							if( queryCount > cMaximumQueries ) {
								break;
							}
						}
					}
				}
				catch( Exception ex ) {

				}
			}
		}

		private void ArtistImageDownloadComplete( object parentObject, byte[] imageData ) {
			Condition.Requires( parentObject ).IsNotNull();
			Condition.Requires( imageData ).IsNotNull();

			var	bio = parentObject as DbBiography;
			if( bio != null ) {
				bio.ArtistImage = imageData;
				mDatabase.Database.Store( bio );
			}
		}
	}
}
