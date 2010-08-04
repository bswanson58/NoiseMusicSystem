using System;
using System.ComponentModel.Composition;
using System.Linq;
using CuttingEdge.Conditions;
using Lastfm.Services;
using Microsoft.Practices.Unity;
using Noise.Core.Database;
using Noise.Core.DataBuilders;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.DataProviders {
	internal abstract class BaseLastFmProvider : IContentProvider {
		public	abstract ContentType	ContentType { get; }

		[Import]
		protected IUnityContainer	Container { get; set; }

		public TimeSpan ExpirationPeriod {
			get { return( new TimeSpan( 30, 0, 0, 0 )); }
		}

		public bool CanUpdateArtist {
			get{ return( true ); }
		}

		public bool CanUpdateAlbum {
			get{ return( false ); }
		}

		public bool CanUpdateTrack {
			get{ return( false ); }
		}

		public void UpdateContent( DbArtist forArtist ) {
			var provider = new LastFmProvider( Container );

			provider.UpdateArtist( forArtist );
		}

		public void UpdateContent( DbAlbum forAlbum ) {
			throw new NotImplementedException();
		}

		public void UpdateContent( DbTrack forTrack ) {
			throw new NotImplementedException();
		}
	}

	[Export( typeof( IContentProvider ))]
	internal class BiographyProvider : BaseLastFmProvider {
		public override ContentType ContentType {
			get { return( ContentType.Biography ); }
		}
	}

	[Export( typeof( IContentProvider ))]
	internal class SimilarArtistsProvider : BaseLastFmProvider {
		public override ContentType ContentType {
			get { return( ContentType.SimilarArtists ); }
		}
	}

	[Export( typeof( IContentProvider ))]
	internal class TopAlbumsProvider : BaseLastFmProvider {
		public override ContentType ContentType {
			get { return( ContentType.TopAlbums ); }
		}
	}

	public class LastFmProvider {
		private const string		cApiKey		= "2cc6cebb071ba39a2d6fa71fc60255e8";
		private const string		cApiSecret	= "e01705ce5fa579cc070811ebfe5206f0";

		private readonly IDatabaseManager	mDatabase;
		private readonly ILog				mLog;
		private readonly Session			mSession;

		public LastFmProvider( IUnityContainer container ) {
			mDatabase = container.Resolve<IDatabaseManager>();

			mLog = new Log();

			try {
				mSession = new Session( cApiKey, cApiSecret );
			}
			catch( Exception ex ) {
				mLog.LogException( "LastFmProvider creating Session:", ex );
			}
		}

		public void UpdateArtist( DbArtist artist ) {
			try {
				var artistId = mDatabase.Database.GetUid( artist );
				var bio = ( from DbTextInfo info in mDatabase.Database where info.AssociatedItem == artistId && info.ContentType == ContentType.Biography select info ).FirstOrDefault();
				var similarArtists = ( from DbSimilarItems item in mDatabase.Database where item.AssociatedItem == artistId && item.ContentType == ContentType.SimilarArtists select item ).FirstOrDefault();
				var topAlbums = ( from DbTopItems item in mDatabase.Database where item.AssociatedItem == artistId && item.ContentType == ContentType.TopAlbums select  item ).FirstOrDefault();

				if(( bio == null ) ||
				   ( similarArtists == null ) ||
				   ( topAlbums == null )) {
					var	artistSearch = Artist.Search( artist.Name, mSession );
					var	artistMatch = artistSearch.GetFirstMatch();

					if( artistMatch != null ) {
						var	tags = artistMatch.GetTopTags( 3 );
						if( tags.GetLength( 0 ) > 0 ) {
							artist.Genre = tags[0].Item.Name;
							mDatabase.Database.Store( artist );
						}

						if( bio == null ) {
							bio = new DbTextInfo( artistId, ContentType.Biography ) { Text = artistMatch.Bio.getContent(),
																					  Source = InfoSource.External,
																					  IsContentAvailable = true };
							mDatabase.Database.Store( bio );

							var imageUrl = artistMatch.GetImageURL();
							if(!string.IsNullOrWhiteSpace( imageUrl )) {
								new ImageDownloader( imageUrl, artistId, ArtistImageDownloadComplete );
							}
						}

						if( similarArtists == null ) {
							var	sim = artistMatch.GetSimilar( 5 );
							similarArtists = new DbSimilarItems( artistId, ContentType.SimilarArtists ) { IsContentAvailable = true };

							similarArtists.SimilarItems = new string[sim.GetLength( 0 )];
							for( int index = 0; index < sim.GetLength( 0 ); index++ ) {
								similarArtists.SimilarItems[index] = sim[index].Name;
							}

							mDatabase.Database.Store( similarArtists );
						}

						if( topAlbums == null ) {
							var top = artistMatch.GetTopAlbums();
							topAlbums = new DbTopItems( artistId, ContentType.TopAlbums ) { IsContentAvailable = true };

							topAlbums.TopItems = new string[top.GetLength( 0 ) > 5 ? 5 : top.GetLength( 0 )];
							for( int index = 0; index < topAlbums.TopItems.GetLength( 0 ); index++ ) {
								topAlbums.TopItems[index] = top[index].Item.Name;
							}

							mDatabase.Database.Store( topAlbums );
						}

						mLog.LogInfo( "Updating LastFm artist: {0}", artist.Name );
					}
					else {
						// If there is no match, store blank entries in the database to prevent checking again until the expiration date.
						if( bio == null ) {
							bio = new DbTextInfo( artistId, ContentType.Biography );
						}
						else {
							bio.UpdateExpiration();
						}
						mDatabase.Database.Store( bio );

						if( similarArtists == null ) {
							similarArtists = new DbSimilarItems( artistId, ContentType.SimilarArtists );
						}
						else {
							similarArtists.UpdateExpiration();
						}
						mDatabase.Database.Store( similarArtists );

						if( topAlbums == null ) {
							topAlbums = new DbTopItems( artistId, ContentType.TopAlbums );
						}
						else {
							topAlbums.UpdateExpiration();
						}
						mDatabase.Database.Store( topAlbums );
					}
				}
			}
			catch( Exception ex ) {
				mLog.LogException( "LastFmProvider UpdateArtistInfo:", ex );
			}
		}

		private void ArtistImageDownloadComplete( long parentId, byte[] imageData ) {
			Condition.Requires( imageData ).IsNotNull();

			var image = new DbArtwork( parentId, ContentType.ArtistPrimaryImage ) { Source = InfoSource.External,
																					Image = imageData };
			mDatabase.Database.Store( image );
		}
	}
}
