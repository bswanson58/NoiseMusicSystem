using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using Hammock;
using Hammock.Serialization;
using Microsoft.Practices.Unity;
using Noise.Core.DataBuilders;
using Noise.Infrastructure.Dto;

namespace Noise.Core.DataProviders {
	public class DiscogsArtist {
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

	public class DiscogRelease {
		public string	Id { get; private set; }
		public string	Status { get; private set; }
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
					retValue.WebsiteUrl = urlList.First() != null ? urlList.First().Value : "";

					elements = from images in artistRoot.Descendants( "images" ) select images;
					var imageList = from image in elements.Descendants( "image" ) select image;
					if( imageList.First() != null ) {
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

	public class DiscogReleaseDetail {
		
	}

	internal class ReleaseDeserializer : IDeserializer {
		public object Deserialize( string content, Type type ) {
			var retValue = new DiscogReleaseDetail();

			if( type == typeof( DiscogReleaseDetail )) {
				
			}

			return( retValue );
		}

		public T Deserialize<T>( string content ) {
			return((T)Deserialize( content, typeof( T )));
		}
	}

//	[Export( typeof( IContentProvider ))]
	internal class DiscographyProvider : IContentProvider {
		[Dependency]
		private IUnityContainer	Container { get; set; }

		public ContentType ContentType {
			get { return( ContentType.Discography ); }
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

		public void UpdateContent( DbArtist forArtist ) {
			var discogsProvider = new DiscogsProvider();

			discogsProvider.UpdateDiscography( forArtist.Name );
		}

		public void UpdateContent( DbAlbum forAlbum ) {
			throw new NotImplementedException();
		}

		public void UpdateContent( DbTrack forTrack ) {
			throw new NotImplementedException();
		}
	}

	internal class DiscogsProvider {
		private const string	cApiKey = "406ed9a21c";
		private const string	cAuthority = "http://www.discogs.com";

		private	readonly RestClient		mClient;

		public DiscogsProvider() {
			mClient = new RestClient { Authority = cAuthority, UserAgent = "Noise", DecompressionMethods = DecompressionMethods.GZip };
			mClient.AddParameter( "f", "xml" );
			mClient.AddParameter( "api_key", cApiKey );
		}

		public void UpdateDiscography( string artistName ) {
			var	request = new RestRequest { VersionPath = "artist", Path = artistName, Deserializer = new ArtistDeserializer() };

			var	response = mClient.Request<DiscogsArtist>( request );
			if(( response.StatusCode == HttpStatusCode.OK ) &&
			   ( response.ContentEntity != null )) {
				foreach( var release in response.ContentEntity.ReleaseList ) {
					AddRelease( release.Id );
				}
			}
		}

		public void AddRelease( string releaseId ) {
			var	request = new RestRequest { VersionPath = "release", Path = releaseId, Deserializer = new ReleaseDeserializer() };
			var response = mClient.Request<DiscogReleaseDetail>( request );

			if(( response.StatusCode == HttpStatusCode.OK ) &&
			   ( response.ContentEntity != null )) {
			}
		}
	}
}
