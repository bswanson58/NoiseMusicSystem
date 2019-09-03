using System;
using System.Collections.Generic;
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
                    var files = GetMetadataFiles( artistName );

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
                catch( Exception ex ) {
                    mLog.LogException( "ArtistArtworkSelector:SelectArtwork", ex );
                }
            }
        }

        public IEnumerable<Artwork> ArtworkPortfolio( string artistName ) {
            var retValue = new List<Artwork>();

            if(!string.IsNullOrWhiteSpace( artistName )) {
                try {
                    var files = GetMetadataFiles( artistName );

                    foreach( var file in files ) {
                        var fileInfo = new FileInfo( file );

                        if( fileInfo.Length > 100 ) {
                            var artwork = new Artwork( new DbArtwork( Constants.cDatabaseNullOid, ContentType.ArtistPrimaryImage ) { Name = Path.GetFileName( file )});

                            artwork.Image = File.ReadAllBytes( file );
                            retValue.Add( artwork );
                        }
                    }
                }
                catch( Exception ex ) {
                    mLog.LogException( "ArtistArtworkSelector:ArtistPortfolio", ex );
                }
            }

            return retValue;
        }

        public bool ArtistPortfolioAvailable( string forArtist ) {
            return GetMetadataFiles( forArtist ).Any();
        }

        private string[] GetMetadataFiles( string forArtist ) {
            var retValue = new string[0];
            var artist = mArtistProvider.FindArtist( forArtist );

            if( artist != null ) {
                var artistPath = mFolderSupport.GetArtistPath( artist.DbId );

                if( Directory.Exists( artistPath )) {
                    var artworkPath = Path.Combine( artistPath, Constants.LibraryMetadataFolder );

                    if( Directory.Exists( artworkPath )) {
                        retValue = Directory.GetFiles( artworkPath );
                    }
                }
            }

            return retValue;
        }
    }
}
