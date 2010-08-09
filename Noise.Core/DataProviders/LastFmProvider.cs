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
				var parms = mDatabase.Database.CreateParameters();

				parms["artistId"] = artistId;
				parms["artistImage"] = ContentType.ArtistPrimaryImage;

				var	artwork = mDatabase.Database.ExecuteScalar( "SELECT DbArtwork WHERE AssociatedItem = @artistId AND ContentType = @artistImage", parms ) as DbArtwork;
				var bio = ( from DbTextInfo info in mDatabase.Database where info.AssociatedItem == artistId && info.ContentType == ContentType.Biography select info ).FirstOrDefault();
				var similarArtists = ( from DbAssociatedItems item in mDatabase.Database where item.AssociatedItem == artistId && item.ContentType == ContentType.SimilarArtists select item ).FirstOrDefault();
				var topAlbums = ( from DbAssociatedItems item in mDatabase.Database where item.AssociatedItem == artistId && item.ContentType == ContentType.TopAlbums select  item ).FirstOrDefault();
				if( artwork == null ) {
					artwork = new DbArtwork( artistId, ContentType.ArtistPrimaryImage );
				}
				if( bio == null ) {
					bio = new DbTextInfo( artistId, ContentType.Biography );
				}

				if( similarArtists == null ) {
					similarArtists = new DbAssociatedItems( artistId, ContentType.SimilarArtists );
				}

				if( topAlbums == null ) {
					topAlbums = new DbAssociatedItems( artistId, ContentType.TopAlbums );
				}

				artwork.UpdateExpiration();
				mDatabase.Database.Store( artwork );

				bio.UpdateExpiration();
				similarArtists.UpdateExpiration();
				topAlbums.UpdateExpiration();

				var	artistSearch = Artist.Search( artist.Name, mSession );
				var	artistMatch = artistSearch.GetFirstMatch();

				if( artistMatch != null ) {
					var	tags = artistMatch.GetTopTags( 3 );
					if( tags.GetLength( 0 ) > 0 ) {
						artist.Genre = tags[0].Item.Name;
						mDatabase.Database.Store( artist );
					}

					bio.Text = artistMatch.Bio.getContent();
					bio.Source = InfoSource.External;
					bio.IsContentAvailable = true;

					var imageUrl = artistMatch.GetImageURL();
					if(!string.IsNullOrWhiteSpace( imageUrl )) {
						new ImageDownloader( imageUrl, artistId, ArtistImageDownloadComplete );
					}

					var	sim = artistMatch.GetSimilar( 5 );
					similarArtists.Items = new string[sim.GetLength( 0 )];
					for( int index = 0; index < sim.GetLength( 0 ); index++ ) {
						similarArtists.Items[index] = sim[index].Name;
					}
					similarArtists.IsContentAvailable = true;

					var top = artistMatch.GetTopAlbums();
					topAlbums.Items = new string[top.GetLength( 0 ) > 5 ? 5 : top.GetLength( 0 )];
					for( int index = 0; index < topAlbums.Items.GetLength( 0 ); index++ ) {
						topAlbums.Items[index] = top[index].Item.Name;
					}
					topAlbums.IsContentAvailable = true;

					mLog.LogInfo( "Updating LastFm artist: {0}", artist.Name );
				}

				mDatabase.Database.Store( bio );
				mDatabase.Database.Store( similarArtists );
				mDatabase.Database.Store( topAlbums );
			}
			catch( Exception ex ) {
				mLog.LogException( "LastFmProvider UpdateArtistInfo:", ex );
			}
		}

		private void ArtistImageDownloadComplete( long parentId, byte[] imageData ) {
			Condition.Requires( imageData ).IsNotNull();

			var parms = mDatabase.Database.CreateParameters();

			parms["artistId"] = parentId;
			parms["artistImage"] = ContentType.ArtistPrimaryImage;

			var	artwork = mDatabase.Database.ExecuteScalar( "SELECT DbArtwork WHERE AssociatedItem = @artistId AND ContentType = @artistImage", parms ) as DbArtwork;
			if( artwork == null ) {
				artwork = new DbArtwork( parentId, ContentType.ArtistPrimaryImage );
			}

			artwork.Image = imageData;
			artwork.Source = InfoSource.External;
			artwork.IsContentAvailable = true;
			artwork.UpdateExpiration();

			mDatabase.Database.Store( artwork );
		}
	}
}
