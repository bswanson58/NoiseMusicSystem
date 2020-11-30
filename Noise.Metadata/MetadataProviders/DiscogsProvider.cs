using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Metadata.ArtistMetadata;
using Noise.Metadata.Dto;
using Noise.Metadata.Interfaces;
using Noise.Metadata.Logging;
using Noise.Metadata.MetadataProviders.Discogs;
using Noise.Metadata.MetadataProviders.Discogs.Rto;

namespace Noise.Metadata.MetadataProviders {
	internal class DiscogsProvider : IArtistMetadataProvider {
		private readonly ArtistArtworkDownloader	mArtworkDownloader;
		private readonly IDiscogsClient				mDiscogsClient;
		private readonly IArtistBiographyProvider	mBiographyProvider;
		private readonly IArtistDiscographyProvider	mDiscographyProvider;
		private readonly ILogMetadata				mLog;
		private readonly bool						mHasNetworkAccess;

		public	string			ProviderKey { get; }

		public DiscogsProvider( ArtistArtworkDownloader artworkDownloader, NoiseCorePreferences preferences, IDiscogsClient discogsClient, ILogMetadata log,
                                IArtistBiographyProvider biographyProvider, IArtistDiscographyProvider discographyProvider ) {
			mArtworkDownloader = artworkDownloader;
			mDiscogsClient = discogsClient;
			mBiographyProvider = biographyProvider;
			mDiscographyProvider = discographyProvider;
			mLog = log;

			mHasNetworkAccess = preferences.HasNetworkAccess;

			ProviderKey = "Discogs";
		}

		public async Task<bool> UpdateArtist( string artistName ) {
			var retValue = false;

			if( mHasNetworkAccess ) {
				try {
					var discogsArtist = await LocateArtist( artistName );

					if( discogsArtist != null ) {
						var artistReleases = await mDiscogsClient.GetArtistReleases( discogsArtist.Id );
						
						UpdateArtistRecord( artistName, discogsArtist, artistReleases );
						mArtworkDownloader.DownloadArtwork( artistName, discogsArtist.Images, ProviderKey );

						retValue = true;
					}
				}
				catch( Exception ex ) {
					retValue = false;

					mLog.LogException( $"Discogs search failed for artist \"{artistName}\"", ex );
				}
			}

			return( retValue );
		}

		private async Task<DiscogsArtist> LocateArtist( string artistName ) {
			var retValue = default( DiscogsArtist );

			var searchResults = await mDiscogsClient.ArtistSearch( artistName );

			if(( searchResults != null ) &&
			   ( searchResults.Results != null ) &&
			   ( searchResults.Results.Any())) {
				var result = searchResults.Results.First();

				if( result.Type.Equals( DiscogsClient.cSearchItemTypeArtist )) {
					retValue = await mDiscogsClient.GetArtist( result.Id );
				}
			}

			if( retValue == null ) {
				mLog.ArtistNotFound( ProviderKey, artistName );
			}

			return retValue;
		}

		private void UpdateArtistRecord( string artistName, DiscogsArtist discogsArtist, DiscogsArtistReleaseList artistReleases ) {
			var artistBio = mBiographyProvider.GetBiography( artistName ) ?? new DbArtistBiography { ArtistName = artistName };
			var discography = mDiscographyProvider.GetDiscography( artistName ) ?? new DbArtistDiscography { ArtistName = artistName };
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

			if( artistReleases?.Releases != null ) {
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

					discography.Discography.Add( new DbDiscographyRelease( release.Title, string.Empty, string.Empty, release.Year, releaseType ));
				}
			}

			if(( discogsArtist.Urls != null ) &&
			   ( discogsArtist.Urls.GetLength( 0 ) > 0 ) &&
			   (!string.IsNullOrWhiteSpace( discogsArtist.Urls[0]))) {
				var url = discogsArtist.Urls[0];

				artistBio.SetMetadata( eMetadataType.WebSite, url.Replace( Environment.NewLine, String.Empty ).Replace( "\n", String.Empty ).Replace( "\r", String.Empty ));
			}

			if(!mBiographyProvider.InsertOrUpdate( artistBio )) {
				mLog.LogException( "Discogs: Failed to update artist biography", new ApplicationException( "InsertOrUpdate Biography" ));
            }
			if(!mDiscographyProvider.InsertOrUpdate( discography )) {
				mLog.LogException( "Discogs: Failed to update artist discography", new ApplicationException( "InsertOrUpdate Discography" ));
            }

			mLog.LoadedMetadata( ProviderKey, artistName );
		}
	}
}
