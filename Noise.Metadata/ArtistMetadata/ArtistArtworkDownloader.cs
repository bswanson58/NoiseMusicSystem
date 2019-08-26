using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;
using Noise.Metadata.Logging;
using Noise.Metadata.MetadataProviders.Discogs.Rto;
using Noise.Metadata.Support;

namespace Noise.Metadata.ArtistMetadata {
	internal class ArtistArtworkDownloader {
		private readonly IArtistProvider		mArtistProvider;
		private readonly IStorageFolderSupport	mFolderSupport;
		private readonly ILogMetadata			mLog;

		public ArtistArtworkDownloader( IArtistProvider artistProvider, IStorageFolderSupport folderSupport, ILogMetadata log ) {
			mArtistProvider = artistProvider;
			mFolderSupport = folderSupport;
			mLog = log;
		}

		public void DownloadArtwork( string artistName, IEnumerable<DiscogsImage> imageList, string providerKey ) {
			if((!string.IsNullOrWhiteSpace( artistName )) &&
			   ( imageList != null )) {
				var artist = mArtistProvider.FindArtist( artistName );

				if( artist != null ) {
					var artistPath = mFolderSupport.GetArtistPath( artist.DbId );

					if( Directory.Exists( artistPath )) {
						var artworkPath = Path.Combine( artistPath, Constants.LibraryMetadataFolder );

						if(!Directory.Exists( artworkPath )) {
							Directory.CreateDirectory( artworkPath );
						}

						DownloadArtworkList( from i in imageList select i.Resource_Url, artworkPath, artistName, providerKey );
					}
				}
			}
		}

		private async void DownloadArtworkList( IEnumerable<string> artworkList, string artworkPath, string artistName, string providerKey ) {
			var downloadList = BuildDownloadList( artworkList, artworkPath ).ToList();

			if( downloadList.Any()) {
				try {
					var resultsList = await UriDownloader.DownloadFileListAsync( downloadList );

					foreach( var result in resultsList ) {
						if( result != null ) {
                            InsureArtworkIsValid( result.Item2 );

                            if( result.Item3 != null ) {
                                mLog.LogException( $"Artwork download failed for artist '{artistName}'", result.Item3 );
                            }
						}
					}

					mLog.DownloadedArtwork( providerKey, artistName, ( from r in resultsList where (( r != null ) && ( r.Item3 == null )) select r ).Count());
				}
				catch( Exception ex ) {
					mLog.LogException( "DownloadArtworkList" ,ex );
				}
			}
		}

        private void InsureArtworkIsValid( string path ) {
            if( File.Exists( path )) {
                var info = new FileInfo( path );

                if( info.Length < 100 ) {
                    File.Delete( path );

                    mLog.LogException( $"Artwork download failed for file:'{path}'", null );
                }
            }
        }

		private IEnumerable<DownloadFileDetails> BuildDownloadList( IEnumerable<string> artworkList, string artworkPath ) {
			var downloadList = from item in artworkList select new DownloadFileDetails( item, ComposeLocalNameFromUri( item, artworkPath ));
			var retValue = from item in downloadList where !File.Exists( item.LocalPath ) select item;

			return( retValue.Take( 3 ));
		}

		private string ComposeLocalNameFromUri( string uri, string basePath ) {
			var fileName = Path.GetFileName( uri ) ?? "unamed";

			return( Path.Combine( basePath, fileName ));
		}
	}
}
