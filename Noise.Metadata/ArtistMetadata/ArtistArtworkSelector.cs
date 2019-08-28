using System;
using System.IO;
using System.Linq;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Metadata.Interfaces;

namespace Noise.Metadata.ArtistMetadata {
    class ArtistArtworkSelector : IArtistArtworkSelector {
        private readonly IArtistProvider		mArtistProvider;
        private readonly INoiseLog              mLog;
        private readonly IStorageFolderSupport	mFolderSupport;
        private readonly Random                 mRandom;

        public ArtistArtworkSelector( IArtistProvider artistProvider, IStorageFolderSupport folderSupport, INoiseLog log ) {
            mArtistProvider = artistProvider;
            mFolderSupport = folderSupport;
            mLog = log;

            mRandom = new Random( DateTime.Now.Millisecond );
        }

        public void SelectArtwork( string artistName, Artwork forArtist ) {
            if(!string.IsNullOrWhiteSpace( artistName )) {
                try {
                    var artist = mArtistProvider.FindArtist( artistName );

                    if( artist != null ) {
                        var artistPath = mFolderSupport.GetArtistPath( artist.DbId );

                        if( Directory.Exists( artistPath )) {
                            var artworkPath = Path.Combine( artistPath, Constants.LibraryMetadataFolder );

                            if( Directory.Exists( artworkPath )) {
                                var files = Directory.GetFiles( artworkPath );

                                if( files.Any()) {
                                    var circuitBreaker = 5;

                                    do {
                                        var fileToPick = mRandom.Next( files.Length );

                                        forArtist.Image = File.ReadAllBytes( files[fileToPick]);
                                        circuitBreaker--;
                                    } while((!forArtist.HaveValidImage ) &&
                                            ( circuitBreaker >= 0 ));
                                }
                            }
                        }
                    }
                }
                catch( Exception ex ) {
                    mLog.LogException( "SelectArtwork", ex );
                }
            }
        }
    }
}
