using System;
using System.Xml.Linq;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.DataExchange {
	public class ExportFavorites : BaseExporter {
		private readonly IUnityContainer	mContainer;
		private readonly INoiseManager		mNoiseManager;

		public ExportFavorites( IUnityContainer container ) {
			mContainer = container;
			mNoiseManager = mContainer.Resolve<INoiseManager>();
		}

		public override bool Export( string fileName ) {
			var retValue = ValidateExportPath( fileName );

			if( retValue ) {
				try {
					var	doc = new XDocument( new XDeclaration( ExchangeConstants.cXmlDeclVersion, ExchangeConstants.cXmlDeclEncoding, ExchangeConstants.cXmlDeclStandalone ),
											 new XComment( string.Format( "Noise Music System - Favorites Export - {0}", DateTime.Now.ToShortDateString())));
					var rootElement = new XElement( ExchangeConstants.cFavoriteList );
				
					using( var artistList = mNoiseManager.DataProvider.GetFavoriteArtists()) {
						foreach( var artist in artistList.List ) {
							rootElement.Add( new XElement( ExchangeConstants.cFavoriteItem, new XElement( ExchangeConstants.cArtist, artist.Name )));
						}
					}

					using( var albumList = mNoiseManager.DataProvider.GetFavoriteAlbums()) {
						foreach( var album in albumList.List ) {
							var artist = mNoiseManager.DataProvider.GetArtist( album.Artist );

							if( artist != null ) {
								rootElement.Add( new XElement( ExchangeConstants.cFavoriteItem,
																new XElement( ExchangeConstants.cArtist, artist.Name ),
																new XElement( ExchangeConstants.cAlbum, album.Name )));
							}
						}
					}

					using( var trackList = mNoiseManager.DataProvider.GetFavoriteTracks()) {
						foreach( var track in trackList.List ) {
							var album = mNoiseManager.DataProvider.GetAlbum( track.Album );

							if( album != null ) {
								var artist = mNoiseManager.DataProvider.GetArtist( album.Artist );

								if( artist != null ) {
								rootElement.Add( new XElement( ExchangeConstants.cFavoriteItem,
																new XElement( ExchangeConstants.cArtist, artist.Name ),
																new XElement( ExchangeConstants.cAlbum, album.Name ),
																new XElement( ExchangeConstants.cTrack, track.Name )));
								}
							}
						}
					}

					doc.Add( rootElement );
					doc.Save( fileName );
				}
				catch( Exception ex ) {
					NoiseLogger.Current.LogException( "Exception - ExportFavorites.Export: ", ex );

					retValue = false;
				}
			}

			return( retValue );
		}
	}
}
