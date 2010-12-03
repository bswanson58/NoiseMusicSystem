using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using Hammock;
using Hammock.Serialization;
using Microsoft.Practices.Unity;
using Noise.Core.Database;
using Noise.Core.DataBuilders;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.DataProviders {
	internal class DiscogsSearch {
		public	string Uri { get; set; }
	}

	internal class DiscogsArtist {
		public string				Name { get; set; }
		public string				ImageUrl { get; set; }
		public string				WebsiteUrl { get; set; }
		public List<string>			Members { get; private set; }
		public List<DiscogRelease>	ReleaseList { get; private set; }

		public DiscogsArtist() {
			Members = new List<string>();
			ReleaseList = new List<DiscogRelease>();
		}
	}

	internal class DiscogRelease {
		public string	Id { get; private set; }
		public string	Status { get; private set; }
		public string	Type { get; private set; }
		public string	Title { get; private set; }
		public string	Format { get; private set; }
		public string	Label { get; private set; }
		public uint		Year { get; private set; }

		public DiscogRelease( XElement root ) {
			if( root != null ) {
				var	attrib = root.Attribute( "id" );
				Id = attrib != null ? attrib.Value : "";
				attrib = root.Attribute( "status" );
				Status = attrib != null ? attrib.Value : "";
				attrib = root.Attribute( "type" );
				Type = attrib != null ? attrib.Value : "";

				var element = root.Element( "title" );
				Title = element != null ? element.Value : "";
				element = root.Element( "format" );
				Format = element != null ? element.Value : "";
				element = root.Element( "label" );
				Label = element != null ? element.Value : "";
				element = root.Element( "year" );
				Year = element != null ? uint.Parse( element.Value ) : 0;
			}
		}
	}

	internal class SearchDeserializer : IDeserializer {
		public object Deserialize( string content, Type type ) {
			var retValue = new DiscogsSearch();

			if( type == typeof( DiscogsSearch )) {
				XElement	root = XElement.Parse( content );
				XElement	searchRoot = root.Element( "exactresults" );

				if( searchRoot != null ) {
					var	result = searchRoot.Element( "result" );

					if( result != null ) {
						var attrib = result.Attribute( "type" );

						if(( attrib != null ) &&
						   ( String.Compare( attrib.Value, "artist", true ) == 0 )) {
							var element = result.Element( "uri" );

							if( element != null ) {
								retValue.Uri = element.Value;
							}
						}
					}
				}
			}

			return( retValue );
		}

		public T Deserialize<T>( string content ) {
			return((T)Deserialize( content, typeof (T)));
		}
	}

	internal class ArtistDeserializer : IDeserializer {
		public object Deserialize( string content, Type type ) {
			var retValue = new DiscogsArtist();

			if( type == typeof( DiscogsArtist )) {
				XElement	root = XElement.Parse( content );
				XElement	artistRoot = root.Element( "artist" );

				if( artistRoot != null ) {
					var elements = from name in artistRoot.Descendants( "name" ) select name;

					retValue.Name = elements.First() != null ? elements.First().Value : "";

					elements = from url in artistRoot.Descendants( "urls" ) select url;
					var urlList = from url in elements.Descendants( "url" ) select url;
					retValue.WebsiteUrl = urlList.FirstOrDefault() != null ? urlList.First().Value : "";

					elements = from images in artistRoot.Descendants( "images" ) select images;
					var imageList = from image in elements.Descendants( "image" ) select image;
					if( imageList.FirstOrDefault() != null ) {
						var attrib = imageList.First().Attribute( "uri" );
						retValue.ImageUrl = attrib != null ? attrib.Value : "";
					}

					elements = from members in artistRoot.Descendants( "members" ) select members;
					var memberNames = from names in elements.Descendants( "name" ) select names;
					foreach( var member in memberNames ) {
						retValue.Members.Add( member.Value );
					}

					elements = from releases in artistRoot.Descendants( "releases" ) select releases;
					var releaseList = from release in elements.Descendants( "release" ) select  release;
					foreach( var release in releaseList ) {
						retValue.ReleaseList.Add( new DiscogRelease( release ));
					}
				}
			}

			return( retValue );
		}

		public T Deserialize<T>( string content ) {
			return((T)Deserialize( content, typeof (T)));
		}
	}

	internal class ReleaseDeserializer : IDeserializer {
		private readonly long mArtistId;

		public ReleaseDeserializer( long artistId ) {
			mArtistId = artistId;
		}

		public object Deserialize( string content, Type type ) {
			DbDiscographyRelease	retValue = null;

			if( type == typeof( DbDiscographyRelease )) {
				XElement	root = XElement.Parse( content );
				XElement	releaseRoot = root.Element( "release" );

				if( releaseRoot != null ) {
					var element = releaseRoot.Element( "title" );
					var title = element != null ? element.Value : "";

					var format = "";
					element = releaseRoot.Element( "formats" );
					if( element != null ) {
						var formatElement = element.Element( "format" );
						if( formatElement != null ) {
							var attrib = formatElement.Attribute( "name" );

							format = attrib != null ? attrib.Value : "";
						}
					}

					var label = "";
					element = releaseRoot.Element( "labels" );
					if( element != null ) {
						var labelElement = element.Element( "label" );
						if( labelElement != null ) {
							var attrib = labelElement.Attribute( "name" );

							label = attrib != null ? attrib.Value : "";
						}
					}

					var year = Constants.cUnknownYear;
					element = releaseRoot.Element( "released" );
					if( element != null ) {
						uint.TryParse( element.Value, out year );
					}
				
					retValue = new DbDiscographyRelease( mArtistId, title, format, label, year, DiscographyReleaseType.Unknown );
				}
			}

			return( retValue );
		}

		public T Deserialize<T>( string content ) {
			return((T)Deserialize( content, typeof( T )));
		}
	}

	[Export( typeof( IContentProvider ))]
	internal class DiscographyProvider : DiscogsProvider {
		public override ContentType ContentType {
			get { return( ContentType.Discography ); }
		}
	}

	[Export( typeof( IContentProvider ))]
	internal class BandMembersProvider : DiscogsProvider {
		public override ContentType ContentType {
			get { return( ContentType.BandMembers ); }
		}
	}

	internal abstract class DiscogsProvider : IContentProvider {
		private const string	cAuthority = "http://www.discogs.com";

		private RestClient		mClient;
		private ILog			mLog;

		[Import]
		private IUnityContainer		Container { get; set; }

		public	abstract ContentType	ContentType { get; }

		private RestClient Client {
			get {
				if( mClient == null ) {
					try {
						var licenseManager = Container.Resolve<ILicenseManager>();
						if( licenseManager.Initialize( Constants.LicenseKeyFile )) {
							var key = licenseManager.RetrieveKey( LicenseKeys.Discogs );

							if( key != null ) {
								mClient = new RestClient { Authority = cAuthority, UserAgent = "Noise", DecompressionMethods = DecompressionMethods.GZip };
								mClient.AddParameter( "f", "xml" );
								mClient.AddParameter( "api_key", key.Key );
							}
						}
					}
					catch( Exception ex ) {
						Log.LogException( "Exception - Configuring DiscogsProvider: ", ex );
					}
				}

				return( mClient );
			}
		}

		private ILog Log {
			get {
				if( mLog == null ) {
					mLog = Container.Resolve<ILog>();
				}

				return( mLog );
			}
		}

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

		private Uri SearchForArtist( DbArtist forArtist ) {
			Uri retValue = null;
			var request = new RestRequest { VersionPath = "search", Deserializer = new SearchDeserializer() };

			request.AddParameter( "type", "all" );
			request.AddParameter( "q", forArtist.Name );

			var response = Client.Request<DiscogsSearch>( request );
			if(( response.StatusCode == HttpStatusCode.OK ) &&
			   ( response.ContentEntity != null ) &&
			   ( response.ContentEntity.Uri != null )) {
				retValue = new Uri( response.ContentEntity.Uri );
			}

			return( retValue );
		}

		public void UpdateContent( IDatabase database, DbArtist forArtist ) {
			if( Client != null ) {
				try {
					var request = new RestRequest {  Deserializer = new ArtistDeserializer() };
					var requestUri = SearchForArtist( forArtist );
					if( requestUri != null ) {
						request.Path = requestUri.AbsolutePath;

						var	response = Client.Request<DiscogsArtist>( request );
						if(( response.StatusCode == HttpStatusCode.OK ) &&
						   ( response.ContentEntity != null )) {
							var artistId = forArtist.DbId;
							var	bandMembers = ( from DbAssociatedItemList item in database.Database 
												where item.AssociatedItem == artistId && item.ContentType == ContentType.BandMembers select  item ).FirstOrDefault();

							if( bandMembers == null ) {
								bandMembers = new DbAssociatedItemList( artistId, ContentType.BandMembers ) { Artist = forArtist.DbId };

								database.Insert( bandMembers );
							}
							bandMembers.UpdateExpiration();

							if(( response.ContentEntity.Members != null ) &&
							   ( response.ContentEntity.Members.Count > 0 )) {

								bandMembers.IsContentAvailable = true;
								bandMembers.SetItems( response.ContentEntity.Members );

							}
							else {
								bandMembers.IsContentAvailable = false;
							}

							database.Store( bandMembers );

							var releases = from DbDiscographyRelease release in database.Database where release.AssociatedItem == artistId select release;
							foreach( var release in releases ) {
								database.Delete( release );
							}

							if( response.ContentEntity.ReleaseList.Count > 0 ) {
								foreach( DiscogRelease release in response.ContentEntity.ReleaseList ) {
									var releaseType = DiscographyReleaseType.Unknown;

									if( release.Type.Length > 0 ) {
										releaseType = DiscographyReleaseType.Other;

										if( String.Compare( release.Type, "Main", true ) == 0 ) {
											releaseType = DiscographyReleaseType.Release;
										}
										else if( String.Compare( release.Type, "Appearance", true ) == 0 ) {
											releaseType = DiscographyReleaseType.Appearance;
										}
										else if( String.Compare( release.Type, "TrackAppearance", true ) == 0 ) {
											releaseType = DiscographyReleaseType.TrackAppearance;
										}
									}
									database.Insert( new DbDiscographyRelease( artistId, release.Title, release.Format, release.Label, release.Year, releaseType )
																		{ IsContentAvailable = true });
								}
							}
							else {
								database.Insert( new DbDiscographyRelease( artistId, "", "", "", Constants.cUnknownYear, DiscographyReleaseType.Unknown ));
							}

							forArtist.Website = response.ContentEntity.WebsiteUrl;
							database.Store( forArtist );

		//					var releaseAdded = false;
		//					foreach( var release in response.ContentEntity.ReleaseList ) {
		//						AddRelease( forArtist.Name, artistId, release.Id );
		//
		//						releaseAdded = true;
		//					}
		//					if(!releaseAdded ) {
		//						Database.Database.Store( new DbDiscographyRelease( artistId, "", "", "", Constants.cUnknownYear ));
		//					}

							Log.LogInfo( String.Format( "Discogs updated artist: {0}", forArtist.Name ));
						}
						else {
							Log.LogMessage( String.Format( "Discogs: {0}", response.StatusCode ));
						}
					}
				}
				catch( Exception ex ) {
					Log.LogException( "Discogs Provider: ", ex );
				}
			}
		}

/*		private void AddRelease( string artistName, long artistId, string releaseId ) {
			try {
				var	request = new RestRequest { VersionPath = "release", Path = releaseId, Deserializer = new ReleaseDeserializer( artistId ) };
				var response = mClient.Request<DbDiscographyRelease>( request );

				if(( response.StatusCode == HttpStatusCode.OK ) &&
				   ( response.ContentEntity != null )) {
					response.ContentEntity.IsContentAvailable = true;
					Database.Database.Store( response.ContentEntity );
				}
			}
			catch( Exception ex ) {
				mLog.LogException( String.Format( "Discogs requesting release {0} from artist: {1}", releaseId, artistName ), ex );
			}
		}
*/
		public void UpdateContent( IDatabase database, DbAlbum forAlbum ) {
			throw new NotImplementedException();
		}

		public void UpdateContent( IDatabase database, DbTrack forTrack ) {
			throw new NotImplementedException();
		}
	}
}
