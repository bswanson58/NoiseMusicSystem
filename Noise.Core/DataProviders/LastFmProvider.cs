using System;
using System.Collections.Generic;
using CuttingEdge.Conditions;
using Lastfm.Services;
using Noise.Core.DataBuilders;
using Noise.Core.Support;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;

namespace Noise.Core.DataProviders {
	internal class BiographyProvider : BaseLastFmProvider {
		public BiographyProvider( ILifecycleManager lifecycleManager, LastFmProvider lastFmProvider ) :
			base( lastFmProvider ) {
			lifecycleManager.RegisterForInitialize( this );
		}

		public override ContentType ContentType {
			get { return( ContentType.Biography ); }
		}
	}

	internal class SimilarArtistsProvider : BaseLastFmProvider {
		public SimilarArtistsProvider( ILifecycleManager lifecycleManager, LastFmProvider lastFmProvider ) :
			base( lastFmProvider ) {
			lifecycleManager.RegisterForInitialize( this );
		}

		public override ContentType ContentType {
			get { return( ContentType.SimilarArtists ); }
		}
	}

	internal class TopAlbumsProvider : BaseLastFmProvider {
		public TopAlbumsProvider( ILifecycleManager lifecycleManager, LastFmProvider lastFmProvider ) :
			base( lastFmProvider ) {
			lifecycleManager.RegisterForInitialize( this );
		}

		public override ContentType ContentType {
			get { return( ContentType.TopAlbums ); }
		}
	}

	public abstract class BaseLastFmProvider : IContentProvider, IRequireInitialization {
		public	abstract ContentType	ContentType { get; }
		private	readonly LastFmProvider	mLastFmProvider;
		private	bool					mHasNetworkAccess;

		internal BaseLastFmProvider( LastFmProvider lastFmProvider ) {
			mLastFmProvider = lastFmProvider;
		}

		public void Initialize() {
			var configuration = NoiseSystemConfiguration.Current.RetrieveConfiguration<ExplorerConfiguration>( ExplorerConfiguration.SectionName );

			if( configuration != null ) {
				mHasNetworkAccess = configuration.HasNetworkAccess;
			}

			mLastFmProvider.Initialize();
		}

		public void Shutdown() { }

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

		public void UpdateContent( DbArtist forArtist ) {
			if( mHasNetworkAccess ) {
				mLastFmProvider.UpdateArtist( forArtist );
			}
		}

		public void UpdateContent( DbAlbum forAlbum ) {
			throw new NotImplementedException();
		}

		public void UpdateContent( DbTrack forTrack ) {
			throw new NotImplementedException();
		}
	}

	internal class LastFmProvider {
		private readonly IArtistProvider				mArtistProvider;
		private readonly IArtworkProvider				mArtworkProvider;
		private readonly ITextInfoProvider				mTextInfoProvider;
		private readonly IAssociatedItemListProvider	mAssociationProvider;
		private readonly ITagManager					mTagManager;
		private Session									mSession;

		public LastFmProvider( IArtistProvider artistProvider, IArtworkProvider artworkProvider, ITextInfoProvider textInfoProvider,
							   IAssociatedItemListProvider associatedItemListProvider, ITagManager tagManager ) {
			mArtistProvider = artistProvider;
			mArtworkProvider = artworkProvider;
			mTextInfoProvider = textInfoProvider;
			mAssociationProvider = associatedItemListProvider;
			mTagManager = tagManager;
		}

