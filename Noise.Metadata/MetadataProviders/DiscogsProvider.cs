using System;
using DiscogsConnect;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Metadata.Dto;
using Noise.Metadata.Interfaces;
using Raven.Client;

namespace Noise.Metadata.MetadataProviders {
	internal class DiscogsProvider : IArtistMetadataProvider {
		private IDocumentStore	mDocumentStore;
		private DiscogsClient	mClient;
		private bool			mHasNetworkAccess;

		public	string			ProviderKey { get; private set; }

		public DiscogsProvider() {
			ProviderKey = "Discogs";
		}

		public void Initialize( IDocumentStore documentStore ) {
			mDocumentStore = documentStore;

			var configuration = NoiseSystemConfiguration.Current.RetrieveConfiguration<ExplorerConfiguration>( ExplorerConfiguration.SectionName );
			if( configuration != null ) {
//				mHasNetworkAccess = configuration.HasNetworkAccess;
			}
		}

		public void Shutdown() {
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

		public void UpdateArtist( string artistName ) {
			if( mHasNetworkAccess ) {
				try {
					var client = Client;
					var discogsArtist = client.SearchArtist( artistName, true );

					if( discogsArtist != null ) {
						using( var session = mDocumentStore.OpenSession()) {
							var artistBio = session.Load<DbArtistBiography>( DbArtistBiography.FormatStatusKey( artistName )) ??
											new DbArtistBiography { ArtistName = artistName };
							var discography = session.Load<DbArtistDiscography>( DbArtistDiscography.FormatStatusKey( artistName )) ??
											new DbArtistDiscography { ArtistName = artistName };

							if( discogsArtist.BandMembers != null ) {
								artistBio.SetMetadata( eMetadataType.BandMembers, discogsArtist.BandMembers );
							}
							else {
								artistBio.ClearMetadata( eMetadataType.BandMembers );
							}

							discography.Discography.Clear();
							if( discogsArtist.Releases != null ) {
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
									discography.Discography.Add( new DbDiscographyRelease( release.Title, "", "", release.Year, releaseType ));
								}
							}

							if(( discogsArtist.Urls != null ) &&
							   ( discogsArtist.Urls.Count > 0 ) &&
							   ( !string.IsNullOrWhiteSpace( discogsArtist.Urls[0] ))) {
								var url = discogsArtist.Urls[0];

								artistBio.SetMetadata( eMetadataType.WebSite, url.Replace( Environment.NewLine, "" ).Replace( "\n", "" ).Replace( "\r", "" ));
							}

							session.Store( artistBio );
							session.Store( discography );
							session.SaveChanges();

							NoiseLogger.Current.LogMessage( "Discogs updated artist: {0}", artistName );
						}
					}
				}
				catch( Exception ex ) {
					NoiseLogger.Current.LogException( string.Format( "Discogs failed for artist: {0}", artistName ), ex );
				}
			}
		}
	}
}
