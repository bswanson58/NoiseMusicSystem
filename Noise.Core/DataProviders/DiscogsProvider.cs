﻿using System;
using System.ComponentModel.Composition;
using System.Linq;
using DiscogsConnect;
using Microsoft.Practices.Unity;
using Noise.Core.Database;
using Noise.Core.DataBuilders;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;

namespace Noise.Core.DataProviders {
	[Export( typeof( IContentProvider ) )]
	internal class DiscographyProvider : DiscogsProvider {
		public override ContentType ContentType {
			get { return ( ContentType.Discography ); }
		}
	}

	[Export( typeof( IContentProvider ) )]
	internal class BandMembersProvider : DiscogsProvider {
		public override ContentType ContentType {
			get { return ( ContentType.BandMembers ); }
		}
	}

	internal abstract class DiscogsProvider : IContentProvider {
		private IUnityContainer	mContainer;
		private bool			mHasNetworkAccess;
		private DiscogsClient	mClient;
		public abstract ContentType ContentType { get; }

		public bool Initialize( IUnityContainer container ) {
			mContainer = container;

			var	systemConfig = mContainer.Resolve<ISystemConfiguration>();
			var configuration = systemConfig.RetrieveConfiguration<ExplorerConfiguration>( ExplorerConfiguration.SectionName );

			if( configuration != null ) {
				mHasNetworkAccess = configuration.HasNetworkAccess;
			}

			return ( true );
		}

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

		public void UpdateContent( IDatabase database, DbArtist forArtist ) {
			if( mHasNetworkAccess ) {
				try {
					var client = Client;
					var discogsArtist = client.SearchArtist( forArtist.Name, true );

					if( discogsArtist != null ) {
						var artistId = forArtist.DbId;
						var parms = database.Database.CreateParameters();

						parms["artistId"] = artistId;
						parms["bandMembers"] = ContentType.BandMembers;

						var bandMembers = database.Database.ExecuteScalar( "SELECT DbAssociatedItemList WHERE AssociatedItem = @artistId AND ContentType = @bandMembers", parms ) as DbAssociatedItemList;

						if( bandMembers == null ) {
							bandMembers = new DbAssociatedItemList( artistId, ContentType.BandMembers ) { Artist = forArtist.DbId };

							database.Insert( bandMembers );
						}
						bandMembers.UpdateExpiration();

						if(( discogsArtist.BandMembers != null ) &&
							( discogsArtist.BandMembers.Count > 0 ) ) {

							bandMembers.IsContentAvailable = true;
							bandMembers.SetItems( discogsArtist.BandMembers );
						}
						else {
							bandMembers.IsContentAvailable = false;
						}

						database.Store( bandMembers );

						var releases = from DbDiscographyRelease release in database.Database where release.AssociatedItem == artistId select release;
						foreach( var release in releases ) {
							database.Delete( release );
						}

						if(( discogsArtist.Releases != null ) &&
						   ( discogsArtist.Releases.Count > 0 ) ) {
							foreach( var release in discogsArtist.Releases ) {
								var releaseType = DiscographyReleaseType.Unknown;

								if( release.Type.Length > 0 ) {
									releaseType = DiscographyReleaseType.Other;

									if( String.Compare( release.Role, "Main", true ) == 0 ) {
										releaseType = DiscographyReleaseType.Release;
									}
									else if( String.Compare( release.Role, "Appearance", true ) == 0 ) {
										releaseType = DiscographyReleaseType.Appearance;
									}
									else if( String.Compare( release.Role, "TrackAppearance", true ) == 0 ) {
										releaseType = DiscographyReleaseType.TrackAppearance;
									}
								}
								database.Insert( new DbDiscographyRelease( artistId, release.Title, "", "", (uint)release.Year, releaseType ) { IsContentAvailable = true } );
							}
						}
						else {
							database.Insert( new DbDiscographyRelease( artistId, "", "", "", Constants.cUnknownYear, DiscographyReleaseType.Unknown ) );
						}

						if(( discogsArtist.Urls != null ) &&
						   ( discogsArtist.Urls.Count > 0 ) &&
						   ( !string.IsNullOrWhiteSpace( discogsArtist.Urls[0] ))) {
							var url = discogsArtist.Urls[0];

							forArtist.Website = url.Replace( Environment.NewLine, "" ).Replace( "\n", "" ).Replace( "\r", "" );
						}
						forArtist.UpdateLastChange();
						database.Store( forArtist );

						NoiseLogger.Current.LogMessage( String.Format( "Discogs updated artist: {0}", forArtist.Name ));
					}
				}
				catch( Exception ex ) {
					NoiseLogger.Current.LogException( "Discogs Provider: ", ex );
				}
			}
		}

		public void UpdateContent( IDatabase database, DbAlbum forAlbum ) {
			throw new NotImplementedException();
		}

		public void UpdateContent( IDatabase database, DbTrack forTrack ) {
			throw new NotImplementedException();
		}
	}
}
