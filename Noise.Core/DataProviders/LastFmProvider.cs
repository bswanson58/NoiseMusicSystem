using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using CuttingEdge.Conditions;
using Lastfm.Services;
using Noise.Core.DataBuilders;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;

namespace Noise.Core.DataProviders {
	internal abstract class BaseLastFmProvider : IContentProvider {
		public		abstract ContentType	ContentType { get; }
		protected	INoiseManager			mNoiseManager;
		private		bool					mHasNetworkAccess;

		public bool Initialize( INoiseManager noiseManager ) {
			mNoiseManager = noiseManager;

			var configuration = NoiseSystemConfiguration.Current.RetrieveConfiguration<ExplorerConfiguration>( ExplorerConfiguration.SectionName );

			if( configuration != null ) {
				mHasNetworkAccess = configuration.HasNetworkAccess;
			}

			return( true );
		}

		public TimeSpan ExpirationPeriod {
			get { return( new TimeSpan( 30, 0, 0, 0 )); }
		}

		public bool CanUpdateArtist {
			get{ return( mHasNetworkAccess ); }
		}

		public bool CanUpdateAlbum {
			get{ return( false ); }
		}

		public bool CanUpdateTrack {
			get{ return( false ); }
		}

		public void UpdateContent( IDatabase database, DbArtist forArtist ) {
			if( mHasNetworkAccess ) {
				var provider = new LastFmProvider( mNoiseManager );

				provider.UpdateArtist( database, forArtist );
			}
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
		private readonly IDatabaseManager	mDatabaseManager;
		private readonly ITagManager		mTagManager;
		private readonly Session			mSession;

		public LastFmProvider( INoiseManager noiseManager ) {
			mDatabaseManager = noiseManager.DatabaseManager;
			mTagManager = noiseManager.TagManager;

			try {
				var key = NoiseLicenseManager.Current.RetrieveKey( LicenseKeys.LastFm );

				if( key != null ) {
					mSession = new Session( key.Name, key.Key );
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "LastFmProvider creating Session:", ex );
			}
		}

		public void UpdateArtist( IDatabase database, DbArtist artist ) {
			if( mSession != null ) {
				try {
					var artistId = artist.DbId;
					var parms = database.Database.CreateParameters();

					parms["artistId"] = artistId;
					parms["artistImage"] = ContentType.ArtistPrimaryImage;
					parms["biography"] = ContentType.Biography;
					parms["similarArtist"] = ContentType.SimilarArtists;
					parms["topAlbums"] = ContentType.TopAlbums;

					var	artwork = database.Database.ExecuteScalar( "SELECT DbArtwork WHERE AssociatedItem = @artistId AND ContentType = @artistImage", parms ) as DbArtwork;
					var bio = database.Database.ExecuteScalar( "SELECT DbTextInfo WHERE AssociatedItem = @artistId AND ContentType = @biography", parms ) as DbTextInfo;
					var similarArtists = database.Database.ExecuteScalar( "SELECT DbAssociatedItemList WHERE AssociatedItem = @artistId AND ContentType = @similarArtist", parms ) as DbAssociatedItemList;
					var topAlbums = database.Database.ExecuteScalar( "SELECT DbAssociatedItemList WHERE AssociatedItem = @artistId AND ContentType = @topAlbums", parms ) as DbAssociatedItemList;
					if( artwork == null ) {
						artwork = new DbArtwork( artistId, ContentType.ArtistPrimaryImage ) { Artist = artist.DbId, Name = "Last.fm download" };

						database.Insert( artwork );
					}
					artwork.UpdateExpiration();
					database.Store( artwork );

					if( bio == null ) {
						bio = new DbTextInfo( artistId, ContentType.Biography ) { Artist = artist.DbId, Name = "Last.fm download" };

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

						database.BlobStorage.StoreText( bio.DbId, artistMatch.Bio.getContent());
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

						NoiseLogger.Current.LogMessage( "LastFm updated artist: {0}", artist.Name );
					}

					database.Store( bio );
					database.Store( similarArtists );
					database.Store( topAlbums );
				}
				catch( Exception ex ) {
					NoiseLogger.Current.LogException( "Exception - LastFmProvider:UpdateArtistInfo:", ex );
				}
			}
		}

		private void ArtistImageDownloadComplete( long parentId, byte[] imageData ) {
			Condition.Requires( imageData ).IsNotNull();

			var database = mDatabaseManager.ReserveDatabase();

			try {
				var parms = database.Database.CreateParameters();

				parms["artistId"] = parentId;
				parms["artistImage"] = ContentType.ArtistPrimaryImage;

				var	artwork = database.Database.ExecuteScalar( "SELECT DbArtwork WHERE AssociatedItem = @artistId AND ContentType = @artistImage", parms ) as DbArtwork;
				if( artwork == null ) {
					artwork = new DbArtwork( parentId, ContentType.ArtistPrimaryImage )
											{ Artist = parentId, Source = InfoSource.External, IsContentAvailable = true, Name = "Last.fm download" };

					database.Insert( artwork );
				}

				var	memoryStream = new MemoryStream( imageData );
				database.BlobStorage.Store( artwork.DbId, memoryStream );

				artwork.UpdateExpiration();
				database.Store( artwork );
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Exception - LastFmProvider:ImageDownload: ", ex );
			}
			finally {
				mDatabaseManager.FreeDatabase( database );
			}
		}
	}
}