		public void Initialize() {
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

		public void UpdateArtist( DbArtist artist ) {
			if( mSession != null ) {
				try {
					var artistId = artist.DbId;
					var artwork = mArtworkProvider.GetArtistArtwork( artistId, ContentType.ArtistPrimaryImage );
					var bio = mTextInfoProvider.GetArtistTextInfo( artistId, ContentType.Biography );
					var similarArtists = mAssociationProvider.GetAssociatedItems( artistId, ContentType.SimilarArtists );
					var topAlbums = mAssociationProvider.GetAssociatedItems( artistId, ContentType.TopAlbums );
					if( artwork == null ) {
						var	dbArtwork = new DbArtwork( artistId, ContentType.ArtistPrimaryImage ) { Artist = artist.DbId, Name = "Last.fm download" };
						artwork = new Artwork( dbArtwork );

						mArtworkProvider.AddArtwork( dbArtwork );
					}
					using( var updater = mArtworkProvider.GetArtworkForUpdate( artwork.DbId )) {
						if( updater.Item != null ) {
							updater.Item.UpdateExpiration();

							updater.Update();
						}
					}

					if( bio == null ) {
						var	dbBio = new DbTextInfo( artistId, ContentType.Biography ) { Artist = artist.DbId, Name = "Last.fm download" };
						bio = new TextInfo( dbBio );

						mTextInfoProvider.AddTextInfo( dbBio );
					}
					bio.UpdateExpiration();

					if( similarArtists == null ) {
						similarArtists = new DbAssociatedItemList( artistId, ContentType.SimilarArtists ) { Artist = artist.DbId };

						mAssociationProvider.AddAssociationList( similarArtists );
					}
					similarArtists.UpdateExpiration();

					if( topAlbums == null ) {
						topAlbums = new DbAssociatedItemList( artistId, ContentType.TopAlbums ) { Artist = artist.DbId };

						mAssociationProvider.AddAssociationList( topAlbums );
					}
					topAlbums.UpdateExpiration();

					var	artistSearch = Artist.Search( artist.Name, mSession );
					var	artistMatch = artistSearch.GetFirstMatch();

					if( artistMatch != null ) {
						var	tags = artistMatch.GetTopTags( 3 );
						if( tags.GetLength( 0 ) > 0 ) {
							artist.ExternalGenre = mTagManager.ResolveGenre( tags[0].Item.Name );
						}

						using( var updater = mTextInfoProvider.GetTextInfoForUpdate( bio.DbId )) {
							if( updater.Item != null ) {
								updater.Item.Source = InfoSource.External;
								updater.Item.IsContentAvailable = true;
								updater.Item.Text = artistMatch.Bio.getContent();

								updater.Update();
							}
						}

						var imageUrl = artistMatch.GetImageURL();
						if(!string.IsNullOrWhiteSpace( imageUrl )) {
							new ImageDownloader( imageUrl, mArtworkProvider, artwork.DbId, ArtistImageDownloadComplete );
						}

						var	sim = artistMatch.GetSimilar( 5 );
						var artistList = new List<string>();
						for( int index = 0; index < sim.GetLength( 0 ); index++ ) {
							artistList.Add( sim[index].Name );
						}
						using( var updater = mAssociationProvider.GetAssociationForUpdate( similarArtists.DbId )) {
							if( updater.Item != null ) {
								updater.Item.SetItems( artistList );
								updater.Item.IsContentAvailable = true;

								updater.Update();
							}
						}

						var top = artistMatch.GetTopAlbums();
						var albumList = new List<string>();
						for( int index = 0; index < ( top.GetLength( 0 ) > 5 ? 5 : top.GetLength( 0 )); index++ ) {
							albumList.Add( top[index].Item.Name );
						}
						using( var updater = mAssociationProvider.GetAssociationForUpdate( topAlbums.DbId )) {
							if( updater.Item != null ) {
								updater.Item.SetItems( albumList );
								updater.Item.IsContentAvailable = true;

								updater.Update();
							}
						}

						mArtistProvider.UpdateArtistLastChanged( artistId );

						NoiseLogger.Current.LogMessage( "LastFm updated artist: {0}", artist.Name );
					}
				}
				catch( Exception ex ) {
					NoiseLogger.Current.LogException( "Exception - LastFmProvider:UpdateArtistInfo:", ex );
				}
			}
		}

		private static void ArtistImageDownloadComplete( IArtworkProvider artworkProvider, long artworkId, byte[] imageData ) {
			Condition.Requires( artworkProvider ).IsNotNull();
			Condition.Requires( imageData ).IsNotNull();

			try {
				using( var updater = artworkProvider.GetArtworkForUpdate( artworkId )) {
					if( updater.Item != null ) {
						updater.Item.Image = imageData;
						updater.Item.UpdateExpiration();

						updater.Update();
					}
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Exception - LastFmProvider:ImageDownload: ", ex );
			}
		}
	}
}
