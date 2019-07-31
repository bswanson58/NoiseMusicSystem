using System;
using System.IO;
using System.Xml.Linq;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.DataExchange {
	internal enum eExchangeType {
		Favorites,
		Streams,
        UserTags
	}

	internal class DataExchangeManager : IDataExchangeManager {
		private readonly IArtistProvider			mArtistProvider;
		private readonly IAlbumProvider				mAlbumProvider;
		private readonly ITrackProvider				mTrackProvider;
		private readonly IInternetStreamProvider	mStreamProvider;
        private readonly IUserTagManager            mTagManager;
        private readonly ITagProvider               mTagProvider;
        private readonly ITagAssociationProvider    mAssociationProvider;
		private readonly IRatings					mRatings;
		private readonly INoiseLog					mLog;

		public DataExchangeManager( IArtistProvider artistProvider, IAlbumProvider albumProvider, ITrackProvider trackProvider, IInternetStreamProvider streamProvider,
									IUserTagManager tagManager, ITagProvider tagProvider, ITagAssociationProvider associationProvider, IRatings ratings, INoiseLog log ) {
			mArtistProvider = artistProvider;
			mAlbumProvider = albumProvider;
			mTrackProvider = trackProvider;
			mStreamProvider = streamProvider;
            mTagManager = tagManager;
            mTagProvider = tagProvider;
            mAssociationProvider = associationProvider;
			mRatings = ratings;
			mLog = log;
		}

		public bool ExportFavorites( string fileName ) {
			return( Export( eExchangeType.Favorites, fileName ));
		}

		public bool ExportStreams( string fileName ) {
			return( Export( eExchangeType.Streams, fileName ));
		}

        public bool ExportUserTags( string fileName ) {
            return Export( eExchangeType.UserTags, fileName );
        }

		private bool Export( eExchangeType exportType, string fileName ) {
			var retValue = false;

            try {
                var	exporter = CreateExporter( exportType );

                if( exporter != null ) {
                    retValue = exporter.Export( fileName );
                }
            }
            catch( Exception ex ) {
                mLog.LogException( $"Exporting {exportType} to '{fileName}'", ex );
            }

			return( retValue );
		}

		public int Import( string fileName, bool eliminateDuplicates ) {
			var retValue = 0;

            try {
                if((!string.IsNullOrWhiteSpace( fileName )) &&
                   ( File.Exists( fileName ))) {
                    var importDoc = XDocument.Load( fileName );

                    foreach( var listNode in importDoc.Elements()) {
                        if( listNode.Name.LocalName.Equals( ExchangeConstants.cStreamList )) {
                            retValue += Import( eExchangeType.Streams, listNode, eliminateDuplicates  );
                        }
                        else if( listNode.Name.LocalName.Equals( ExchangeConstants.cFavoriteList )) {
                            retValue += Import( eExchangeType.Favorites, listNode, eliminateDuplicates  );
                        }
                        else if( listNode.Name.LocalName.Equals( ExchangeConstants.cTagsList )) {
                            retValue += Import( eExchangeType.UserTags, listNode, eliminateDuplicates  );
                        }
                    }
                }
            }
            catch( Exception ex ) {
                mLog.LogException( $"Importing '{fileName}'", ex );
            }

			return( retValue );
		}

		private int Import( eExchangeType importType, XElement rootNode, bool eliminateDuplicates ) {
			var retValue = 0;

            var importer = CreateImporter( importType );

            if( importer != null ) {
                retValue = importer.Import( rootNode, eliminateDuplicates );
            }

			return( retValue );
		}

		private IDataExport CreateExporter( eExchangeType exportType ) {
			IDataExport	retValue = null;

			switch( exportType ) {
				case eExchangeType.Favorites:
					retValue = new ExportFavorites( mArtistProvider, mAlbumProvider, mTrackProvider, mLog );
					break;

				case eExchangeType.Streams:
					retValue = new ExportStreams( mStreamProvider, mLog );
					break;

                case eExchangeType.UserTags:
                    retValue = new ExportTags( mTagManager, mArtistProvider, mAlbumProvider, mTrackProvider, mLog );
                    break;
			}

			return( retValue );
		}

		private IDataImport CreateImporter( eExchangeType importType ) {
			IDataImport	retValue = null;

			switch( importType ) {
				case eExchangeType.Favorites:
					retValue = new ImportFavorites( mArtistProvider, mAlbumProvider, mTrackProvider, mRatings );
					break;

				case eExchangeType.Streams:
					retValue = new ImportStreams( mStreamProvider );
					break;

                case eExchangeType.UserTags:
                    retValue = new ImportTags( mTagManager, mTagProvider, mAssociationProvider, mArtistProvider, mAlbumProvider, mTrackProvider );
                    break;
			}

			return( retValue );
		}
	}
}
