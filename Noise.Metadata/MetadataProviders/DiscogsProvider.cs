using System;
using System.Collections.Generic;
using System.Linq;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Metadata.Dto;
using Noise.Metadata.Interfaces;
using Noise.Metadata.MetadataProviders.Discogs;
using Raven.Client;

namespace Noise.Metadata.MetadataProviders {
	internal class DiscogsProvider : IArtistMetadataProvider {
		private IDocumentStore	mDocumentStore;
		private bool			mHasNetworkAccess;

		public	string			ProviderKey { get; private set; }

		public DiscogsProvider() {
			ProviderKey = "Discogs";
		}

		public void Initialize( IDocumentStore documentStore, ILicenseManager licenseManager ) {
			mDocumentStore = documentStore;

			var configuration = NoiseSystemConfiguration.Current.RetrieveConfiguration<ExplorerConfiguration>( ExplorerConfiguration.SectionName );
			if( configuration != null ) {
				mHasNetworkAccess = configuration.HasNetworkAccess;
			}
		}

		public void Shutdown() {
		}

		public async void UpdateArtist( string artistName ) {
			if( mHasNetworkAccess ) {
				try {
					var discogsClient = new DiscogsClient();
					var searchResults = await discogsClient.ArtistSearch( artistName );

					if(( searchResults != null ) &&
					   ( searchResults.Any())) {
						var result = searchResults[0];

						if( result.Type.Equals( DiscogsClient.cSearchItemTypeArtist )) {
							var discogsArtist = await discogsClient.GetArtist( result.Id );

							using( var session = mDocumentStore.OpenSession()) {
								var artistBio = session.Load<DbArtistBiography>( DbArtistBiography.FormatStatusKey( artistName )) ??
												new DbArtistBiography { ArtistName = artistName };
								var discography = session.Load<DbArtistDiscography>( DbArtistDiscography.FormatStatusKey( artistName )) ??
												new DbArtistDiscography { ArtistName = artistName };
								var bandMembers = new List<string>();

								if( discogsArtist.Members != null ) {
									bandMembers.AddRange( from m in discogsArtist.Members where m.Active select m.Name );
									bandMembers.AddRange( from m in discogsArtist.Members where !m.Active select "-" + m.Name );
								}
								if( discogsArtist.Groups != null ) {
									bandMembers.AddRange( from g in discogsArtist.Groups select "(" + g.Name + ")" );
								}

								if( bandMembers.Any()) {
									artistBio.SetMetadata( eMetadataType.BandMembers, bandMembers );
								}
								else {
									artistBio.ClearMetadata( eMetadataType.BandMembers );
								}

								var artistReleases = await discogsClient.GetArtistReleases( result.Id );
								if( artistReleases != null ) {
									discography.Discography.Clear();

									foreach( var release in artistReleases ) {
										var releaseType = DiscographyReleaseType.Other;

										if( String.Compare(release.Role, "Main", StringComparison.OrdinalIgnoreCase) == 0 ) {
											releaseType = DiscographyReleaseType.Release;
										}
										else if( String.Compare(release.Role, "Appearance", StringComparison.OrdinalIgnoreCase) == 0 ) {
											releaseType = DiscographyReleaseType.Appearance;
										}
										else if( String.Compare(release.Role, "TrackAppearance", StringComparison.OrdinalIgnoreCase) == 0 ) {
											releaseType = DiscographyReleaseType.TrackAppearance;
										}

										discography.Discography.Add( new DbDiscographyRelease( release.Title, "", "", release.Year, releaseType ));
									}
								}


								if(( discogsArtist.Urls != null ) &&
									( discogsArtist.Urls.GetLength( 0 ) > 0 ) &&
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
						else {
							NoiseLogger.Current.LogMessage( string.Format( "Discogs search did not locate an artsit: {0}", artistName ));
						}
					}
					else {
						NoiseLogger.Current.LogMessage( string.Format( "Discogs search failed for: {0}", artistName ));
					}
				}
				catch( Exception ex ) {
					NoiseLogger.Current.LogException( string.Format( "Discogs search failed for artist: {0}", artistName ), ex );
				}
			}
		}
	}
}
