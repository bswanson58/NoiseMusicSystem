using System;
using DiscogsConnect;
using Noise.Core.DataBuilders;
using Noise.Core.Support;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.DataProviders {
	internal class DiscographyProvider : DiscogsProvider {
		public DiscographyProvider( ILifecycleManager lifecycleManager, 
									IArtistProvider artistProvider, IDiscographyProvider discographyProvider, IAssociatedItemListProvider associatedItemListProvider ) :
			base( artistProvider, discographyProvider, associatedItemListProvider ) {
			lifecycleManager.RegisterForInitialize( this );
		}

		public override ContentType ContentType {
			get { return ( ContentType.Discography ); }
		}
	}

	internal class BandMembersProvider : DiscogsProvider {
		public BandMembersProvider( ILifecycleManager lifecycleManager,
									IArtistProvider artistProvider, IDiscographyProvider discographyProvider, IAssociatedItemListProvider associatedItemListProvider ) :
			base( artistProvider, discographyProvider, associatedItemListProvider ) {
			lifecycleManager.RegisterForInitialize( this );
		}

		public override ContentType ContentType {
			get { return ( ContentType.BandMembers ); }
		}
	}

	internal abstract class DiscogsProvider : IContentProvider, IRequireInitialization {
		private readonly IArtistProvider				mArtistProvider;
		private readonly IDiscographyProvider			mDiscographyProvider;
		private readonly IAssociatedItemListProvider	mAssociationProvider;
		private bool			mHasNetworkAccess;
		private DiscogsClient	mClient;
		public abstract ContentType ContentType { get; }

		protected DiscogsProvider( IArtistProvider artistProvider, IDiscographyProvider discographyProvider, IAssociatedItemListProvider associatedItemListProvider ) {
			mArtistProvider = artistProvider;
			mDiscographyProvider = discographyProvider;
			mAssociationProvider = associatedItemListProvider;
		}

		public void Initialize() {
			var configuration = NoiseSystemConfiguration.Current.RetrieveConfiguration<ExplorerConfiguration>( ExplorerConfiguration.SectionName );
			if( configuration != null ) {
				mHasNetworkAccess = configuration.HasNetworkAccess;
			}
		}

		public void Shutdown() { }

		public TimeSpan ExpirationPeriod {
			get { return ( new TimeSpan( 30, 0, 0, 0 ) ); }
		}

		public bool CanUpdateArtist {
			get { return ( mHasNetworkAccess ); }
		}

		public bool CanUpdateAlbum {
			get { throw new NotImplementedException(); }
		}

		public bool CanUpdateTrack {
			get { throw new NotImplementedException(); }
		}

		private DiscogsClient Client {
			get {
				var	retValue = mClient;

				if( retValue == null ) {
					mClient = new DiscogsClient();
				}

				return ( mClient );
			}
		}

		public void UpdateContent( DbArtist forArtist ) {
			if( mHasNetworkAccess ) {
				try {
					var client = Client;
					var discogsArtist = client.SearchArtist( forArtist.Name, true );

					if( discogsArtist != null ) {
						var bandMembers = mAssociationProvider.GetAssociatedItems( forArtist.DbId, ContentType.BandMembers );
						if( bandMembers == null ) {
							bandMembers = new DbAssociatedItemList( forArtist.DbId, ContentType.BandMembers ) { Artist = forArtist.DbId };

							mAssociationProvider.AddAssociationList( bandMembers );
						}

						if( discogsArtist.BandMembers != null ) {
							using( var updater = mAssociationProvider.GetAssociationForUpdate( bandMembers.DbId )) {
								if( updater.Item != null ) {
									if( discogsArtist.BandMembers.Count > 0 ) {
										updater.Item.SetItems( discogsArtist.BandMembers );
										updater.Item.IsContentAvailable = true;
									}
									else {
										updater.Item.IsContentAvailable = false;
									}

									updater.Item.UpdateExpiration();

									updater.Update();
								}
							}
						}

						using( var releases = mDiscographyProvider.GetDiscography( forArtist.DbId )) {
							foreach( var release in releases.List ) {
								mDiscographyProvider.RemoveDiscography( release );
							}
						}

						if(( discogsArtist.Releases != null ) &&
						   ( discogsArtist.Releases.Count > 0 ) ) {
							foreach( var release in discogsArtist.Releases ) {
								var releaseType = DiscographyReleaseType.Unknown;

								if( release.Type.Length > 0 ) {
									releaseType = DiscographyReleaseType.Other;

									if( String.Compare(release.Role, "Main", StringComparison.OrdinalIgnoreCase) == 0 ) {
										releaseType = DiscographyReleaseType.Release;
									}
									else if( String.Compare(release.Role, "Appearance", StringComparison.OrdinalIgnoreCase) == 0 ) {
										releaseType = DiscographyReleaseType.Appearance;
									}
									else if( String.Compare(release.Role, "TrackAppearance", StringComparison.OrdinalIgnoreCase) == 0 ) {
										releaseType = DiscographyReleaseType.TrackAppearance;
									}
								}
								mDiscographyProvider.AddDiscography( new DbDiscographyRelease( forArtist.DbId, release.Title, "", "", (uint)release.Year, releaseType )
																		{ IsContentAvailable = true } );
							}
						}
						else {
							mDiscographyProvider.AddDiscography( new DbDiscographyRelease( forArtist.DbId, "", "", "", Constants.cUnknownYear, DiscographyReleaseType.Unknown ));
						}

						if(( discogsArtist.Urls != null ) &&
						   ( discogsArtist.Urls.Count > 0 ) &&
						   ( !string.IsNullOrWhiteSpace( discogsArtist.Urls[0] ))) {
							var url = discogsArtist.Urls[0];

							forArtist.Website = url.Replace( Environment.NewLine, "" ).Replace( "\n", "" ).Replace( "\r", "" );
						}

						mArtistProvider.UpdateArtistLastChanged( forArtist.DbId );

						NoiseLogger.Current.LogMessage( String.Format( "Discogs updated artist: {0}", forArtist.Name ));
					}
				}
				catch( Exception ex ) {
					NoiseLogger.Current.LogException( "Discogs Provider: ", ex );
				}
			}
		}

		public void UpdateContent( DbAlbum forAlbum ) {
			throw new NotImplementedException();
		}

		public void UpdateContent( DbTrack forTrack ) {
			throw new NotImplementedException();
		}
	}
}
