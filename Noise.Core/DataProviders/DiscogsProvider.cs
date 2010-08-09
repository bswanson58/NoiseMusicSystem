﻿using System;
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

					element = releaseRoot.Element( "released" );
					var year = element != null ? uint.Parse( element.Value ) : Constants.cUnknownYear;
				
					retValue = new DbDiscographyRelease( mArtistId, title, format, label, year );
				}
			}

			return( retValue );
		}

		public T Deserialize<T>( string content ) {
			return((T)Deserialize( content, typeof( T )));
		}
	}

	internal abstract class DiscogsProvider : IContentProvider {
		private const string	cApiKey = "406ed9a21c";
		private const string	cAuthority = "http://www.discogs.com";

		private	readonly RestClient	mClient;
		private IDatabaseManager	mDatabase;
		private ILog				mLog;

		[Import]
		private IUnityContainer	Container { get; set; }

		public	abstract ContentType	ContentType { get; }

		public DiscogsProvider() {
			mClient = new RestClient { Authority = cAuthority, UserAgent = "Noise", DecompressionMethods = DecompressionMethods.GZip };
			mClient.AddParameter( "f", "xml" );
			mClient.AddParameter( "api_key", cApiKey );

		}

		private IDatabaseManager Database {
			get {
				if( mDatabase == null ) {
					mDatabase = Container.Resolve<IDatabaseManager>();
				}

				return( mDatabase );
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

		[Export( typeof( IContentProvider ))]
		public class DiscographyProvider : DiscogsProvider {
			public override ContentType ContentType {
				get { return( ContentType.Discography ); }
			}
		}

		[Export( typeof( IContentProvider ))]
		public class BandMembersProvider : DiscogsProvider {
			public override ContentType ContentType {
				get { return( ContentType.BandMembers ); }
			}
		}

		public void UpdateContent( DbArtist forArtist ) {
			try {
				var	request = new RestRequest { VersionPath = "artist", Path = forArtist.Name, Deserializer = new ArtistDeserializer() };

				var	response = mClient.Request<DiscogsArtist>( request );
				if(( response.StatusCode == HttpStatusCode.OK ) &&
				   ( response.ContentEntity != null )) {
					var artistId = Database.Database.GetUid( forArtist );
					var	bandMembers = ( from DbAssociatedItems item in Database.Database where item.AssociatedItem == artistId && item.ContentType == ContentType.BandMembers select  item ).FirstOrDefault();

					if( bandMembers == null ) {
						bandMembers = new DbAssociatedItems( artistId, ContentType.BandMembers );
					}
					bandMembers.UpdateExpiration();

					if(( response.ContentEntity.Members != null ) &&
					   ( response.ContentEntity.Members.Count > 0 )) {

						bandMembers.IsContentAvailable = true;

						bandMembers.Items = new string[response.ContentEntity.Members.Count];
						response.ContentEntity.Members.CopyTo( bandMembers.Items );

					}
					else {
						bandMembers.IsContentAvailable = false;
					}

					Database.Database.Store( bandMembers );

					var releases = from DbDiscographyRelease release in mDatabase.Database where release.AssociatedItem == artistId select release;
					foreach( var release in releases ) {
						mDatabase.Database.Delete( release );
					}

					var releaseAdded = false;
					foreach( var release in response.ContentEntity.ReleaseList ) {
						AddRelease( artistId, release.Id );

						releaseAdded = true;
					}
					if(!releaseAdded ) {
						Database.Database.Store( new DbDiscographyRelease( artistId, "", "", "", Constants.cUnknownYear ));
					}

					Log.LogInfo( String.Format( "Discogs updated artist: {0}", forArtist.Name ));
				}
				else {
					Log.LogMessage( String.Format( "Discogs: {0}", response.StatusCode ));
				}
			}
			catch( Exception ex ) {
				Log.LogException( "Discogs Provider: ", ex );
			}
		}

		private void AddRelease( long artistId, string releaseId ) {
			var	request = new RestRequest { VersionPath = "release", Path = releaseId, Deserializer = new ReleaseDeserializer( artistId ) };
			var response = mClient.Request<DbDiscographyRelease>( request );

			if(( response.StatusCode == HttpStatusCode.OK ) &&
			   ( response.ContentEntity != null )) {
				response.ContentEntity.IsContentAvailable = true;
				mDatabase.Database.Store( response.ContentEntity );
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
