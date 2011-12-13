using System;
using System.Xml.Linq;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.DataExchange {
	public class ExportFavorites : BaseExporter {
		private readonly IDataProvider	mDataProvider;

		public ExportFavorites( IDataProvider dataProvider ) {
			mDataProvider = dataProvider;
		}

		public override bool Export( string fileName ) {
			var retValue = ValidateExportPath( fileName );

			if( retValue ) {
				try {
					var	doc = new XDocument( new XDeclaration( ExchangeConstants.cXmlDeclVersion, ExchangeConstants.cXmlDeclEncoding, ExchangeConstants.cXmlDeclStandalone ),
											 new XComment( string.Format( "Noise Music System - Favorites Export - {0}", DateTime.Now.ToShortDateString())));
					var rootElement = new XElement( ExchangeConstants.cFavoriteList );
				
					using( var artistList = mDataProvider.GetFavoriteArtists()) {
						foreach( var artist in artistList.List ) {
							rootElement.Add( new XElement( ExchangeConstants.cFavoriteItem, new XElement( ExchangeConstants.cArtist, artist.Name )));
						}
					}

					using( var albumList = mDataProvider.GetFavoriteAlbums()) {
						foreach( var album in albumList.List ) {
							var artist = mDataProvider.GetArtist( album.Artist );

							if( artist != null ) {
								rootElement.Add( new XElement( ExchangeConstants.cFavoriteItem,
																new XElement( ExchangeConstants.cArtist, artist.Name ),
																new XElement( ExchangeConstants.cAlbum, album.Name )));
							}
						}
					}

					using( var trackList = mDataProvider.GetFavoriteTracks()) {
						foreach( var track in trackList.List ) {
							var album = mDataProvider.GetAlbum( track.Album );

							if( album != null ) {
								var artist = mDataProvider.GetArtist( album.Artist );

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
