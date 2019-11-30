using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using TuneRenamer.Interfaces;
using TuneRenamer.Platform;

namespace TuneRenamer.Models {
    class TextHelpers : ITextHelpers {
        private readonly IFileTypes     mFileTypes;

        public TextHelpers( IFileTypes fileTypes ) {
            mFileTypes = fileTypes;
        }

        public string GetCommonSubstring( string text ) {
            var strings = text.Split( new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None );

            return strings.GetLongestCommonSubstring();
        }

        public IEnumerable<string> Lines( string text ) {
            var split = text.Split( new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None );

            return from l in split where !String.IsNullOrWhiteSpace( l ) select l;
        }

        public int LineCount( string text ) {
            return Lines( text ).Count();
        }

        public string CleanText( string text, int defaultIndex ) {
            var extension = DetermineExtension( ref text ).ToLower( CultureInfo.CurrentCulture );
            var index = DetermineTrackIndex( ref text, defaultIndex );
            var name = ReplaceCharSets( text );

            name = PathSanitizer.SanitizeFilename( name, ' ' ).Trim();

            return $"{index:D2} - {name}{extension}";
        }

        public string DetermineExtension( ref string text ) {
            var retValue = String.Empty;

            if((!String.IsNullOrWhiteSpace( text )) &&
               ( mFileTypes.ItemIsMusicFile( PathSanitizer.SanitizeFilename( text, ' ' )))) {
                var lastPeriod = text.LastIndexOf( ".", StringComparison.CurrentCulture );

                if(( lastPeriod > 0 ) &&
                  (( text.Length - lastPeriod ) < 6 )) {
                    retValue = text.Substring( lastPeriod, text.Length - lastPeriod );
                    text  = text.Substring( 0, lastPeriod );
                }
            }

            return retValue;
        }

        public int DetermineTrackIndex( ref string text, int defaultIndex ) {
            var retValue = defaultIndex;
            var pattern = new Regex( @"[ ([]*(?<index>[0-9]+)" );
            var match = pattern.Match( text );

            if( match.Success ) {
                var indexStr = match.Groups["index"];

                if( Int16.TryParse( indexStr.Value, out var index )) {
                    retValue = index;

                    // clean up the returned string.
                    var startIndex = text.IndexOf( indexStr.Value, StringComparison.CurrentCulture );
                    text = text.Remove( 0, startIndex + indexStr.Length );
                    text = text.TrimStart( ' ', '"', '-', '.', ')', ']' );
                }
            }

            return retValue;
        }

        private string ReplaceCharSets( string text ) {
            var retValue = text;

            retValue = retValue.Replace( "_",   " " );
            retValue = retValue.Replace( " > ", " - " );
            retValue = retValue.Replace( " >",  " - " );
            retValue = retValue.Replace( "> ",  " - " );
            retValue = retValue.Replace( ">",   " - " );

            while( retValue.IndexOf( "  ", StringComparison.CurrentCulture ) >= 0 ) {
                retValue = retValue.Replace( "  ",  " " );
            }

            retValue = retValue.Trim( ' ', '-' );

            return retValue;
        }
    }

    // from: https://stackoverflow.com/questions/21797599/how-can-i-find-lcs-length-between-two-large-strings/21797687
    static class StringArrayEx {
        public static string GetLongestCommonSubstring( this string[] strings ) {
            var commonSubstrings = new HashSet<string>(strings[0].GetSubstrings());

            foreach ( string str in strings.Skip( 1 ) ) {
                commonSubstrings.IntersectWith( str.GetSubstrings());
                if ( commonSubstrings.Count == 0 )
                    return string.Empty;
            }

            return commonSubstrings.OrderByDescending( s => s.Length ).DefaultIfEmpty( string.Empty ).First();
        }

        private static IEnumerable<string> GetSubstrings( this string str ) {
            for ( int c = 0; c < str.Length - 1; c++ ) {
                for ( int cc = 1; c + cc <= str.Length; cc++ ) {
                    yield return str.Substring( c, cc );
                }
            }
        }
    }
}
