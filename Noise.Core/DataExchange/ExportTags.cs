using System;
using System.Xml.Linq;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.DataExchange {
	public class ExportTags : BaseExporter {
        private readonly IUserTagManager            mTagManager;
		private readonly IArtistProvider	        mArtistProvider;
		private readonly IAlbumProvider		        mAlbumProvider;
		private readonly ITrackProvider		        mTrackProvider;
		private readonly INoiseLog			        mLog;

		public ExportTags( IUserTagManager tagManager, IArtistProvider artistProvider, IAlbumProvider albumProvider, ITrackProvider trackProvider, INoiseLog log ) {
            mTagManager = tagManager;
			mArtistProvider = artistProvider;
			mAlbumProvider = albumProvider;
			mTrackProvider = trackProvider;
			mLog = log;
		}

		public override bool Export( string fileName ) {
			var retValue = ValidateExportPath( fileName );

			if( retValue ) {
				try {
					var	doc = new XDocument( new XDeclaration( ExchangeConstants.cXmlDeclVersion, ExchangeConstants.cXmlDeclEncoding, ExchangeConstants.cXmlDeclStandalone ),
											 new XComment( $"Noise Music System - Tags Export - {DateTime.Now.ToShortDateString()}" ));
					var rootElement = new XElement( ExchangeConstants.cTagsList );
                    var tagList = mTagManager.GetUserTagList();

                    foreach( var tag in tagList ) {
                        var tagElement = new XElement( ExchangeConstants.cUserTag );
                        var associations = mTagManager.GetAssociations( tag.DbId );

                        tagElement.Add( new XAttribute( ExchangeConstants.cUserTagName, tag.Name ?? "Unknown Tag" ));
                        tagElement.Add( new XAttribute( ExchangeConstants.cUserTagDescription, tag.Description ?? String.Empty ));

                        foreach( var association in associations ) {
                            var track = mTrackProvider.GetTrack( association.ArtistId );

							if( track != null ) {
                                var artist = mArtistProvider.GetArtist( track.Artist );
                                var album = mAlbumProvider.GetAlbum( track.Album );

                                if((!String.IsNullOrWhiteSpace( artist?.Name )) &&
                                   (!String.IsNullOrWhiteSpace( album?.Name )) &&
                                   (!String.IsNullOrWhiteSpace( track.Name ))) {
                                    tagElement.Add( new XElement( ExchangeConstants.cTagAssociation,
                                        new XElement( ExchangeConstants.cArtist, artist.Name ),
                                        new XElement( ExchangeConstants.cAlbum, album.Name ),
                                        new XElement( ExchangeConstants.cTrack, track.Name )));
                                }
                            }
                        }

                        rootElement.Add( tagElement );
                    }

					doc.Add( rootElement );
					doc.Save( fileName );
				}
				catch( Exception ex ) {
					mLog.LogException( "Exporting user tags", ex );

					retValue = false;
				}
			}

			return( retValue );
		}
	}
}
