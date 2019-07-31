using System.Linq;
using System.Xml.Linq;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.DataExchange {
	internal class ImportTags : IDataImport {
        private readonly IUserTagManager            mTagManager;
        private readonly ITagProvider               mTagProvider;
        private readonly ITagAssociationProvider    mAssociationProvider;
		private readonly IArtistProvider	        mArtistProvider;
		private readonly IAlbumProvider		        mAlbumProvider;
		private readonly ITrackProvider		        mTrackProvider;

		public ImportTags( IUserTagManager tagManager, ITagProvider tagProvider, ITagAssociationProvider associationProvider,
                           IArtistProvider artistProvider, IAlbumProvider albumProvider, ITrackProvider trackProvider ) {
            mTagManager = tagManager;
            mTagProvider = tagProvider;
            mAssociationProvider = associationProvider;
			mArtistProvider = artistProvider;
			mAlbumProvider = albumProvider;
			mTrackProvider = trackProvider;
		}

		public int Import( XElement rootElement, bool eliminateDuplicates ) {
			var retValue = 0;
            var tagList = mTagManager.GetUserTagList().ToList();
			var importList = ( from element in rootElement.Descendants( ExchangeConstants.cUserTag ) select element ).ToList();

            foreach( var importItem in importList ) {
                var tagName = importItem.Attribute( ExchangeConstants.cUserTagName )?.Value;
                var tagDescription = importItem.Attribute( ExchangeConstants.cUserTagDescription )?.Value;

                if(!string.IsNullOrWhiteSpace( tagName )) {
                    var tag = tagList.FirstOrDefault( t => t.Name.Equals( tagName ));

                    if( tag == null ) {
                        tag = new DbTag( eTagGroup.User, tagName ) { Description = tagDescription };

                        mTagProvider.AddTag( tag );
                        tagList.Add( tag );
                    }

                    var importTagList = from element in importItem.Descendants( ExchangeConstants.cTagAssociation ) select element;

                    foreach( var importTag in importTagList ) {
                        var artistName = importTag.Element( ExchangeConstants.cArtist )?.Value;
                        var albumName = importTag.Element( ExchangeConstants.cAlbum )?.Value;
                        var trackName = importTag.Element( ExchangeConstants.cTrack )?.Value;

                        if((!string.IsNullOrWhiteSpace( artistName )) &&
                           (!string.IsNullOrWhiteSpace( albumName )) &&
                           (!string.IsNullOrWhiteSpace( trackName ))) {
                            var artist = mArtistProvider.FindArtist( artistName );

                            if( artist != null ) {
                                using( var albumList = mAlbumProvider.GetAlbumList( artist )) {
                                    var album = albumList.List.FirstOrDefault( a => a.Name.Equals( albumName ));

                                    if( album != null ) {
                                        using( var trackList = mTrackProvider.GetTrackList( album )) {
                                            var track = trackList.List.FirstOrDefault( t => t.Name.Equals( trackName ));

                                            if( track != null ) {
                                                var tags = mTagManager.GetAssociations( tag.DbId );

                                                var currentTag = tags.FirstOrDefault( t => t.ArtistId.Equals( track.DbId ));

                                                if( currentTag == null ) {
                                                    mAssociationProvider.AddAssociation( new DbTagAssociation( eTagGroup.User, tag.DbId, track.DbId, Constants.cDatabaseNullOid ));

                                                    retValue++;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

			return( retValue );
		}
	}
}
