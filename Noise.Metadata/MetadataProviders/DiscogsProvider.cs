using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Metadata.Dto;
using Noise.Metadata.Interfaces;
using Noise.Metadata.Logging;
using Noise.Metadata.MetadataProviders.Discogs;
using Raven.Client;

namespace Noise.Metadata.MetadataProviders {
	internal class DiscogsProvider : IArtistMetadataProvider {
		private readonly IDiscogsClient	mDiscogsClient;
		private readonly ILogMetadata	mLog;
		private IDocumentStore			mDocumentStore;
		private readonly bool			mHasNetworkAccess;

		public	string			ProviderKey { get; private set; }

		public DiscogsProvider( NoiseCorePreferences preferences, IDiscogsClient discogsClient, ILogMetadata log ) {
			mDiscogsClient = discogsClient;
			mLog = log;

			mHasNetworkAccess = preferences.HasNetworkAccess;

			ProviderKey = "Discogs";
		}

		public void Initialize( IDocumentStore documentStore ) {
			mDocumentStore = documentStore;
		}

		public void Shutdown() {
		}

		public void UpdateArtist( string artistName ) {
			// Since we are running on a thread, wait for the async work to
			//  complete before our thread is terminated.
			if( mHasNetworkAccess ) {
				AsyncUpdateArtist( artistName ).Wait();
			}
		}

		private async Task AsyncUpdateArtist( string artistName ) {
			try {
				var searchResults = await mDiscogsClient.ArtistSearch( artistName );

				if(( searchResults != null ) &&
				   ( searchResults.Results != null ) &&
				   ( searchResults.Results.Any())) {
					var result = searchResults.Results[0];

					if( result.Type.Equals( DiscogsClient.cSearchItemTypeArtist )) {
						var discogsArtist = await mDiscogsClient.GetArtist( result.Id );

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

							var artistReleases = await mDiscogsClient.GetArtistReleases( result.Id );
							if(( artistReleases != null ) &&
							   ( artistReleases.Releases != null )) {
								discography.Discography.Clear();

								foreach( var release in artistReleases.Releases ) {
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
							   (!string.IsNullOrWhiteSpace( discogsArtist.Urls[0]))) {
								var url = discogsArtist.Urls[0];

								artistBio.SetMetadata( eMetadataType.WebSite, url.Replace( Environment.NewLine, "" ).Replace( "\n", "" ).Replace( "\r", "" ));
							}

							session.Store( artistBio );
							session.Store( discography );
							session.SaveChanges();

							mLog.LoadedMetadata( ProviderKey, artistName );
						}
					}
					else {
						mLog.ArtistNotFound( ProviderKey, artistName );
					}
				}
				else {
					mLog.ArtistNotFound( ProviderKey, artistName );
				}
			}
			catch( Exception ex ) {
				mLog.LogException( string.Format( "Discogs search failed for artist \"{0}\"", artistName ), ex );
			}
		}
	}
}
