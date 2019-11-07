using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using ArchiveLoader.Interfaces;

namespace ArchiveLoader.Models {
    class FileMetadata : IFileMetadata {
        public const string     cArtistMetadataTag = "{artist}";
        public const string     cAlbumMetadataTag = "{album}";
        public const string     cTrackMetadataTag = "{track}";
        public const string     cTrackNumberMetadataTag = "{number}";
        public const string     cPublishedMetadataTag = "{year}";

        private readonly List<Regex>			mDatePatterns;
        private readonly List<Regex>			mTrackNamePatterns;

        public FileMetadata() {
            mDatePatterns = new List<Regex>{
                new Regex( "(?<month>0?[1-9]|1[012]) [- .] (?<day>0?[1-9]|[12][0-9]|3[01]) [- .] (?<year>[0-9]{2,})", RegexOptions.IgnorePatternWhitespace ),
                new Regex( "(?<year1>[0-9]{4})-(?<year>[0-9]{4})" ),
                new Regex( "(?<year1>[0-9]{2})-(?<year>[0-9]{2})" ),
                new Regex( "'(?<year1>[0-9]{2})-'(?<year>[0-9]{2})" ),
                new Regex( "(?<year>[0-9]{4})" ),
                new Regex( "'(?<year>[0-9]{2})" )
            };

            mTrackNamePatterns = new List<Regex>{
                new Regex( @"\(*(?<index>\d{1,4}) *[\).-]* *(?<trackName>.+)" ) // 01 - trackname / 01. trackname / 01 trackname / (01) trackname
            };
        }

        public string GetTrackNameFromFileName( string fileName ) {
            var retValue = string.Empty;
            var	trackName = Path.GetFileNameWithoutExtension( fileName );

            if( !string.IsNullOrWhiteSpace( trackName )) {
                foreach( var regex in mTrackNamePatterns ) {
                    var match = regex.Match( trackName );

                    if( match.Success ) {
                        retValue = match.Groups["trackName"].Captures[0].Value;

                        break;
                    }
                }
            }

            return( retValue );
        }

        public int GetTrackNumberFromFileName( string fileName ) {
            var retValue = 0;
            var	trackName = Path.GetFileNameWithoutExtension( fileName );

            if( !string.IsNullOrWhiteSpace( trackName )) {
                foreach( var regex in mTrackNamePatterns ) {
                    var match = regex.Match( trackName );

                    if( match.Success ) {
                        var indexString = match.Groups["index"].Captures[0].Value;

                        if( Int16.TryParse( indexString, out var value )) {
                            retValue = value;
                        }

                        break;
                    }
                }
            }

            return( retValue );
        }

        public string GetAlbumNameFromAlbum( string album ) {
            var retValue = album;

            if(!String.IsNullOrWhiteSpace( album )) {
                var split = album.LastIndexOf( " - ", StringComparison.CurrentCulture );

                if( split > 0 ) {
                    retValue = album.Substring( 0, split ).Trim();
                }
            }

            return retValue;
        }

        public int GetPublishedYearFromAlbum( string albumName ) {
            var retValue = 0;

            foreach( var regex in mDatePatterns ) {
                var	match = regex.Match( albumName );

                if( match.Success ) {
                    var year = Convert.ToUInt16( match.Groups["year"].Captures[0].Value );

                    if( year < 30 ) {
                        year += 2000;
                    }
                    else if( year < 100 ) {
                        year += 1900;
                    }

                    if(( year >= 1930 ) &&
                       ( year <= DateTime.Now.Year )) {
                        retValue = year;
                    }

                    break;
                }
            }

            return retValue;
        }
    }
}
