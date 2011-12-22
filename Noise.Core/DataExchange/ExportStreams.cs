using System;
using System.Xml.Linq;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.DataExchange {
	public class ExportStreams : BaseExporter {
		private readonly IInternetStreamProvider	mStreamProvider;

		public ExportStreams( IInternetStreamProvider streamProvider ) {
			mStreamProvider = streamProvider;
		}

		public override bool Export( string fileName ) {
			var retValue = ValidateExportPath( fileName );

			if( retValue ) {
				try {
					var	doc = new XDocument( new XDeclaration( ExchangeConstants.cXmlDeclVersion, ExchangeConstants.cXmlDeclEncoding, ExchangeConstants.cXmlDeclStandalone ),
											 new XComment( string.Format( "Noise Music System - Stream Export - {0}", DateTime.Now.ToShortDateString())));
					var rootElement = new XElement( ExchangeConstants.cStreamList );
				
					using( var streamList = mStreamProvider.GetStreamList()) {
						foreach( var stream in streamList.List ) {
							rootElement.Add( new XElement( ExchangeConstants.cStreamItem, 
													new XElement( ExchangeConstants.cName, stream.Name ),
													new XElement( ExchangeConstants.cDescription, stream.Description ),
													new XElement( ExchangeConstants.cStreamUrl, stream.Url ),
													new XElement( ExchangeConstants.cStreamPlaylist, stream.IsPlaylistWrapped ),
													new XElement( ExchangeConstants.cWebsite, stream.Website ),
													new XElement( ExchangeConstants.cGenre, stream.Genre ),
													new XElement( ExchangeConstants.cRating, stream.Rating ),
													new XElement( ExchangeConstants.cIsFavorite, stream.IsFavorite )));
						}
					}

					doc.Add( rootElement );
					doc.Save( fileName );
				}
				catch( Exception ex ) {
					NoiseLogger.Current.LogException( "Exception - ExportStreams.Export: ", ex );

					retValue = false;
				}
			}

			return( retValue );
		}
	}
}
