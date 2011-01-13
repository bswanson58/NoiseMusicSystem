using System;
using System.Collections.Generic;
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

		public void UpdateContent( IDatabase database, DbArtist forArtist ) {
			var provider = new LastFmProvider( Container );

			provider.UpdateArtist( database, forArtist );
		}

		public void UpdateContent( IDatabase database, DbAlbum forAlbum ) {
			throw new NotImplementedException();
		}

		public void UpdateContent( IDatabase database, DbTrack forTrack ) {
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
		private readonly IUnityContainer	mContainer;
		private readonly ITagManager		mTagManager;
		private readonly ILog				mLog;
		private readonly Session			mSession;

		public LastFmProvider( IUnityContainer container ) {
			mContainer = container;
			mLog = container.Resolve<ILog>();

			var noiseManager = mContainer.Resolve<INoiseManager>();
			mTagManager = noiseManager.TagManager;

			try {
				var licenseManager = mContainer.Resolve<ILicenseManager>();
				if( licenseManager.Initialize( Constants.LicenseKeyFile )) {
					var key = licenseManager.RetrieveKey( LicenseKeys.LastFm );

					if( key != null ) {
						mSession = new Session( key.Name, key.Key );
					}
				}
			}
			catch( Exception ex ) {
				mLog.LogException( "LastFmProvider creating Session:", ex );
			}
		}

		public void UpdateArtist( IDatabase database, DbArtist artist ) {
			if( mSession != null ) {
				try {
					var artistId = artist.DbId;
					var parms = database.Database.CreateParameters();

					parms["artistId"] = artistId;
					parms["artistImage"] = ContentType.ArtistPrimaryImage;

					var	artwork = database.Database.ExecuteScalar( "SELECT DbArtwork WHERE AssociatedItem = @artistId AND ContentType = @artistImage", parms ) as DbArtwork;
					var bio = ( from DbTextInfo info in database.Database where info.AssociatedItem == artistId && info.ContentType == ContentType.Biography select info ).FirstOrDefault();
					var similarArtists = ( from DbAssociatedItemList item in database.Database where item.AssociatedItem == artistId && item.ContentType == ContentType.SimilarArtists select item ).FirstOrDefault();
					var topAlbums = ( from DbAssociatedItemList item in database.Database where item.AssociatedItem == artistId && item.ContentType == ContentType.TopAlbums select  item ).FirstOrDefault();
					if( artwork == null ) {
						artwork = new DbArtwork( artistId, ContentType.ArtistPrimaryImage ) { Artist = artist.DbId };

						database.Insert( artwork );
					}
					artwork.UpdateExpiration();
					database.Store( artwork );

					if( bio == null ) {
						bio = new DbTextInfo( artistId, ContentType.Biography ) { Artist = artist.DbId };

						database.Insert( bio );
					}
					bio.UpdateExpiration();

					if( similarArtists == null ) {
						similarArtists = new DbAssociatedItemList( artistId, ContentType.SimilarArtists ) { Artist = artist.DbId };

						database.Insert( similarArtists );
					}
					similarArtists.UpdateExpiration();

					if( topAlbums == null ) {
						topAlbums = new DbAssociatedItemList( artistId, ContentType.TopAlbums ) { Artist = artist.DbId };

						database.Insert( topAlbums );
					}
					topAlbums.UpdateExpiration();

					var	artistSearch = Artist.Search( artist.Name, mSession );
					var	artistMatch = artistSearch.GetFirstMatch();

					if( artistMatch != null ) {
						var	tags = artistMatch.GetTopTags( 3 );
						if( tags.GetLength( 0 ) > 0 ) {
							artist.ExternalGenre = mTagManager.ResolveGenre( tags[0].Item.Name );
						}

						bio.Text = artistMatch.Bio.getContent();
						bio.Source = InfoSource.External;
						bio.IsContentAvailable = true;

						var imageUrl = artistMatch.GetImageURL();
						if(!string.IsNullOrWhiteSpace( imageUrl )) {
							new ImageDownloader( imageUrl, artistId, ArtistImageDownloadComplete );
						}

						var	sim = artistMatch.GetSimilar( 5 );
						var artistList = new List<string>();
						for( int index = 0; index < sim.GetLength( 0 ); index++ ) {
							artistList.Add( sim[index].Name );
						}
						similarArtists.SetItems( artistList );
						similarArtists.IsContentAvailable = true;

						var top = artistMatch.GetTopAlbums();
						var albumList = new List<string>();
						for( int index = 0; index < ( top.GetLength( 0 ) > 5 ? 5 : top.GetLength( 0 )); index++ ) {
							albumList.Add( top[index].Item.Name );
						}
						topAlbums.SetItems( albumList );
						topAlbums.IsContentAvailable = true;

						artist.UpdateLastChange();
						database.Store( artist );

						mLog.LogInfo( "LastFm updated artist: {0}", artist.Name );
					}

					database.Store( bio );
					database.Store( similarArtists );
					database.Store( topAlbums );
				}
				catch( Exception ex ) {
					mLog.LogException( "Exception - LastFmProvider:UpdateArtistInfo:", ex );
				}
			}
		}

		private void ArtistImageDownloadComplete( long parentId, byte[] imageData ) {
			Condition.Requires( imageData ).IsNotNull();

			var dbManager = mContainer.Resolve<IDatabaseManager>();
			var database = dbManager.ReserveDatabase();

			try {
				var parms = database.Database.CreateParameters();

				parms["artistId"] = parentId;
				parms["artistImage"] = ContentType.ArtistPrimaryImage;

				var	artwork = database.Database.ExecuteScalar( "SELECT DbArtwork WHERE AssociatedItem = @artistId AND ContentType = @artistImage", parms ) as DbArtwork;
				if( artwork == null ) {
					artwork = new DbArtwork( parentId, ContentType.ArtistPrimaryImage )
											{ Artist = parentId, Source = InfoSource.External, IsContentAvailable = true };

					database.Insert( artwork );
				}

				artwork.Image = imageData;
				artwork.UpdateExpiration();

				database.Store( artwork );
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - LastFmProvider:ImageDownload: ", ex );
			}
			finally {
				dbManager.FreeDatabase( database );
			}
		}
	}
}
